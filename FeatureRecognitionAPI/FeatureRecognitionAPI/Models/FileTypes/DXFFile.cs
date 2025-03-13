/*
 * SupportedFile child that implements a DXF instance
 * I believe this will not require the use of Entities and can instead just read the data straight into features
 */
using ACadSharp;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Utility;
using NHibernate.Id.Insert;

namespace FeatureRecognitionAPI.Models
{
    public class DXFFile : SupportedFile
    {
        private FileVersion _fileVersion;
        private string[] _lines;

        public DXFFile()
        {
        }

        public DXFFile(string path) : base(path)
        {
            fileType = SupportedExtensions.dxf;

            if (File.Exists(path))
            {
                _lines = File.ReadAllLines(path);
                _fileVersion = GetFileVersion();
                if (_fileVersion == FileVersion.AutoCad10 || _fileVersion == FileVersion.AutoCad12)
                {
                    readEntitiesOld();
                }
                else
                {
                    readEntities();
                }

            }
            else
                throw new FileNotFoundException();
        }

        public FileVersion GetFileVersion()
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                if (_lines[i] == "$ACADVER")
                {
                    string version = _lines[i + 1];
                    switch (version)
                    {
                        case "AC1006":
                            return FileVersion.AutoCad10;
                        case "AC1009":
                            return FileVersion.AutoCad12;
                        case "AC1012":
                            return FileVersion.AutoCad13;
                        case "AC1014":
                            return FileVersion.AutoCad14;
                        case "AC1015":
                            return FileVersion.AutoCad2000;
                        case "AC1018":
                            return FileVersion.AutoCad2004;
                        case "AC1021":
                            return FileVersion.AutoCad2007;
                        case "AC1024":
                            return FileVersion.AutoCad2010;
                        case "AC1027":
                            return FileVersion.AutoCad2013;
                        case "AC1032":
                            return FileVersion.AutoCad2018;
                        default:
                            return FileVersion.Unknown;
                    }
                }
            }

