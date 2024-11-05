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
using System;
using System.IO;
using System.Numerics;
using FeatureRecognitionAPI.Models.Enums;

public class Feature
{
    [JsonProperty]
    public PossibleFeatureTypes featureType { get; set; }
    [JsonProperty]
    public List<Entity> EntityList { get; set; } //list of touching entities that make up the feature
    [JsonProperty]
    bool kissCut;
    [JsonProperty]
    bool multipleRadius;
    [JsonProperty]
    double perimeter;
    [JsonProperty]
    bool border;
    public int count;
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]

    internal List<Entity> ExtendedEntityList { get; set; } // list of entities after extending them all
    internal List<Entity> baseEntityList; // what the list is sorted into from extendedEntityList which should only
                                           // contain entities that make up the base shape and possibly corner features
    protected List<List<Entity>> PerimeterEntityList; // 2 dimensional list where each list at each index is a group of
                                                      // touching entities that make up a single perimeter feature for
                                                      // the original feature
    //EXAMPLE: <[list for Mitiered notch], [list for raduis notch], [list for Group17], [list for chamfered corner]>
    // You will have to run detection for perimeter features for each index of this list
   
    private int numLines = 0;
    private int numArcs = 0;
    private int numCircles = 0;
    
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

        calcPerimeter();
    }

    public Feature (List<Entity> entityList, bool kissCut, bool multipleRadius)
    {
        EntityList = entityList;
        this.kissCut = kissCut;
        this.multipleRadius = multipleRadius;

        calcPerimeter();
    }

    public Feature(List<Entity> entityList)
    {
        this.count = 1;
        this.EntityList = entityList;
        psuedoFeatureDetection(entityList);
        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    public void psuedoFeatureDetection(List<Entity> entityList)
    {
        numLines = 0;
        numArcs = 0;
        numCircles = 0;

        //count the number of each entity type
        for (int i = 0; i < entityList.Count; i++)
        {
            if (entityList[i] is Line)
            {
                numLines++;
            }
            else if (entityList[i] is Arc)
            {
                numArcs++;
            }
            else if (entityList[i] is Circle)
            {
                numCircles++;
            }
            else
            {
                Console.WriteLine("Error: Cannot detect entity type.");
                break;
            }
        }


        //calculate and set the perimeter of the feature
        calcPerimeter();
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
        else if(numArcs == 2 && numLines == 2)
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

    //calculates the perimeter of the feature
    public void calcPerimeter()
    {
        if (featureType == PossibleFeatureTypes.Punch || featureType == PossibleFeatureTypes.Group1B1)
        {
            perimeter = EntityList[0].Length / Math.PI;
        }

        else
        {
            for (int i = 0; i < EntityList.Count; i++)
            {
                perimeter += EntityList[i].Length;
            }
        }
    }

    /*
     * Overriding the Equals method to compare two Feature objects
    */
    public override bool Equals(object obj)
    {
        var item = obj as Feature;
        if (item == null)
        {
            return false;
        }

        //calculate difference in order to use tolerence
        double perDiff = perimeter - item.perimeter;

        //if the features are identical Group1B features
        if (featureType == item.featureType && featureType == PossibleFeatureTypes.Group1B1 && kissCut == item.kissCut && multipleRadius == item.multipleRadius &&
                Math.Abs(perDiff) < 0.0005 && border == item.border)
        {
                //serialize and deserialize in order to set them to circle objects, should look into different way of doing this
                //TODO: create added check if they are arcs instead of circles
                var serializedParent = JsonConvert.SerializeObject(EntityList[0]);
                Circle c1 = JsonConvert.DeserializeObject<Circle>(serializedParent);
                serializedParent = JsonConvert.SerializeObject(item.EntityList[0]);
                Circle c2 = JsonConvert.DeserializeObject<Circle>(serializedParent);

                if (c1.radius == c2.radius)
                {
                    return true;
                }
        }

        // Checking equality of all other feature types
        else if (featureType == item.featureType && kissCut == item.kissCut && multipleRadius == item.multipleRadius &&
                Math.Abs(perDiff) < 0.0005 && border == item.border)
        {
            return true;
        }
        //not equal
        return false;
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
     *  sets extendedEntityList to entityList
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
                    if (PointToExtendLine1.X == line1.StartX && PointToExtendLine1.Y == line1.StartY)
                    {
                        //make new line1 line with extended start point
                        ExtendedEntityList.Add(new Line(intersectPoint.X, intersectPoint.Y, line1.EndX, line1.EndY, true));
                    }
                    else
                    {
                        //make new line1 line with extended end point
                        ExtendedEntityList.Add(new Line(line1.StartX, line1.StartY, intersectPoint.X, intersectPoint.Y, true));
                    }


                    // Logic for extending line2: determines what point to extend for line2
                    if (PointToExtendLine2.X == line2.StartX && PointToExtendLine2.Y == line2.StartY)
                    {
                        //make new line2 line with extended start point
                        ExtendedEntityList.Add(new Line(intersectPoint.X, intersectPoint.Y, line2.EndX, line2.EndY, true));
                    }
                    else
                    {
                        //make new line2 line with extended end point
                        ExtendedEntityList.Add(new Line(line2.StartX, line2.StartY, intersectPoint.X, intersectPoint.Y, true));
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
        Stack<Entity> curPath = new Stack<Entity>();
        List<Entity> testedEntities = new List<Entity>();

        Entity head = ExtendedEntityList[0];

        foreach(Entity entity in ExtendedEntityList)
            //this finds the entity with the greatest length and makes it the head to hopefully reduce runtime
        {
            if (entity.Length > head.Length)
            {
                head = entity;
            }
        }

        curPath.Push(head);
        if (sortExtendedLinesHelper(curPath, testedEntities, head))
        {
            baseEntityList = curPath.ToList();
            return true;
        }
        return false;
    }
    /*recursive helper function to find a closed shape with extended lines
     */
    public bool sortExtendedLinesHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head)
    {
        //base case where the current entity touches the head (means its a closed shape)
        //checks if contained in testedEntities to avoid the second entity from triggering this
        //checks if current entity is the same as head to avoid a false true
        if ( curPath.Peek() != head && curPath.Peek().EntityPointsAreTouching(head) && !testedEntities.Contains(curPath.Peek()))
        {
            return true;//path found
        }

        testedEntities.Add(curPath.Peek());//adds the current entitiy to the testedEntities

        foreach (Entity entity in ExtendedEntityList)
        {
            if (entity != curPath.Peek())
            { // checks if entity in loop is not the curent entity being checked
                if (curPath.Peek().EntityPointsAreTouching(entity) && (!testedEntities.Contains(entity)))
                // checks that the entitiy has not already been tested and is touching the entity
                {
                    curPath.Push(entity);//adds to stack
                    if (sortExtendedLinesHelper(curPath, testedEntities, head))//recursive call with updated curPath
                    {
                        return true;
                    }
                }
            }
        }
        //this point in the function means nothing is touching current entity

        if (curPath.Peek() == head)
            //if the function of the head reaches this point it means it has not found a path back to the head
        {
            foreach (Entity entity in ExtendedEntityList)
            {
                if (!testedEntities.Contains(entity)) // finds the first entity that has not been tested and selects it as the head
                {
                    curPath = new Stack<Entity>();//clears curPath and adds the new head to it
                    curPath.Push(entity);
                    return sortExtendedLinesHelper(curPath, testedEntities, entity);
                }
            }
        }

        curPath.Pop();
        return false;//nothing is touching this entity so it is popped off of curPath
    }


}