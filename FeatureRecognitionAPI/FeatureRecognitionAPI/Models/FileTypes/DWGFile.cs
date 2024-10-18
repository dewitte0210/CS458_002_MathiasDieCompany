/*
 * Child class from SupportedFiles that handles DWG files.
 */
using FeatureRecognitionAPI.Models.Enums;
using ACadSharp;
using System;
using ACadSharp.IO;
using CSMath;
using iText.Barcodes.Qrcode;

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
            entityList = new List<Entity>();
            if(File.Exists(path)) 
                readEntities();
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
                                ((ACadSharp.Entities.Arc)entities[i]).StartAngle,
                                ((ACadSharp.Entities.Arc)entities[i]).EndAngle);
                            entityList.Add(arcEntity);
                            break; 
                        }
                    case "CIRCLE":
                        {
                            Circle circleEntity =
                                new Circle(((ACadSharp.Entities.Circle)entities[i]).Center.X,
                                ((ACadSharp.Entities.Circle)entities[i]).Center.Y,
                                ((ACadSharp.Entities.Arc)entities[i]).Radius);
                            entityList.Add(circleEntity);
                            break;
                        }

                } 
            }

            throw new NotImplementedException();
        }

        
    }
}
