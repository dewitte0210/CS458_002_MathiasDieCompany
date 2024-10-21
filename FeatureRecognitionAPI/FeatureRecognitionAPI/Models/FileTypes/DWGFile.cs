/*
 * Child class from SupportedFiles that handles DWG files.
 */
using FeatureRecognitionAPI.Models.Enums;
using ACadSharp;
using System;
using ACadSharp.IO;
using CSMath;
using iText.Barcodes.Qrcode;
using ACadSharp.Header;
using System.Security.Permissions;

namespace FeatureRecognitionAPI.Models
{
    public class DWGFile : SupportedFile
    {
        private FileVersion _fileVersion;
        private string _path;
        public DWGFile(string path) : base(path)
        {
            fileType = SupportedExtensions.dwg;
            _path = path;
            _fileVersion = GetFileVersion();

            entityList = new List<Entity>();
            if (_fileVersion >= FileVersion.AutoCad14)
            {
                if (File.Exists(path))
                    readEntities();
            }
            else
            {
                throw new Exception("Unsupported DWG File");
            }
            
        }

        /*
         * Finds all entities withing the file and stores them in entityList
         * Returns false if some error occurs, otherwise returns true
         */
        public bool findEntities()
        {
            //TODO
            return false;
        }

        /*
         * Finds all features in the file based of the entities within the file
         * calls findEntities to garuantee an updated entity list
         * returns false if fails, otherwise returns true
         */
        public override bool findFeatures()
        {
            if (findEntities())
            {
                //TODO
            }
            return false;
        }

        public override void readEntities()
        {
            DwgReader reader = new DwgReader(_path);
            
            CadDocument doc = reader.Read();

            CadObjectCollection<ACadSharp.Entities.Entity> entities = doc.Entities;

            for (int i = 0; i < entities.Count(); i++)
            {
                switch (entities[i].ObjectName)
                {
                    case "LINE":
                        {
                            Line lineEntity = 
                                new Line(((ACadSharp.Entities.Line)entities[i]).StartPoint.X,
                                ((ACadSharp.Entities.Line)entities[i]).StartPoint.Y,
                                ((ACadSharp.Entities.Line)entities[i]).EndPoint.X,
                                ((ACadSharp.Entities.Line)entities[i]).EndPoint.Y);
                            entityList.Add(lineEntity);
                            break;
                        }
                    case "ARC":
                        {
                            Arc arcEntity =
                                new Arc(((ACadSharp.Entities.Arc)entities[i]).Center.X,
                                ((ACadSharp.Entities.Arc)entities[i]).Center.Y,
                                ((ACadSharp.Entities.Arc)entities[i]).Radius,
                                //Start and end angle return radians, and must be converted to degrees
                                (((ACadSharp.Entities.Arc)entities[i]).StartAngle * (180/Math.PI)),
                                (((ACadSharp.Entities.Arc)entities[i]).EndAngle * (180/ Math.PI)));
                            entityList.Add(arcEntity);
                            break; 
                        }
                    case "CIRCLE":
                        {
                            Circle circleEntity =
                                new Circle(((ACadSharp.Entities.Circle)entities[i]).Center.X,
                                ((ACadSharp.Entities.Circle)entities[i]).Center.Y,
                                ((ACadSharp.Entities.Circle)entities[i]).Radius);
                            entityList.Add(circleEntity);
                            break;
                        }
                } 
            }
        }

        public FileVersion GetFileVersion()
        {
            DwgReader reader = new DwgReader(_path);
            CadHeader header = reader.ReadHeader();

            String version = header.VersionString;

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

        public List<Entity> GetEntities()
        {
            return  entityList;
        }


    }
}
