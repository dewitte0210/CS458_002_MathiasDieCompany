/*
 * Abstract class to be inherrited by every File child class
 * - DWG, DXF, PDF
 */
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;
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
        protected List<Feature> featureList;
        protected List<Entity> entityList;
        protected List<FeatureGroup> featureGroups;
        //protected keyword for nested enum is about granting 
        protected SupportedFile()
        {
            entityList = new List<Entity>();
            featureList = new List<Feature>();
        }
        public SupportedFile(string path)
        {
            this.path = path;
            entityList = new List<Entity>();
            featureList = new List<Feature>();
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
        public void setFeatureList(List<Feature> featureList)
        {
            this.featureList = featureList;
        }
        
        public List<Feature> getFeatureList(List<List<Entity>> entities)
        {
            List<Feature> featureList = new List<Feature>();

            for (int i = 0; i < entities.Count(); i++)
            {
                Feature feature = new Feature(entities[i]);
                feature.extendAllEntities();
                feature.sortExtendedLines();
                feature.DetectFeatures();
                featureList.Add(feature);
                if (feature.PerimeterEntityList != null)
                {
                    for (int j = 0; j < feature.PerimeterEntityList.Count(); j++)
                    {
                        Feature newFeat = new Feature(feature.PerimeterEntityList[j]);
                        featureList.Add(newFeat);
                    }
                }
            }


            // Group identical features together
            for (int i = 0; i < featureList.Count(); i++)
            {
                for (int j = i + 1; j < featureList.Count(); j++)
                {
                    if (featureList[i].Equals(featureList[j]))
                    {
                        featureList[i].count += featureList[j].count;
                        featureList.RemoveAt(j);
                        j--;
                    }
                }
            }


            return featureList;
        }
        public List<Feature> getFeatureList() { return featureList; }

        /**
         * Creates and returns a list of features that are made up of touching entities in another list.
         * @Param entityList - the list of entites in the file
         */
        public List<List<Entity>> makeTouchingEntitiesList(List<Entity> entityList)
        {
            List<Entity> myEntityList = new List<Entity>(entityList);
            //  Return list of features
            List<List<Entity>> touchingEntityList = new List<List<Entity>>();
            //  myEntityList is modified in the process, so it will eventually be empty
            while (myEntityList.Count > 0)
            {
                //  List of entities that are touching
                List<Entity> features = new List<Entity>();
                //  Starting entity to check for touching entities
                Entity temp = myEntityList[0];
                features.Add(temp);
                myEntityList.RemoveAt(0);
                scanThroughEntityList(temp, myEntityList, features);
                //All touching entities are found, add to return list
                touchingEntityList.Add(features);
            }
            return touchingEntityList;
        }

        /**
         * Reccurssible function to check if other entities in myEntityList are touching the current entity.
         * @Param entity - the current entity being checked
         * @Param myEntityList - the list of entities not touching another so far
         * @Param features - the list of entities currently found touching each other
         */
        void scanThroughEntityList(Entity entity, List<Entity> myEntityList, List<Entity> features)
        {
            /**
             * This will scan through the entity list and if another touching entity is found,
             * that entity is added to features and removed from the entity list. i is then downticked
             * so the scan does not skip over the next entity in the list. The function is
             * then called again recursively to check the if anything is touching the new
             * entity found. This ensures that every touching entity is found as it could be
             * scrambled in the entity list.
             * 
             * Base case 1: No touching entities are found
             * Base case 2: The entity list is empty
             */
            for (int i = 0; i < myEntityList.Count; i++)
            {
                if (entity.DoesIntersect(myEntityList[i]))
                {
                    Entity temp = myEntityList[i];
                    features.Add(myEntityList[i]);
                    myEntityList.RemoveAt(i);
                    i--;
                    scanThroughEntityList(temp, myEntityList, features);
                }
            }
        }

        /*
         * Groups features together and stores how many of each feature group are present in the file
         * Initliazes class variable featuresList
         */
        void SetFeatureGroups()
        {
            List<Feature> features = getFeatureList(makeTouchingEntitiesList(entityList));

            for (int i = 0; i < features.Count; i++)
            {
                
            }
        }

        /* 
         * method that goes from the path to detected features
        */
        public void detectAllFeatures()
        {
            detectAllFeatures(entityList);
        }
        public void detectAllFeatures(List<Entity> myEntityList)
        {
            List<List<Entity>> touchingEntities = makeTouchingEntitiesList(myEntityList);
            featureList = getFeatureList(touchingEntities);
            foreach (Feature feature in featureList)
            {
                    feature.DetectFeatures();
                    feature.extendAllEntities();
                    feature.sortExtendedLines();
            }
        }
        // Method to read the data from a file and fill the entityList with entities
        public abstract void readEntities();
    }
}
