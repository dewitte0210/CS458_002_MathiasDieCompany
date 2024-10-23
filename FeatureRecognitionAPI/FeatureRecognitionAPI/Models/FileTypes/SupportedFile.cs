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
