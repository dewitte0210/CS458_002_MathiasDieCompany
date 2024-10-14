/*
 * Abstract class to be inherrited by every File child class
 * - DWG, DXF, PDF
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace FeatureRecognitionAPI.Models
{
    abstract public class SupportedFile
    {
        protected string path;
        protected SupportedExtensions fileType;
        protected List<Feature> featureList = [];
        protected List<Entity> entityList;
        protected enum SupportedExtensions
        {
            pdf,
            dwg,
            dxf,
        }
        public SupportedFile(string path)
        {
            this.path = path;
        }
        public void setPath(string path)
        {
            this.path = path;
        }
        public string getPath()
        {
            return this.path;
        }
        public string getFileType()
        {
            return fileType.ToString();
        }
        public void writeFeatures()
        {
        
        }
        public void readFeatures()
        {
        }
        public List<Feature> getFeatureList()
        {
            return featureList;
        }

        public List<List<Entity>> makeTouchingEntitiesList(List<Entity> myEntityList)
        {
            List<List<Entity>> touchingEntityList = new List<List<Entity>>();
            /*for (int i = 0; i < myEntityList.Count; i++)
            {
                for (int j = i; j < myEntityList.Count;j++)
                {

                }
            }*/

            return touchingEntityList;
        }
        public void extendEntities(List<Entity> myEntityList)
        {
            for (int i = 0; i < myEntityList.Count; i++)
            {
                for (int j = 0;j <myEntityList.Count; j++)
                {
                    if (myEntityList[i])
                }
            }
        }
        private List<Entity> makeTouchingEntitiesListHelper(List<Entity> myEntityList)
        {
            return myEntityList;
        }

        /* 
         * Method that should be implemented by each child 
         * This is where the feature recognition logic will go
        */
        abstract public bool findFeatures();
        // Method to read the data from a file and fill the entityList with entities
        public abstract void readEntities();
    }
}
