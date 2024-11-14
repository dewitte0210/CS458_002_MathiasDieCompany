/*
 * Abstract class to be inherrited by every File child class
 * - DWG, DXF, PDF
 */
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;
using Remotion.Linq.Clauses.ResultOperators;
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
        protected List<FeatureGroup> featureGroups;
        public int GetFeatureGroupsCount() { return featureList.Count; }
        //protected keyword for nested enum is about granting 
        public SupportedFile(string path)
        {
            this.path = path;
            featureGroups = new List<FeatureGroup>();
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
        public List<Feature> getFeatureList(List<List<Entity>> entities)
        {
            List<Feature> featureList = new List<Feature>();

            for (int i = 0; i < entities.Count(); i++)
            {
                Feature feature = new Feature(entities[i]);
                featureList.Add(feature);
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

        /**
         * Creates and returns a list of features that are made up of touching entities in another list.
         * @Param myEntityList - the list of entites in the file
         */
        public List<List<Entity>> makeTouchingEntitiesList(List<Entity> myEntityList)
        {
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
        public void SetFeatureGroups()
        {
            List<Feature> features = getFeatureList(makeTouchingEntitiesList(entityList));

            //First iteration of loop (Declaring variables outside loop)
            Point minPoint = features[0].FindMinPoint();
            Point maxPoint = features[0].FindMaxPoint();
            Point maxDiff = new((maxPoint.X-minPoint.X), (maxPoint.Y-minPoint.Y));
            int maxDiffIndex = 0;

            //Temp variables to overwrite
            Point tempDiff = new(0, 0);
            Point tempMinPoint = minPoint;
            Point tempMaxPoint = maxPoint;

            while (features.Count > 0)
            {
                for (int i = 1; i < features.Count; i++)
                {
                    tempMinPoint = features[i].FindMinPoint();
                    tempMaxPoint = features[i].FindMaxPoint();

                    tempDiff.X = maxPoint.X - minPoint.X;
                    tempDiff.Y = maxPoint.Y - minPoint.Y;
                    if (tempDiff.X > maxDiff.X && tempDiff.Y > maxDiff.Y)
                    {
                        maxPoint = tempMaxPoint;
                        minPoint = tempMinPoint;
                        maxDiff = tempDiff;
                        maxDiffIndex = i;
                    }
                }

                //Start the list
                
                Feature bigFeature = features[maxDiffIndex];
                features.RemoveAt(maxDiffIndex);

                List<Feature> featureGroupList = new List<Feature>();
                featureGroupList.Add(bigFeature);

                for (int i = 0; i < features.Count; i++)
                {
                    tempMaxPoint = features[i].FindMaxPoint();
                    tempMinPoint = features[i].FindMinPoint();
                    //Temp max should be less than maxPoint (if it's the same it also shouldn't be added)
                    if (tempMaxPoint.X < maxPoint.X && tempMaxPoint.Y < maxPoint.Y
                        //TempMin should be greater than minPoint
                        && tempMinPoint.X > minPoint.X && tempMinPoint.Y > minPoint.Y)
                    {
                        featureGroupList.Add(features[i]);
                        features.RemoveAt(i);
                        i--;
                    }
                }
                //featureGroupList should now contain all features that fall inside of bigFeature
                bool added = false;
                FeatureGroup newfGroup = new FeatureGroup(featureGroupList);
                if (featureGroupList.Count > 0)
                {
                    foreach(FeatureGroup fGroup in featureGroups)
                    {
                        if (fGroup.Equals(newfGroup))
                        {
                            fGroup.Count++;
                            added = true;
                            break;
                        }
                    }
                    //If the foreach loop was excited without adding anything add newFGroup to the featuregroup list
                    if(!added)
                    {
                        featureGroups.Add(newfGroup);
                    }
                }
                else featureGroups.Add(newfGroup);
            }
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
