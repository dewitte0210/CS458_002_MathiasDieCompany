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

        public async Task<List<Entity>> ReadEntities()
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
                           // double xStart, yStart, xEnd, yEnd;
                            

                           // Line lineEntity = new Line(((ACadSharp.Entities.Line)entities[i]).StartPoint.X, yStart, xEnd, yEnd);
                            break;
                        }
                    case "ARC":
                        {
                            break; 
                        }
                    case "CIRCLE":
                        {
                            break;
                        }

                } 
            }

            throw new NotImplementedException();
        }

        public override void readEntities()
        {

        }
        
    }
}
