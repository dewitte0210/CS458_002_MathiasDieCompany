/*
 * For borders look at every feature that is not inside another feature (that is a border)
 * calc number up using number of borders
 * 
 * for optimization before detecting features ignore all entity groups outside
 * the first border and calculates feautrues only for that one 
 */
using FeatureRecognitionAPI.Models;
using iText.Layout.Splitting;
using iText.StyledXmlParser.Node;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHibernate.Hql.Ast;
using System;
using System.IO;
using System.Numerics;
using FeatureRecognitionAPI.Models.Enums;
using System.Security.Cryptography.Xml;
using NHibernate.Action;
using iText.Commons.Utils.Collections;
using System.Web.Http.ModelBinding;
using NHibernate.Type;

public class Feature
{
    [JsonProperty]
    public PossibleFeatureTypes featureType { get; set; }
    
    // A list of all the perimeter features attached to this features.
    [JsonProperty]
    public List<PerimeterFeatureTypes> perimeterFeatures { get; set; }

    [JsonProperty]
    public List<Entity> EntityList { get; set; } //list of touching entities that make up the feature
    [JsonProperty]
    public bool kissCut;
    [JsonProperty]
    public bool multipleRadius;
    [JsonProperty]
    public double perimeter;
    [JsonProperty]
    public bool border;
    public int count;
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]

    internal List<Entity> ExtendedEntityList { get; set; } // list of entities after extending them all
    internal List<Entity> baseEntityList; // what the list is sorted into from extendedEntityList which should only
                                           // contain entities that make up the base shape and possibly corner features
    internal List<List<Entity>> PerimeterEntityList; // 2 dimensional list where each list at each index is a group of
                                                      // touching entities that make up a single perimeter feature for
                                                      // the original feature
                                                      //EXAMPLE: <[list for Mitiered notch], [list for raduis notch], [list for Group17], [list for chamfered corner]>
                                                      // You will have to run detection for perimeter features for each index of this list

    private int numLines = 0;
    public int getNumLines() { return numLines; }
    private int numArcs = 0;
    public int getNumArcs() { return numArcs; }
    private int numCircles = 0;
    public int getNumCircles() { return numCircles; }

    private Feature() { }//should not use default constructor

    public Feature(string featureType, bool kissCut, bool multipleRadius, bool border)
    {
        this.count = 1;
        //change string input to enum value
        PossibleFeatureTypes inputAsEnum = (PossibleFeatureTypes)Enum.Parse(typeof(PossibleFeatureTypes), featureType);
        this.featureType = inputAsEnum;
        this.kissCut = kissCut;
        this.multipleRadius = multipleRadius;
        this.border = border;
        this.perimeterFeatures = new List<PerimeterFeatureTypes>();
        calcPerimeter();
    }

    public Feature(List<Entity> EntityList, bool kissCut, bool multipleRadius)
    {
        this.EntityList = EntityList;
        this.kissCut = kissCut;
        this.multipleRadius = multipleRadius;
        this.perimeterFeatures = new List<PerimeterFeatureTypes>();
        
        calcPerimeter();
    }

    public Feature(List<Entity> EntityList)
    {
        this.count = 1;
        this.EntityList = EntityList;
        this.baseEntityList = EntityList;
        this.perimeterFeatures = new List<PerimeterFeatureTypes>();
        
        CountEntities(baseEntityList, out numLines, out numArcs, out numCircles);
        
        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    public void CountEntities(List<Entity> entityList, out int numLines, out int numArcs, out int numCircles)
    {
        numLines = 0;
        numArcs = 0;
        numCircles = 0;

        //count the number of each entity type
        for (int i = 0; i < EntityList.Count; i++)
        {
            if (EntityList[i] is Line)
            {
                numLines++;
            }
            else if (EntityList[i] is Arc)
            {
                numArcs++;
            }
            else if (EntityList[i] is Circle)
            {
                numCircles++;
            }
            else
            {
                Console.WriteLine("Error: Cannot detect entity type.");
                break;
            }
        }
    }

    internal void DetectFeatures()
    {
        //check two conditions possible to make Group1B (with no perimeter features)
        if (CheckGroup1B(numCircles, numLines, numArcs, out PossibleFeatureTypes type))
        {
            featureType = type;
        }
        //check two conditions possible to make Group1A (with no perimeter features)
        else if (numLines == 4)
        {
            if (numArcs == 0)
            {
                featureType = PossibleFeatureTypes.Group1A1;
            }
            else featureType = PossibleFeatureTypes.Group1A2;
        }
        else
        {
            Console.WriteLine("Error: Cannot assign feature type.");
        }

        //Finally Add the perimeter features
        CheckGroup5();
        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    // Checks the feature to see if it is one of the Group 1B features
    internal bool CheckGroup1B(int numCircles, int numLines, int numArcs, out PossibleFeatureTypes type)
    {
        // Entity is just a circle
        if (numCircles == 1 && numLines == 0 && numArcs == 0)
        {
            type = PossibleFeatureTypes.Group1B1;
            return true;
        }
        //Entity contains the correct number of lines and arcs to be a rounded rectangle add up the degree measuers
        //of the arcs and make sure they are 360
        else if (numArcs == 2 && numLines == 2)
        {
            double totalDegrees = 0;
            baseEntityList.ForEach(entity =>
            {
                if (entity is Arc)
                {
                    totalDegrees += (entity as Arc).centralAngle;
                }
            });
            if (totalDegrees > 359.999 && totalDegrees < 360.0009)
            {
                type = PossibleFeatureTypes.Group1B2;
                return true;
            }
        }

        // set a dummy type and return false.
        type = PossibleFeatureTypes.Punch;
        return false;
    }
    
    //Checks the perimiter features attached to this feature and adds to the perimiterFeature list for every one we find
    public void CheckGroup5()
    {
        if(PerimeterEntityList == null) { return; }

        foreach (List<Entity> perimeterFeatures in PerimeterEntityList)
        {
            CountEntities(perimeterFeatures, out int lineCount, out int arcCount, out int circCount);
            if (lineCount < 2 || lineCount > 3 || circCount != 0 || arcCount > 2) { continue; }
            foreach (Entity entity in perimeterFeatures)
            {
                if(entity is Arc && ((entity as Arc).centralAngle != 90 || (entity as Arc).centralAngle != 180)) { break; }
            }
            
            // If the feature is group5, add it to the list! 
            if(HasTwoParalellLine(perimeterFeatures))
            {
                this.perimeterFeatures.Add(PerimeterFeatureTypes.Group5);
            }
        }
    }
    
    // Checks if an entity list has atleast one set of parralell lines
    private bool HasTwoParalellLine(List<Entity> entities)
    {
        for(int i = 0; i < entities.Count(); i++)
        {
            if (entities[i] is Line)
            {
                for(int j = 0; j < entities.Count(); j++)
                {
                    if(j == i || entities[j] is not Line) { continue; }
                   
                    Line entityI = (entities[i] as Line);
                    Line entityJ = (entities[j] as Line);
                    double slopeI = Math.Abs(entityI.SlopeY / entityI.SlopeX);
                    double slopeJ = Math.Abs(entityJ.SlopeY / entityJ.SlopeX);
                   
                    if (slopeI == slopeJ) 
                    {
                        return true; 
                    }
                }
            }
        }
        return false; 
    }

    //calculates the perimeter of the feature
    public void calcPerimeter()
    {
        
            for (int i = 0; i < EntityList.Count; i++)
            {
                perimeter += EntityList[i].Length;
            }
        
    }

    /*
     * Overriding the Equals method to compare two Feature objects
    */
    public override bool Equals(object obj)
    {
        if (!(obj is Feature) || (obj == null))
        {
            return false;
        }
        else if (obj == this) return true;
        /*
        * Way to quickly determin that it's likely that the features are equal.
        * There are edge cases where two features that aren't the same could be set as equal,
        * for instance, 2 arcs and 2 lines could have an equal perimeter, but be different feature types
        */

        /*
         if (this.perimeter == ((Feature)obj).perimeter
            && this.numLines == ((Feature)obj).numLines
            && this.numArcs == ((Feature)obj).numArcs
            && this.numCircles == ((Feature)obj).numCircles)
        {
            return true;
        }
        else
        {
            return false;
        }
        */

        /*
         * If there are the same number of arcs lines and circles, and permiters match, 
         * then check to see if all entities have a corresponding entity with matching values
         */
        if (((Feature)obj).numLines == numLines
            && ((Feature)obj).numCircles == numCircles
            && ((Feature)obj).numArcs == numArcs
            && ((Feature)obj).perimeter == perimeter)
        {
           // List<Entity> tmpList = new List<Entity>(((Feature)obj).EntityList);

            //Creat an array of booleans for every entity in this EntityList
            bool[] validArray = new bool[EntityList.Count];
            //Boolean arrays should be false by default
            //for(int i = 0; i < validArray.Length; i++)
            //{
            //    validArray[i] = false;
            //}

            //Genuinly my first time ever using lambda expression for something actually useful
            //sort both lists by length
            EntityList.Sort( (x, y) => x.Length.CompareTo(y.Length) );
            ((Feature)obj).EntityList.Sort( (x, y) => x.Length.CompareTo(y.Length) );

            //For each entity in this.EntityList check for a corresponding entity in tmpList
            //Remove the entity if it's found, and set the corresponding value in validArray to true

            for (int i = 0; i < EntityList.Count; i++)
            {
                switch (EntityList[i].GetEntityType())
                {
                    //All of these cases are essentially the same, but it's needed to specify which entity type
                    //.equals to use (or else it doesn't work)
                    case ("arc"):
                        {
                            int j = i;
                            //If current entities are not equal, but lengths are equal, then try next element in list
                            //Once loop ends, if obj.entitylist @ j is not equal to entitylist @ i features are not the same
                            while (!((Arc)EntityList[i]).Equals(((Feature)obj).EntityList[j])
                                && ((Feature)obj).EntityList[j].Length == EntityList[i].Length)
                            {
                                j++;
                                if (j >= EntityList.Count)
                                    break;
                            }
                            if(((Arc)EntityList[i]).Equals(((Feature)obj).EntityList[j]))
                            {
                                validArray[i] = true;
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        }
                    case ("circle"):
                        {
                            int j = i;

                            while (!((Circle)EntityList[i]).Equals(((Feature)obj).EntityList[j])
                                && ((Feature)obj).EntityList[j].Length == EntityList[i].Length)
                            {
                                j++;
                                if (j >= EntityList.Count)
                                    break;
                            }
                            if (((Circle)EntityList[i]).Equals(((Feature)obj).EntityList[j]))
                            {
                                validArray[i] = true;
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        }
                    case ("line"):
                        {
                            int j = i;
                            while (!((Line)EntityList[i]).Equals(((Feature)obj).EntityList[j])
                                && ((Feature)obj).EntityList[j].Length == EntityList[i].Length)
                            {
                                j++;
                                if (j >= EntityList.Count)
                                    break;
                            }
                            if (((Line)EntityList[i]).Equals(((Feature)obj).EntityList[j]))
                            {
                                validArray[i] = true;
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        }
                    default:
                        {
                            //why are we here?
                            return false;
                        }
                }
            }// End of for loop
            //After exiting for loop, every value of validArray should be true (if features are equal)
            foreach(bool var in validArray)
            {
                if(!var)
                {
                    return false;
                }
            }
            //If false isn't returned in foreach loop, return true
            return true;
        }
        else return false;

    }


    /*
     * Recursive function that calls extendAllEntitiesHelper
     * @Param myEntityList parameter that extendedEntityList is set equal to
    */
    public void extendAllEntities(List<Entity> myEntityList)
    {
        ExtendedEntityList = myEntityList;
        extendAllEntitiesHelper();
    }

    /*
     *  Recursive function that calls extendAllEntitiesHelper
     *  sets extendedEntityList to EntityList
    */
    public void extendAllEntities()
    {
        ExtendedEntityList = EntityList;
        extendAllEntitiesHelper();
    }

    /*
     * This is a recursive helper function to extend every line in extendedEntityList
    */
    private void extendAllEntitiesHelper()
    {
        bool extendedALine = false; // repeats recursivly if this is true
        //this block extends every line in extendedEntityList
        //foreach (var entity in extendedEntityList)
        for (int i = 0; i < ExtendedEntityList.Count; i++)
        {
            if (ExtendedEntityList[i] is Line)
            {
                //foreach (var otherEntity in extendedEntityList)
                for (int j = 0; j < ExtendedEntityList.Count; j++)
                {   
                    if (ExtendedEntityList[j] is Line && ExtendedEntityList[i] != ExtendedEntityList[j])
                    {
                        // for each entity it checks if it can extend with every other entity and does so
                        // removes the two previous entities
                        // new extended lines are added in the extendTwoLines method
                        if (extendTwoLines((Line)ExtendedEntityList[i], (Line)ExtendedEntityList[j]))
                        {
                            extendedALine = true;
                        }
                    }
                }
            }
        }
        if (extendedALine)
        {
            extendAllEntitiesHelper();
        }
        else
        {
            return;
        }
    }

    //Method that takes two lines and extends them to touch if they are:
    // 1. not already touching
    // 2. are parallel or perpendicular
    //adds extended line(parallel) or lines(perpendicular) to extendedEntityList
    //returns true if lines were extended, otherwise false

    public bool extendTwoLines(Line line1, Line line2)
    {
        if (!line1.DoesIntersect(line2))
        //makes sure youre not extending lines that already touch
        {
            //Does not need to detect if lines are perpendicular since they might not be perfectly perpendicular
            //check if the lines are parallel or perpendicular
            /*if (line1.isPerpendicular(line2))
            {
                Point intersectPoint = line1.getIntersectPoint(line1, line2);
                Point PointToExtendLine1 = line1.findPointToExtend(line1, intersectPoint);
                Point PointToExtendLine2 = line2.findPointToExtend(line2, intersectPoint);
                if (PointToExtendLine1 != null && PointToExtendLine2 != null)
                {
                    // Logic for extending line1: determines what point to extend for line1
                    if (PointToExtendLine1.X == line1.Start.X && PointToExtendLine1.Y == line1.Start.Y)
                    {
                        //make new line1 line with extended start point
                        ExtendedEntityList.Add(new Line(intersectPoint.X, intersectPoint.Y, line1.End.X, line1.End.Y, true));
                    }
                    else
                    {
                        //make new line1 line with extended end point
                        ExtendedEntityList.Add(new Line(line1.Start.X, line1.Start.Y, intersectPoint.X, intersectPoint.Y, true));
                    }


                    // Logic for extending line2: determines what point to extend for line2
                    if (PointToExtendLine2.X == line2.Start.X && PointToExtendLine2.Y == line2.Start.Y)
                    {
                        //make new line2 line with extended start point
                        ExtendedEntityList.Add(new Line(intersectPoint.X, intersectPoint.Y, line2.End.X, line2.End.Y, true));
                    }
                    else
                    {
                        //make new line2 line with extended end point
                        ExtendedEntityList.Add(new Line(line2.Start.X, line2.Start.Y, intersectPoint.X, intersectPoint.Y, true));
                    }
                    return true;
                }
            }*/
            if (line1.isParallel(line2))
            {
                Point pointToExtend;
                Line tempLine = new Line(true);//makes a new line object with extendedLine boolean to true
                if (line1.findDistance(
                    line1.StartPoint,
                    line2.StartPoint)
                    < line1.findDistance(
                    line1.EndPoint,
                    line2.StartPoint))
                //This looks like a lot but all this is doing is finding the closest point on line1 to line2
                {
                    //At this point we know the point to be extended on line1 is the start point, meaning the end point can stay the same
                    //  Hence why tempLine end point is set to line1's
                    pointToExtend = line1.StartPoint;
                    tempLine.StartPoint.X = line1.EndPoint.X;
                    tempLine.StartPoint.Y = line1.EndPoint.Y;
                }
                else
                {
                    pointToExtend = line1.EndPoint;
                    tempLine.StartPoint.X = line1.StartPoint.X;
                    tempLine.StartPoint.Y = line1.StartPoint.Y;
                }
                if (line2.findDistance(
                    pointToExtend,
                    line2.StartPoint)
                    > line2.findDistance(
                    pointToExtend,
                    line2.EndPoint))
                //Similar to the one above but finds what point on line2 is farthest from line1's point to extend
                {
                    tempLine.EndPoint.X = line2.StartPoint.X;
                    tempLine.EndPoint.Y = line2.StartPoint.Y;
                }
                else
                {
                    tempLine.EndPoint.X = line2.EndPoint.X;
                    tempLine.EndPoint.Y = line2.EndPoint.Y;
                }
                ExtendedEntityList.Remove(line1);
                ExtendedEntityList.Remove(line2);
                ExtendedEntityList.Add(tempLine);
                return true;//extended two parallel lines into 1
            }
        }
        return false;
    }

    public bool sortExtendedLines()
    {
        Stack<Entity> path = new Stack<Entity>();
        sortExtendedLinesHelper(path, 0);
        return false;
    }
    public bool sortExtendedLinesHelper(Stack<Entity> curPath, int index)
    {
        curPath.Push(ExtendedEntityList[index]);
        List<Entity> connectedEntities = new List<Entity>();
        foreach (Entity entity in ExtendedEntityList)
        {
            if (ExtendedEntityList[index] != entity)
            {
              
            }
        }
        return false;
    }



    public Point FindMaxPoint()
    {
        double maxX = 0;
        //Find the starting max 
        if (EntityList[0] is Line)
        {
            if (((Line)EntityList[0]).StartPoint.X > ((Line)EntityList[0]).EndPoint.X)
            {
                maxX = ((Line)EntityList[0]).StartPoint.X;
            }
            else
            {
                maxX = ((Line)EntityList[0]).EndPoint.X;
            }
        }
        else if (EntityList[0] is Arc)
        {
            if (((Arc)EntityList[0]).Start.X > ((Arc)EntityList[0]).End.X)
            {
                maxX = ((Arc)EntityList[0]).Start.X;
            }
            else
            {
                maxX = ((Arc)EntityList[0]).End.X;
            }
        }
        else if (EntityList[0] is Circle)
        {
            maxX = ( ((Circle)EntityList[0]).Center.X + ((Circle)EntityList[0]).radius );
        }

        //Loop through list and see if there is a bigger X
        for (int i = 1; i < EntityList.Count; i++)
        {
            if (EntityList[i] is Line)
            {
                if (((Line)EntityList[i]).StartPoint.X > maxX)
                {
                    maxX = ((Line)EntityList[i]).StartPoint.X;
                }
                if (((Line)EntityList[i]).EndPoint.X > maxX)
                {
                    maxX = ((Line)EntityList[i]).EndPoint.X;
                }
            }
            else if (EntityList[i] is Arc)
            {
                if (((Arc)EntityList[i]).Start.X > maxX)
                {
                    maxX = ((Arc)EntityList[i]).Start.X;
                }
                if (((Arc)EntityList[i]).End.X > maxX)
                {
                    maxX = ((Arc)EntityList[i]).End.X;
                }
            }
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.X + ((Circle)EntityList[0]).radius) > maxX ) 
            {
                    maxX = (((Circle)EntityList[0]).Center.X + ((Circle)EntityList[0]).radius);
            }
            
        }

        double maxY = 0;
        //Find the starting max Y
        if (EntityList[0] is Line)
        {
            if (((Line)EntityList[0]).StartPoint.Y > ((Line)EntityList[0]).EndPoint.Y)
            {
                maxY = ((Line)EntityList[0]).StartPoint.Y;
            }
            else
            {
                maxY = ((Line)EntityList[0]).EndPoint.Y;
            }
        }
        else if (EntityList[0] is Arc)
        {
            if (((Arc)EntityList[0]).Start.Y > ((Arc)EntityList[0]).End.Y)
            {
                maxY = ((Arc)EntityList[0]).Start.Y;
            }
            else
            {
                maxY = ((Arc)EntityList[0]).End.Y;
            }
        }
        else if (EntityList[0] is Circle)
        {
            maxY = (((Circle)EntityList[0]).Center.Y + ((Circle)EntityList[0]).radius);
        }

        //Loop through list and see if there is a bigger Y 
        for (int i = 1; i < EntityList.Count; i++)
        {
            if (EntityList[i] is Line)
            {
                if (((Line)EntityList[i]).StartPoint.Y > maxY)
                {
                    maxY = ((Line)EntityList[i]).StartPoint.Y;
                }
                if (((Line)EntityList[i]).EndPoint.Y > maxY)
                {
                    maxY = ((Line)EntityList[i]).EndPoint.Y;
                }
            }
            else if (EntityList[i] is Arc)
            {
                if (((Arc)EntityList[i]).Start.Y > maxY)
                {
                    maxY = ((Arc)EntityList[i]).Start.Y;
                }
                if (((Arc)EntityList[i]).End.Y > maxY)
                {
                    maxY = ((Arc)EntityList[i]).End.Y;
                }
            }
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.Y + ((Circle)EntityList[0]).radius) > maxY)
            {
                maxY = (((Circle)EntityList[0]).Center.Y + ((Circle)EntityList[0]).radius);
            }

        }
        return new Point( maxX, maxY );
    }

    public Point FindMinPoint()
    {
        double minX = 0;
        //Find the starting min 
        if (EntityList[0] is Line)
        {
            if (((Line)EntityList[0]).StartPoint.X < ((Line)EntityList[0]).EndPoint.X)
            {
                minX = ((Line)EntityList[0]).StartPoint.X;
            }
            else
            {
                minX = ((Line)EntityList[0]).EndPoint.X;
            }
        }
        else if (EntityList[0] is Arc)
        {
            if (((Arc)EntityList[0]).Start.X < ((Arc)EntityList[0]).End.X)
            {
                minX = ((Arc)EntityList[0]).Start.X;
            }
            else
            {
                minX = ((Arc)EntityList[0]).End.X;
            }
        }
        else if (EntityList[0] is Circle)
        {
            minX = (((Circle)EntityList[0]).Center.X - ((Circle)EntityList[0]).radius);
        }

        //Loop through list and see if there is a smaller Y
        for (int i = 1; i < EntityList.Count; i++)
        {
            if (EntityList[i] is Line)
            {
                if (((Line)EntityList[i]).StartPoint.X < minX)
                {
                    minX = ((Line)EntityList[i]).StartPoint.X;
                }
                if (((Line)EntityList[i]).EndPoint.X < minX)
                {
                    minX = ((Line)EntityList[i]).EndPoint.X;
                }
            }
            else if (EntityList[i] is Arc)
            {
                if (((Arc)EntityList[i]).Start.X < minX)
                {
                    minX = ((Arc)EntityList[i]).Start.X;
                }
                if (((Arc)EntityList[i]).End.X < minX)
                {
                    minX = ((Arc)EntityList[i]).End.X;
                }
            }
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.X - ((Circle)EntityList[0]).radius) > minX)
            {
                minX = (((Circle)EntityList[0]).Center.X - ((Circle)EntityList[0]).radius);
            }

        }

        double minY = 0;
        //Find the starting minumum 
        if (EntityList[0] is Line)
        {
            if (((Line)EntityList[0]).StartPoint.Y < ((Line)EntityList[0]).EndPoint.Y)
            {
                minY = ((Line)EntityList[0]).StartPoint.Y;
            }
            else
            {
                minY = ((Line)EntityList[0]).EndPoint.Y;
            }
        }
        else if (EntityList[0] is Arc)
        {
            if (((Arc)EntityList[0]).Start.Y < ((Arc)EntityList[0]).End.Y)
            {
                minY = ((Arc)EntityList[0]).Start.Y;
            }
            else
            {
                minY = ((Arc)EntityList[0]).End.Y;
            }
        }
        else if (EntityList[0] is Circle)
        {
            minY = (((Circle)EntityList[0]).Center.Y - ((Circle)EntityList[0]).radius);
        }

        //Loop through list and see if there is a smaller Y
        for (int i = 1; i < EntityList.Count; i++)
        {
            if (EntityList[i] is Line)
            {
                if (((Line)EntityList[i]).StartPoint.Y < minY)
                {
                    minY = ((Line)EntityList[i]).StartPoint.Y;
                }
                if (((Line)EntityList[i]).EndPoint.Y < minY)
                {
                    minY = ((Line)EntityList[i]).EndPoint.Y;
                }
            }
            else if (EntityList[i] is Arc)
            {
                if (((Arc)EntityList[i]).Start.Y < minY)
                {
                    minY = ((Arc)EntityList[i]).Start.Y;
                }
                if (((Arc)EntityList[i]).End.Y < minY)
                {
                    minY = ((Arc)EntityList[i]).End.Y;
                }
            }
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.Y - ((Circle)EntityList[0]).radius) > minY)
            {
                minY = (((Circle)EntityList[0]).Center.Y - ((Circle)EntityList[0]).radius);
            }

        }
        return new Point(minX, minY);

    }

} 


