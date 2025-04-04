/*
 * Abstract class to be inherited by every File child class
 * - DWG, DXF
 */

using ACadSharp;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;
using FeatureRecognitionAPI.Models.Utility;

namespace FeatureRecognitionAPI.Models
{
    public abstract class SupportedFile
    {

        protected string Path { get; set; }
        protected SupportedExtensions FileType { get; set; }
        internal List<Feature> FeatureList { get; set; }
        protected List<Entity> EntityList;
        internal List<FeatureGroup> FeatureGroups { get; }
        protected FileVersion _fileVersion;
        protected CadDocument doc;

        #region Constructors

        //protected keyword for nested enum is about granting 
        protected SupportedFile()
        {
            EntityList = new List<Entity>();
            FeatureList = new List<Feature>();
            FeatureGroups = new List<FeatureGroup>();
        }

        protected SupportedFile(string path)
        {
            Path = path;
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
        
        public void DetectAllFeatureTypes()
        {
            GroupFeatureEntities();
            SetFeatureGroups();

            foreach (FeatureGroup featureGroup in FeatureGroups)
            {
                featureGroup.FindFeatureTypes();
            }
        }
        
        // This function takes in a list of entities and creates features based on groups of touching entities
        // it also constructs each entity's AdjList (Adjacency List)
        public void GroupFeatureEntities()
        {
            if (FeatureList.Count > 0) {return;}
            
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

                        if (listMap[i] == -1 || listMap[j] == -1) // checks that either i or j still needs to be mapped
                        // say there is a third entitiy k that touches i and j. i and j was already checked for k and added to entitylist. when i is checked against j it would attempt to add them again.
                        {
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
                            else // both i and j is not mapped to a feature
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
                        else // both i and j are already mapped
                        {
                            if (listMap[i] != listMap[j]) // means they should become the same feature
                            {
                                FeatureList[listMap[i]].EntityList.AddRange(FeatureList[listMap[j]].EntityList); // combines the EntityLists
                                FeatureList.RemoveAt(listMap[j]); // removed the feature with entity j
                                
                                for (int k = 0; k < listMap.Count; k++) // this for loop corrects the mapping after a feature was removed
                                {
                                    if (k!=j && listMap[k] == listMap[j])
                                    {
                                        listMap[k] = listMap[i];
                                    }
                                    else if (listMap[k] > listMap[j])
                                    {
                                        listMap[k]--;
                                    }
                                }
                                listMap[j] = listMap[i];
                            }
                        }
                    }
                }

                if (count == 0 && listMap[i] == -1)
                {
                    // creates a new feature, adds it to FeatureList with EntityList i being in its EntityList
                    FeatureList.Add(new Feature(new List<Entity>()));
                    int index = FeatureList.Count - 1;
                    FeatureList[index].EntityList.Add(EntityList[i]);
                    // maps i and j to the index of that new feature in FeatureList
                    listMap[i] = index;
                }
            }

            foreach (Feature feature in FeatureList)
            {
                feature.ConstructFromEntityList();
                feature.CountEntities();
            }
        }
        
        /*
         * Groups features together and stores how many of each feature group are present in the file
         * Initliazes class variable featuresList
         */
        public void SetFeatureGroups()
        {
            if (FeatureList.Count == 0)
            {
                GroupFeatureEntities();
            }
            List<Feature> features = new List<Feature>(FeatureList);
            
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
       
        protected void ReadEntities(CadDocument doc)
        {
            List<Entity> returned = new List<Entity>();
            foreach (ACadSharp.Entities.Entity entity in doc.Entities)
            {
                if (entity is Insert insert)
                {
                    returned.AddRange(UnwrapInsert(insert));
                }
                else
                {
                    Entity? castedEntity = CadObjectToInternalEntity(entity);
                    if (!(castedEntity is null))
                    {
                        returned.Add(castedEntity);
                    }
                }
            }

            EntityList.AddRange(returned);
        }
        private static List<Entity> UnwrapInsert(Insert insert)
        {
            List<Entity> returned = new List<Entity>();
            
            Block block = insert.Block.BlockEntity;
            Matrix3 blockTranslate = Matrix3.Translate(block.BasePoint.X, block.BasePoint.Y);
                    
            Matrix3 insertTranslate = Matrix3.Translate(insert.InsertPoint.X, insert.InsertPoint.Y);
            Matrix3 insertScale = Matrix3.Scale(insert.XScale, insert.YScale);
            Matrix3 insertRotate = Matrix3.Rotate(insert.Rotation);

            Matrix3 finalTransform = insertScale * blockTranslate * insertTranslate * insertRotate;
            foreach (ACadSharp.Entities.Entity cadObject in insert.Block.Entities)
            {
                Entity? castedEntity = CadObjectToInternalEntity(cadObject);
                if (!(castedEntity is null))
                {
                    returned.Add(castedEntity.Transform(finalTransform));
                }
            }

            return returned;
        }
        
        internal static Entity? CadObjectToInternalEntity(ACadSharp.Entities.Entity cadEntity)
        {
            switch (cadEntity)
            {
                case ACadSharp.Entities.Line line:
                {
                    return new Line(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
                }
                case ACadSharp.Entities.Arc arc:
                {
                    return new Arc(arc.Center.X, arc.Center.Y, arc.Radius,
                        arc.StartAngle * (180 / Math.PI), arc.EndAngle * (180 / Math.PI));
                }
                case ACadSharp.Entities.Circle circle:
                {
                    return new Circle(circle.Center.X, circle.Center.Y, circle.Radius);
                }
                case ACadSharp.Entities.Ellipse ellipse:
                {
                    return new Ellipse(ellipse.Center.X, ellipse.Center.Y, ellipse.EndPoint.X,
                        ellipse.EndPoint.Y,
                        ellipse.RadiusRatio, ellipse.StartParameter, ellipse.EndParameter);
                }
            }

            return null;
        }
        
        protected static FileVersion GetFileVersion(string version)
        {
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
        
        public abstract void ParseFile();

        public  List<Entity> GetEntities() {return EntityList;}
        public void SetEntities(List<Entity> entities) { EntityList = entities; }
    }
}