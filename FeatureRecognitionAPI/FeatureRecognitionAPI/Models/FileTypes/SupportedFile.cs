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
        protected string path { get; set; }
        protected SupportedExtensions fileType { get; set; }
        protected List<Feature> featureList;
        protected List<Entity> entityList;
        protected List<FeatureGroup> featureGroups;

        //These two functiuons below exist for testing purposes
        public int GetFeatureGroupCount() { return featureGroups.Count; }
        public int GetTotalFeatureGroups()
        {
            int tmp = 0;
            foreach(FeatureGroup fGroup in featureGroups)
            {
                tmp += fGroup.Count;
            }
            return tmp;
        }
        #region Constructors
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
            featureGroups = new List<FeatureGroup>();
        }
        #endregion

        #region SpecialGettersAndSetters
        public void setFeatureList(List<Feature> featureList)
        {
            this.featureList = featureList;
        }
        
        public List<Feature> makeFeatureList(List<List<Entity>> entities)
        {
            for (int i = 0; i < entities.Count(); i++)
            {
                Feature feature = new Feature(entities[i]);
                feature.extendAllEntities();
                feature.seperateBaseEntities();
                feature.seperatePerimeterEntities();
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
        #endregion

        #region MakeTouchingEntities
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
        #endregion

        #region SetFeatureGroups
        /*
         * Groups features together and stores how many of each feature group are present in the file
         * Initliazes class variable featuresList
         */
        public void SetFeatureGroups()
        {
            List<List<Entity>> entities = makeTouchingEntitiesList(entityList);
           // List<Feature> brokenFeatures = makeFeatureList(entities);
            List<Feature> features = new List<Feature>();

            //Create features groups things in a way that breaks the logic here
            foreach (List<Entity> entityList in entities)
            {
                features.Add(new Feature(entityList));
            }


            //First iteration of loop (Declaring variables outside loop)
            Point minPoint = new(0, 0);
            Point maxPoint = new(0,0);
            Point maxDiff = new(0,0);
            int maxDiffIndex = 0;

            //Temp variables to overwrite
            Point tempDiff = new(0, 0);
            Point tempMinPoint = new(0, 0);
            Point tempMaxPoint = new(0,0);

            bool firstrun = true;
            while (features.Count > 0)
            {


                    //Set max values to zero before run, if its not the first one
                    maxDiffIndex = 0;
                    maxDiff.X = 0; maxDiff.Y = 0;
                    maxPoint.X = 0; maxDiff.X = 0;
                    minPoint.X = 0; minPoint.Y = 0;

                for (int i = 0; i < features.Count; i++)
                {
                    //If first run don't start at 0, otherwise reset max 
                    if (firstrun) { i = 1; firstrun = false; }
                   
                    tempMinPoint = features[i].FindMinPoint();
                    tempMaxPoint = features[i].FindMaxPoint();
                    tempDiff.X = (tempMaxPoint.X - tempMinPoint.X);
                    tempDiff.Y = (tempMaxPoint.Y - tempMinPoint.Y);
                    


                    if (tempDiff.X > maxDiff.X && tempDiff.Y > maxDiff.Y)
                    {
                            maxPoint = tempMaxPoint;
                            minPoint = tempMinPoint;
                            maxDiff.X = tempDiff.X; maxDiff.Y = tempDiff.Y;
                            maxDiffIndex = i;
                    }
                    
                }

                //Start the list
                List<Feature> featureGroupList = new List<Feature>();
                Feature bigFeature = features[maxDiffIndex];
                featureGroupList.Add(bigFeature);

                features.RemoveAt(maxDiffIndex);

                
                

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
                    foreach (FeatureGroup fGroup in featureGroups)
                    {
                        if (fGroup.Equals(newfGroup))
                        {
                            fGroup.Count++;
                            added = true;
                            break;
                        }
                    }
                    //If the foreach loop was excited without adding anything add newFGroup to the featuregroup list
                    if (!added)
                    {
                        newfGroup.Count++;
                        featureGroups.Add(newfGroup);
                    }
                }
                else
                {
                    newfGroup.Count++;
                    featureGroups.Add(newfGroup);
                }
            }
        }

        public List<FeatureGroup> SetFeatureGroups(List<List<Entity>> entities)
        {
            // List<Feature> brokenFeatures = getFeatureList(entities);
            List<Feature> features = new List<Feature>();

            //Create features groups things in a way that breaks the logic here
            foreach (List<Entity> entityList in entities)
            {
                features.Add(new Feature(entityList));
            }


            //First iteration of loop (Declaring variables outside loop)
            Point minPoint = new(0, 0);
            Point maxPoint = new(0, 0);
            Point maxDiff = new(0, 0);
            int maxDiffIndex = 0;

            //Temp variables to overwrite
            Point tempDiff = new(0, 0);
            Point tempMinPoint = new(0, 0);
            Point tempMaxPoint = new(0, 0);

            bool firstrun = true;
            while (features.Count > 0)
            {


                //Set max values to zero before run, if its not the first one
                maxDiffIndex = 0;
                maxDiff.X = 0; maxDiff.Y = 0;
                maxPoint.X = 0; maxDiff.X = 0;
                minPoint.X = 0; minPoint.Y = 0;

                for (int i = 0; i < features.Count; i++)
                {
                    //If first run don't start at 0, otherwise reset max 
                    if (firstrun) { i = 1; firstrun = false; }

                    tempMinPoint = features[i].FindMinPoint();
                    tempMaxPoint = features[i].FindMaxPoint();
                    tempDiff.X = (tempMaxPoint.X - tempMinPoint.X);
                    tempDiff.Y = (tempMaxPoint.Y - tempMinPoint.Y);



                    if (tempDiff.X > maxDiff.X && tempDiff.Y > maxDiff.Y)
                    {
                        maxPoint = tempMaxPoint;
                        minPoint = tempMinPoint;
                        maxDiff.X = tempDiff.X; maxDiff.Y = tempDiff.Y;
                        maxDiffIndex = i;
                    }

                }

                //Start the list
                List<Feature> featureGroupList = new List<Feature>();
                Feature bigFeature = features[maxDiffIndex];
                featureGroupList.Add(bigFeature);

                features.RemoveAt(maxDiffIndex);




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
                for (int i = 0; i < featureGroupList.Count; i++)
                {
                    List<Entity> tempEntities = featureGroupList[i].EntityList;
                    newfGroup.touchingEntities.Add(tempEntities);
                }
                if (featureGroupList.Count > 0)
                {
                    foreach (FeatureGroup fGroup in featureGroups)
                    {
                        if (fGroup.Equals(newfGroup))
                        {
                            fGroup.Count++;
                            added = true;
                            break;
                        }
                    }
                    //If the foreach loop was excited without adding anything add newFGroup to the featuregroup list
                    if (!added)
                    {
                        newfGroup.Count++;
                        featureGroups.Add(newfGroup);
                    }
                }
                else
                {
                    newfGroup.Count++;
                    featureGroups.Add(newfGroup);
                }
            }
            return featureGroups;
        }
        #endregion

        #region DetectFeatures
        /* 
         * method that goes from the path to detected features
        */
        public void detectAllFeatures()
        {
            makeFeatureList(makeTouchingEntitiesList(entityList));
        }
        public void detectAllFeatures(List<Entity> myEntityList)
        {
            List<List<Entity>> touchingEntities = makeTouchingEntitiesList(myEntityList);
            featureList = makeFeatureList(touchingEntities);
            foreach (Feature feature in featureList)
            {
                feature.extendAllEntities();
                feature.seperateBaseEntities();
                feature.seperatePerimeterEntities();
                feature.DetectFeatures();
            }
        }
        #endregion
        // Method to read the data from a file and fill the entityList with entities
        public abstract void readEntities();
    }
}
