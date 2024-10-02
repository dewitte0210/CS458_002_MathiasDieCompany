/*
 * Child class from SupportedFiles that handles DWG files.
 */
using System;

namespace FeatureRecognitionAPI.Models
{
    public class DWGFile : SupportedFile
    {
        Entity[] entityList;
        public DWGFile(string path) : base(path)
        {
            fileType = SupportedExtensions.dwg;
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
            throw new NotImplementedException();
        }
    }
}
