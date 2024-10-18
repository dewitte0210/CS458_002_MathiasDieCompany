/*
 * Abstract class to be inherrited by every File child class
 * - DWG, DXF, PDF
 */
using FeatureRecognitionAPI.Models.Enums;
using System;
using System.Diagnostics;
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

        //protected keyword for nested enum is about granting 
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
            featureList.Add(new Feature("Group1B", false, false, true, false));
            featureList.Add(new Feature("Group1A", false, true, false, true));
            featureList.Add(new Feature("Group1C", false, false, true, false));
            featureList.Add(new Feature("Group2A", true, false, false, false));

            return featureList;
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
