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
        protected List<Entity> extendedEntityList;
        protected List<Entity> baseEntityList;
        protected List<Entity> PerimeterEntityList;

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
        public void extendAllEntities(List<Entity> myEntityList)
        {
            for (int i = 0; i < myEntityList.Count; i++)
            {
                if (myEntityList[i].GetEntityType() == PossibleEntityTypes.line)
                {
                    for (int j = 0; j < myEntityList.Count; j++)
                    {
                        if (myEntityList[i] is Line && myEntityList[j] is Line)
                        {
                            if (!extendTwoLines((Line)myEntityList[i], (Line)myEntityList[j])) {
                                if (!extendedEntityList.Contains(myEntityList[i]))
                                {
                                    extendedEntityList.Add(myEntityList[i]);
                                }
                                if (!extendedEntityList.Contains(myEntityList[j]))
                                {
                                    extendedEntityList.Add(myEntityList[j]);
                                }
                            }
                        }
                        if (((Line)myEntityList[i]).extend(myEntityList[j]))
                        {

                        }
                    }
                }
            }
        }

        //Method that takes two lines and extends them to touch if they are:
        // 1. not already touching
        // 2. are parallel or perpendicular
        //adds extended line(parallel) or lines(perpendicular) to extendedEntityList
        //returns true if lines where extended
        public bool extendTwoLines(Line line1, Line line2)
        {
            if (!line1.DoesIntersect(line2)) 
                //makes sure youre not extending lines that already touch
            {
                //check if the lines are parallel or perpendicular
                if (line1.isPerpendicular(line2))
                {
                    Point intersectPoint = line1.getIntersectPoint(line1, line2);
                    Point PointToExtendLine1 = line1.findPointToExtend(line1, intersectPoint);
                    Point PointToExtendLine2 = line2.findPointToExtend(line2, intersectPoint);
                    if (PointToExtendLine1 != null && PointToExtendLine2 != null)
                    {
                        // Logic for extending line1: determines what point to extend for line1
                        if (PointToExtendLine1.X == line1.StartX && PointToExtendLine1.Y == line1.StartY)
                        {
                            //make new line1 line with extended start point
                            extendedEntityList.Add(new Line(intersectPoint.X, intersectPoint.Y, line1.EndX, line1.EndY, true));
                        }
                        else
                        {
                            //make new line1 line with extended end point
                            extendedEntityList.Add(new Line(line1.StartX, line1.StartY, intersectPoint.X, intersectPoint.Y, true));
                        }
                    

                        // Logic for extending line2: determines what point to extend for line2
                        if (PointToExtendLine2.X == line2.StartX && PointToExtendLine2.Y == line2.StartY)
                        {
                            //make new line2 line with extended start point
                            extendedEntityList.Add(new Line(intersectPoint.X, intersectPoint.Y, line2.EndX, line2.EndY, true));
                        }
                        else
                        {
                            //make new line2 line with extended end point
                            extendedEntityList.Add(new Line(line2.StartX, line2.StartY, intersectPoint.X, intersectPoint.Y, true));
                        }
                        return true;
                    }                    
                }
                else if (line1.isParallel(line2))
                {
                    Point pointToExtend;
                    Line tempLine = new Line(true);//makes a new line object with extendedLine boolean to true
                    if (line1.findDistance(
                        new Point(line1.StartX, line1.StartY), 
                        new Point(line2.StartX, line2.StartY))
                        < line1.findDistance(
                        new Point(line1.EndX, line1.EndY), 
                        new Point(line2.StartX, line2.StartY)))
                        //This looks like a lot but all this is doing is finding the closest point on line1 to line2
                    {
                        //At this point we know the point to be extended on line1 is the start point, meaning the end point can stay the same
                        //  Hence why tempLine end point is set to line1's
                        pointToExtend = new Point(line1.StartX, line1.StartY);
                        tempLine.StartX = line1.EndX;
                        tempLine.StartY = line1.EndY;
                    }
                    else
                    {
                        pointToExtend = new Point(line1.EndX, line1.EndY);
                        tempLine.StartX = line1.StartX;
                        tempLine.StartY = line1.StartY;
                    }
                    if (line2.findDistance(
                        pointToExtend, 
                        new Point(line2.StartX, line2.StartY)) 
                        > line2.findDistance(
                        pointToExtend, 
                        new Point(line2.EndX, line2.EndY)))
                        //Similar to the one above but finds what point on line2 is farthest from line1's point to extend
                    {
                        tempLine.EndX = line2.StartX;
                        tempLine.EndY = line2.StartY;
                    }
                    else
                    {
                        tempLine.EndX = line2.EndX;
                        tempLine.EndY = line2.EndY;
                    }
                    extendedEntityList.Add(tempLine);
                    return true;//extended a parallel lines into 1
                }
            }
            return false;
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
