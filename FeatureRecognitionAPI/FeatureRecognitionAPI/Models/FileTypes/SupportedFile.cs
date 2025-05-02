using ACadSharp;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;
using FeatureRecognitionAPI.Models.Utility;
using FeatureRecognitionAPI.Services;
using Arc = FeatureRecognitionAPI.Models.Entities.Arc;
using Circle = FeatureRecognitionAPI.Models.Entities.Circle;
using Ellipse = FeatureRecognitionAPI.Models.Entities.Ellipse;
using Entity = FeatureRecognitionAPI.Models.Entities.Entity;
using Line = FeatureRecognitionAPI.Models.Entities.Line;
using Point = FeatureRecognitionAPI.Models.Entities.Point;

namespace FeatureRecognitionAPI.Models
{
    /// <summary>
    /// Abstract class to be inherited by every File child class - DWG, DXF
    /// </summary>
    public abstract class SupportedFile
    {

        protected string Path { get; set; }
        protected SupportedExtensions FileType { get; set; }
        internal List<Feature> FeatureList { get; set; }
        protected internal List<Entity> EntityList;
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
            SetEntities(EntityTools.CondenseArcs(GetEntities()));
            GroupFeatureEntities();
            SetFeatureGroups();

            //if there is a num-up here, only one copy of the die will have its lines identified as recognized or unrecognized
            foreach (FeatureGroup featureGroup in FeatureGroups)
            {
                featureGroup.FindFeatureTypes();
            }
            
            //run feature detection on everything if there is a num-up so that unrecognized features can be highlighted in the front end
            if (FeatureGroups.Any(group => group.Count > 1))
            {
                foreach (Feature feature in FeatureList.Where(f => f.FeatureType == null))
                {
                   feature.ExtendAllEntities();
                   feature.SeperateBaseEntities();
                   feature.SeperatePerimeterEntities();
                   feature.DetectFeatures();
                }
            }
        }

        /// <summary>
        /// This function takes in a list of entities and creates features based on groups of touching entities
        /// it also constructs each entity's AdjList (Adjacency List)
        /// </summary>
        public void GroupFeatureEntities()
        {
            if (FeatureList.Count > 0) return;

            // parallel list to EntityList mapping them to an index in FeatureList. Initializes a value of -1
            List<int> listMap = Enumerable.Repeat(-1, EntityList.Count).ToList();
            FeatureList.Add(new Feature(new List<Entity>()));

            // starts the mapping with the first entity in EntityList list as a new list in features
            FeatureList[0].EntityList.Add(EntityList[0]);
            listMap[0] = 0;

            for (int i = 0; i < EntityList.Count; i++)
            {
                int count = 0;
                // j = i+1 so we don't see the same check for an example like when i = 1 and j=5 originally and then becomes i=5 and j=1
                for (int j = i + 1; j < EntityList.Count; j++)
                {
                    if (!Intersect.DoesIntersect(EntityList[i], EntityList[j])) continue;

                    // these entities do intersect
                    // adds each entity to their AdjList. This should not happen twice because of the j=i+1
                    EntityList[i].AdjList.Add(EntityList[j]);
                    EntityList[j].AdjList.Add(EntityList[i]);

                    // Check to flag an entity as Kisscut
                    count++;
                    if (count == 4 && EntityList[i] is Line tempLine)
                    {
                        tempLine.KissCut = true;
                    }

                    // checks that either i or j still needs to be mapped
                    // say there is a third entity k that touches i and j.
                    // i and j was already checked for k and added to entityList.
                    // when i is checked against j it would attempt to add them again.
                    if (listMap[i] == -1 || listMap[j] == -1)
                    {
                        if (listMap[i] != -1) // means entity i is mapped to a feature
                        {
                            FeatureList[listMap[i]].EntityList.Add(EntityList[j]);
                            listMap[j] = listMap[i];
                        }
                        else if (listMap[j] != -1) // means entity j is mapped to a feature
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
                        if (listMap[i] == listMap[j])
                        {
                            continue;
                        }

                        //they should become the same feature
                        FeatureList[listMap[i]].EntityList
                            .AddRange(FeatureList[listMap[j]].EntityList); // combines the EntityLists
                        FeatureList.RemoveAt(listMap[j]); // removed the feature with entity j

                        // this for loop corrects the mapping after a feature was removed
                        for (int k = 0; k < listMap.Count; k++)
                        {
                            if (k != j && listMap[k] == listMap[j])
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
            }
        }
        
        /// <summary>
        /// Groups features together and stores how many of each feature group are present in the file
        /// Initializes class variable featuresList
        /// </summary>
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

            while (features.Count > 0)
            {
                //Set max values to zero before run
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
                    //TempMin should be greater than minPoint
                    if (tempMaxPoint.X < maxPoint.X && tempMaxPoint.Y < maxPoint.Y
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
                            fGroup.NumIdenticalFeatureGroups++;
                            added = true;
                            break;
                        }
                    }

                    //If the foreach loop was excited without adding anything add newFGroup to the featuregroup list
                    if (!added)
                    {
                        newfGroup.NumIdenticalFeatureGroups++;
                        FeatureGroups.Add(newfGroup);
                    }
                }
                else
                {
                    newfGroup.NumIdenticalFeatureGroups++;
                    FeatureGroups.Add(newfGroup);
                }
            }
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
        
        public void ParseFile()
        {
            _fileVersion = GetFileVersion(doc.Header.VersionString);
            
            foreach (ACadSharp.Entities.Entity entity in doc.Entities)
            {
                if (entity is Spline)
                {
                    throw new NotImplementedException("Splines are not yet supported. Please explode your splines before uploading your file.");
                }
                if (entity is LwPolyline)
                {
                    throw new NotImplementedException("Polylines are not yet supported. Please explode your polylines before uploading your file.");
                }
                
                if (entity is Insert insert)
                {
                    EntityList.AddRange(UnwrapInsert(insert));
                }
                else
                {
                    Entity? castedEntity = CadObjectToInternalEntity(entity);
                    if (!(castedEntity is null))
                    {
                        EntityList.Add(castedEntity);
                    }
                }
            }
        }

        public void CornerNotchFlag()
        {
            foreach (Feature feature in FeatureList)
            {
            }
        }

        public void CornerNotchFlagHelper(Line entity)
        {
            if (entity.AdjList.Count != 2) { return; }

            Angles.Angle innerAngle = null;
            Angles.Angle outerAngleClose = null;
            Angles.Angle outerAngleFar = null;
            for (int i = 0; i < entity.AdjList.Count; i++)
            {
                if (entity.AdjList[i] is Line { AdjList.Count: 2 } e && entity.GetLength() == e.GetLength())
                {
                    innerAngle = Angles.GetAngle(entity, e);
                    outerAngleClose = Angles.GetAngle(entity, (Line)entity.AdjList[1-i]);

                    for (int j = i + 1; j < e.AdjList.Count; j++)
                    {
                        if (!e.AdjList[j].Equals(entity))
                        {
                            outerAngleFar = Angles.GetAngle(e, (Line)e.AdjList[j]);
                        }
                    }
                }
            }

            if (innerAngle != null && outerAngleClose != null && outerAngleFar != null)
            {
                
            }
        }
        public  List<Entity> GetEntities() {return EntityList;}
        public void SetEntities(List<Entity> entities) { EntityList = entities; }
    }
}