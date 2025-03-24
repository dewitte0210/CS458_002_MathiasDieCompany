/*
 * Abstract class to be inherrited by every File child class
 * - DWG, DXF, PDF
 */

using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;

namespace FeatureRecognitionAPI.Models
{
    public abstract class SupportedFile
    {
        protected string Path { get; set; }
        protected SupportedExtensions FileType { get; set; }
        internal List<Feature> FeatureList { get; set; }
        protected List<Entity> EntityList;
        internal List<FeatureGroup> FeatureGroups { get; }
        
        #region Constructors

        //protected keyword for nested enum is about granting 
        protected SupportedFile()
        {
            EntityList = new List<Entity>();
            FeatureList = new List<Feature>();
        }

        protected SupportedFile(string path)
        {
            this.Path = path;
            EntityList = new List<Entity>();
            FeatureList = new List<Feature>();
            FeatureGroups = new List<FeatureGroup>();
        }

        public SupportedFile(List<Entity> entityList)
        {
            EntityList = entityList;
            FeatureList = new List<Feature>();
        }
        #endregion

        #region SpecialGettersAndSetters
        public List<Feature> makeFeatureList(List<List<Entity>> entities)
        {
            for (int i = 0; i < entities.Count(); i++)
            {
                Feature feature = new Feature(entities[i]);
                feature.extendAllEntities();
                feature.seperateBaseEntities();
                feature.seperatePerimeterEntities();
                feature.DetectFeatures();
                FeatureList.Add(feature);
                if (feature.PerimeterEntityList != null)
                {
                    for (int j = 0; j < feature.PerimeterEntityList.Count(); j++)
                    {
                        Feature newFeat = new Feature(feature.PerimeterEntityList[j]);
                        newFeat.DetectFeatures();
                        FeatureList.Add(newFeat);
                    }
                }
            }


            // Group identical features together
            for (int i = 0; i < FeatureList.Count(); i++)
            {
                for (int j = i + 1; j < FeatureList.Count(); j++)
                {
                    if (FeatureList[i].Equals(FeatureList[j]))
                    {
                        FeatureList[i].count += FeatureList[j].count;
                        FeatureList.RemoveAt(j);
                        j--;
                    }
                }
            }


            return FeatureList;
        }
        #endregion

        #region MakeTouchingEntities

        // This function takes in a list of entities and creates features based on groups of touching entities
        // it also constructs each entity's AdjList (Adjacency List)
        public void GroupFeatureEntities()
        {
            List<int> listMap = Enumerable.Repeat(-1, EntityList.Count).ToList(); // parallel list to EntityList mapping them to an index in FeatureList. Initializes a value of -1
            FeatureList.Add(new Feature(new List<Entity>()));
            
            FeatureList[0].EntityList.Add(EntityList[0]); // starts the mapping with the first entity in EntityList list as a new list in features
            listMap[0] = 0;
            
            for (int i = 0; i < EntityList.Count()-1; i++)
            {
                int count = 0;
                for (int j = i+1; j < EntityList.Count(); j++) // j = i+1 so we dont see the same check for an example like when i = 1 and j=5 originally and then becomes i=5 and j=1
                {
                    if (i != j && EntityList[i].DoesIntersect(EntityList[j])) // if i==j they are checking the same object and would return true for intersecting
                    {
                        // adds each entity to their AdjList. This should not happen twice because of the j=i+1
                        EntityList[i].AdjList.Add(EntityList[j]);
                        EntityList[j].AdjList.Add(EntityList[i]);
                        
                        // Check to flag an entity as Kisscut
                        count++;
                        if (count == 4 && EntityList[i] is Line tempLine)
                        {
                            // TODO: tempLine.Kisscut = True;
                        }
                        
                        if (listMap[i] != -1) // means entitiy i is mapped to a feature
                        {
                            FeatureList[listMap[i]].EntityList.Add(EntityList[j]);
                            listMap[j] = listMap[i];
                        }
                        else if (listMap[j] != -1) // means entitiy j is mapped to a feature
                        {
                            FeatureList[listMap[j]].EntityList.Add(EntityList[i]);
                            listMap[i] = listMap[j];
                        }
                        else // EntityList[i] is not mapped to a feature
                        {
                            // creates a new feature, adds it to FeatureList with EntityList i and j being in its EntityList
                            FeatureList.Add(new Feature(new List<Entity>()));
                            int index = FeatureList.Count - 1;
                            FeatureList[index].EntityList.Add(EntityList[i]);
                            FeatureList[index].EntityList.Add(EntityList[j]);
                            // maps i and j to the index of that new feature in FeatureList
                            listMap[i] = index;
                            listMap[j] = index;
                        }
                    }
                }
            }
        }
        