            return FileVersion.Unknown;
        }

        public void readEntitiesOld()
        {
            List<Entity> entityList = new List<Entity>();

            //find and track index where entities begin in file (where parsing into entities starts)
            int index = GetStartIndex(_lines);

            //While we haven't reached the end of the entities section, loop and grab entities
            while ((_lines[index] != "ENDSEC") && (_lines.Length > index))
            {

                index++;
                //Console.WriteLine("In while loop: " + lines[index]);

                switch (_lines[index])
                {
                    case "LINE":

                        index = ParseLine(_lines, index);
                        break;

                    case "ARC":
                        index = ParseArc(_lines, index);
                        break;

                    case "CIRCLE":
                        index = ParseCircle(_lines, index);
                        break;

                    default:
                        break;
                }

            }

        }

        //Ignore commented lines for Console.WriteLine* these were used in initial testing and writing (may be removed later)
        //Could be further modularlized by breaking internals of switch statements into helper functions (Future todo?)
        public override void readEntities()
        {
            DxfReader reader = new DxfReader(path);

            CadDocument doc = reader.Read();

            CadObjectCollection<ACadSharp.Entities.Entity> entities = doc.Entities;

            entityList.AddRange(UnwrapInserts(doc.Entities));
        }

        List<Entity> UnwrapInserts(CadObjectCollection<ACadSharp.Entities.Entity> entities)
        {
            List<Entity> returned = new List<Entity>();
            foreach (ACadSharp.Entities.Entity entity in entities)
            {
                if (entity is Insert insert)
                {
                    Block block = insert.Block.BlockEntity;
                    Matrix3 blockTranslate = Matrix3.Translate(block.BasePoint.X, block.BasePoint.Y);
                    Matrix3 insertTranslate = Matrix3.Translate(insert.InsertPoint.X, insert.InsertPoint.Y);
                    Matrix3 insertScale = Matrix3.Scale(insert.XScale, insert.YScale);

                    Matrix3 insertRotate = Matrix3.Rotate(insert.Rotation);

                    Matrix3 finalTransform = insertScale * blockTranslate * insertTranslate * insertRotate;
                    foreach (ACadSharp.Entities.Entity cadObject in insert.Block.Entities)
                    {
                        Entity? castedEntity = CadObjectToInternalEntity(cadObject);
                        if (!(castedEntity is null))
                        {
                            returned.Add(castedEntity.Transform(finalTransform));
                        }
                    }
                }
                else
                {
                    Entity? castedEntity = CadObjectToInternalEntity(entity);
                    if (!(castedEntity is null))
                    {
                        returned.Add(castedEntity);
                    }
                }
            }

        return returned;
    }

        internal Entity? CadObjectToInternalEntity(ACadSharp.Entities.Entity cadEntity)
        {
            switch (cadEntity)
            {
                case ACadSharp.Entities.Line line:
                {
                    return new Line(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
                }
                case ACadSharp.Entities.Arc arc:
                {
                    return new Arc(arc.Center.X, arc.Center.Y, arc.Radius,
                        arc.StartAngle * (180 / Math.PI), arc.EndAngle * (180 / Math.PI));
                }
                case ACadSharp.Entities.Circle circle:
                {
                    return new Circle(circle.Center.X, circle.Center.Y, circle.Radius);
                }
                case ACadSharp.Entities.Ellipse ellipse:
                {
                    return new Ellipse(ellipse.Center.X, ellipse.Center.Y, ellipse.EndPoint.X,
                        ellipse.EndPoint.Y,
                        ellipse.RadiusRatio, ellipse.StartParameter, ellipse.EndParameter);
                }
            }

            return null;
        }

        private int GetStartIndex(string[] lines)
            {
                int startIndex = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    // Console.WriteLine(lines[i]);

                    if (lines[i] == "ENTITIES")
                    {
                        startIndex = i;
                        break;
                    }
                }

                return startIndex;
            }

            #region Parse Entities

            private int ParseLine(string[] lines, int index)
            {
                //Variables used to create entity of type Line
                double xStart = 0;
                double yStart = 0;
                double xEnd = 0;
                double yEnd = 0;

                bool startXFlag, startYFlag, endXFlag, endYFlag;
                startXFlag = startYFlag = endXFlag = endYFlag = false;
                //Index must be incremented along with i in order to keep maintain accurate indexing
                while (lines[index] != "  0")
                {
                    //Switch based on which data field is being interpreted
                    switch (lines[index])
                    {
                        case " 10":
                            index++;
                            xStart = double.Parse(lines[index]);
                            startXFlag = true;
                            break;

                        case " 11":
                            index++;
                            xEnd = double.Parse(lines[index]);
                            endXFlag = true;
                            break;

                        case " 20":
                            index++;
                            yStart = double.Parse(lines[index]);
                            startYFlag = true;
                            break;

                        case " 21":
                            index++;
                            yEnd = double.Parse(lines[index]);
                            endYFlag = true;
                            break;

                        default:
                            break;
                    }

                    index++;
                }

                if (startYFlag && startXFlag && endXFlag && endYFlag)
                {
                    Line lineEntity = new Line(xStart, yStart, xEnd, yEnd);
                    entityList.Add(lineEntity);
                    return index;
                }
                else
                {
                    throw new Exception("Error: Issue with DXF File");
                }
            }

            private int ParseArc(string[] lines, int index)
            {
                double xPoint = 0;
                double yPoint = 0;
                double radius = 0;
                double startAngle = 0;
                double endAngle = 0;
                bool xFlag, yFlag, rFlag, startFlag, endFlag;
                xFlag = yFlag = rFlag = startFlag = endFlag = false;

                while (lines[index] != "  0")
                {
                    switch (lines[index])
                    {
                        case " 10":
                            index++;
                            xPoint = double.Parse(lines[index]);
                            xFlag = true;
                            break;

                        case " 20":
                            index++;
                            yPoint = double.Parse(lines[index]);
                            yFlag = true;
                            break;

                        case " 40":
                            index++;
                            radius = double.Parse(lines[index]);
                            rFlag = true;
                            break;

                        case " 50":
                            index++;
                            startAngle = double.Parse(lines[index]);
                            startFlag = true;
                            break;

                        case " 51":
                            index++;
                            endAngle = double.Parse(lines[index]);
                            endFlag = true;
                            break;

                        default:
                            break;
                    }

                    index++;
                }

                if (xFlag && yFlag && rFlag && startFlag && endFlag)
                {
                    Arc arcEntity = new Arc(xPoint, yPoint, radius, startAngle, endAngle);
                    entityList.Add(arcEntity);
                    return index;
                }
                else
                {
                    throw new Exception("Error: Issue with DXF File");
                }
            }

            private int ParseCircle(string[] lines, int index)
            {
                double xPoint = 0;
                double yPoint = 0;
                double radius = 0;
                bool xFlag, yFlag, rFlag;
                xFlag = yFlag = rFlag = false;
                while (lines[index] != "  0")
                {
                    switch (lines[index])
                    {
                        case " 10":
                            index++;
                            xPoint = double.Parse(lines[index]);
                            xFlag = true;
                            break;

                        case " 20":
                            index++;
                            yPoint = double.Parse(lines[index]);
                            yFlag = true;
                            break;

                        case " 40":
                            index++;
                            radius = double.Parse(lines[index]);
                            rFlag = true;
                            break;

                        default:
                            break;
                    }

                    index++;
                }

                if (xFlag && yFlag && rFlag)
                {
                    Circle circleEntity = new Circle(xPoint, yPoint, radius);
                    entityList.Add(circleEntity);
                    return index;
                }
                else
                {
                    throw new Exception("Error: Issue with DXF File");
                }
            }

            #endregion

            //May need to be refactored depending on if c# handles this by copy or by reference
            public List<Entity> GetEntities()
            {
                return entityList;
            }

            public void SetEntities(List<Entity> entities)
            {
                entityList = entities;
            }
        }
}