        /**
         * Creates and returns a list of features that are made up of touching entities in another list.
         * @Param EntityList - the list of entites in the file
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
            List<List<Entity>> entities = makeTouchingEntitiesList(EntityList);
            // List<Feature> brokenFeatures = makeFeatureList(entities);
            List<Feature> features = new List<Feature>();

            //Create features groups things in a way that breaks the logic here
            foreach (List<Entity> entityList in entities)
            {
                features.Add(new Feature(entityList));
            }


            Point minPoint = new(0, 0);
            Point maxPoint = new(0, 0);
            Point maxDiff = new(0, 0);
            int maxDiffIndex;

            //Temp variables to overwrite
            Point tempDiff = new(0, 0);
            Point tempMinPoint;
            Point tempMaxPoint;

            //bool firstrun = true;
            while (features.Count > 0)
            {
                //Set max values to zero before run, if its not the first one
                maxDiffIndex = 0;
                maxDiff.X = 0;
                maxDiff.Y = 0;
                maxPoint.X = 0;
                maxDiff.X = 0;
                minPoint.X = 0;
                minPoint.Y = 0;

                for (int i = 0; i < features.Count; i++)
                {
                    tempMinPoint = features[i].FindMinPoint();
                    tempMaxPoint = features[i].FindMaxPoint();
                    tempDiff.X = (tempMaxPoint.X - tempMinPoint.X);
                    tempDiff.Y = (tempMaxPoint.Y - tempMinPoint.Y);


                    if (tempDiff.X > maxDiff.X && tempDiff.Y > maxDiff.Y)
                    {
                        maxPoint = tempMaxPoint;
                        minPoint = tempMinPoint;
                        maxDiff.X = tempDiff.X;
                        maxDiff.Y = tempDiff.Y;
                        maxDiffIndex = i;
                    }
                }

                // Start the list
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
                    foreach (FeatureGroup fGroup in FeatureGroups)
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
                        FeatureGroups.Add(newfGroup);
                    }
                }
                else
                {
                    newfGroup.Count++;
                    FeatureGroups.Add(newfGroup);
                }
            }
        }

        public List<FeatureGroup> SetFeatureGroups(List<List<Entity>> entities)
        {
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
            Point tempMinPoint;
            Point tempMaxPoint;

            while (features.Count > 0)
            {
                //Set max values to zero before run, if its not the first one
                maxDiffIndex = 0;
                maxDiff.X = 0;
                maxDiff.Y = 0;
                maxPoint.X = 0;
                maxDiff.X = 0;
                minPoint.X = 0;
                minPoint.Y = 0;

                for (int i = 0; i < features.Count; i++)
                {
                    tempMinPoint = features[i].FindMinPoint();
                    tempMaxPoint = features[i].FindMaxPoint();
                    tempDiff.X = (tempMaxPoint.X - tempMinPoint.X);
                    tempDiff.Y = (tempMaxPoint.Y - tempMinPoint.Y);


                    if (tempDiff.X > maxDiff.X && tempDiff.Y > maxDiff.Y)
                    {
                        maxPoint = tempMaxPoint;
                        minPoint = tempMinPoint;
                        maxDiff.X = tempDiff.X;
                        maxDiff.Y = tempDiff.Y;
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

                //featureGroupList should now contain all features that fall inside bigFeature
                bool added = false;
                FeatureGroup newfGroup = new FeatureGroup(featureGroupList);
                for (int i = 0; i < featureGroupList.Count; i++)
                {
                    List<Entity> tempEntities = featureGroupList[i].EntityList;
                    newfGroup.touchingEntities.Add(tempEntities);
                }

                if (featureGroupList.Count > 0)
                {
                    foreach (FeatureGroup fGroup in FeatureGroups)
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
                        FeatureGroups.Add(newfGroup);
                    }
                }
                else
                {
                    newfGroup.Count++;
                    FeatureGroups.Add(newfGroup);
                }
            }

            return FeatureGroups;
        }

        #endregion


        public void detectAllFeatures()
        {
            makeFeatureList(makeTouchingEntitiesList(EntityList));
        }
        // Method to read the data from a file and fill the EntityList with entities
        public abstract void readEntities();
        public abstract List<Entity> GetEntities();
    }
}