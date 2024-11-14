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
    public PossibleFeatureTypes FeatureType { get; set; }
    
    // A list of all the perimeter features attached to this features.
    [JsonProperty]
    public List<PerimeterFeatureTypes> PerimeterFeatures { get; set; }

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

    public Feature(string featureType, bool kissCut, bool multipleRadius, bool border)
    {
        this.count = 1;
        //change string input to enum value
        PossibleFeatureTypes inputAsEnum = (PossibleFeatureTypes)Enum.Parse(typeof(PossibleFeatureTypes), featureType);
        this.FeatureType = inputAsEnum;
        this.kissCut = kissCut;
        this.multipleRadius = multipleRadius;
        this.border = border;
        EntityList = new List<Entity>();
        baseEntityList = new List<Entity>();
        ExtendedEntityList = new List<Entity>();
        PerimeterEntityList = new List<List<Entity>>();
        PerimeterFeatures = new List<PerimeterFeatureTypes>();

        calcPerimeter();
    }

    public Feature (List<Entity> entityList, bool kissCut, bool multipleRadius)
    {
        EntityList = entityList;
        this.kissCut = kissCut;
        this.multipleRadius = multipleRadius;
        baseEntityList = new List<Entity>();
        ExtendedEntityList = new List<Entity>();
        PerimeterEntityList = new List<List<Entity>>();
        this.PerimeterFeatures = new List<PerimeterFeatureTypes>();

        calcPerimeter();
    }

    public Feature(List<Entity> EntityList)
    {
        this.count = 1;
        this.EntityList = EntityList;
        this.baseEntityList = EntityList;
        this.PerimeterFeatures = new List<PerimeterFeatureTypes>();
        ExtendedEntityList = new List<Entity>();
        PerimeterEntityList = new List<List<Entity>>();

        CountEntities(EntityList, out numLines, out numArcs, out numCircles);
        
        //calculate and set the perimeter of the feature
        calcPerimeter();
    }
    public Feature(PerimeterFeatureTypes perimeterFeatureType)
    {
        count = 1;
        FeatureType = (PossibleFeatureTypes)Enum.Parse(typeof(PossibleFeatureTypes), perimeterFeatureType.ToString());

        calcPerimeter();
    }

    public void CountEntities(List<Entity> entityList, out int numLines, out int numArcs, out int numCircles)
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
    }

    internal void DetectFeatures()
    {
        //check two conditions possible to make Group1B (with no perimeter features)
        if (CheckGroup1B(numCircles, numLines, numArcs, out PossibleFeatureTypes type))
        {
            FeatureType = type;
        }
        //check two conditions possible to make Group1A (with no perimeter features)
        else if (numLines == 4)
        {
            if (numArcs == 0)
            {
                FeatureType = PossibleFeatureTypes.Group1A1;
            }
            else FeatureType = PossibleFeatureTypes.Group1A2;
        }
        else
        {
            //Console.WriteLine("Error: Cannot assign feature type.");
        }

        //Finally Add the perimeter features
        CheckGroup5();
        CheckGroup4();
        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    // Checks the feature to see if it is one of the Group 1B features
    internal bool CheckGroup1B(int numCircles, int numLines, int numArcs, out PossibleFeatureTypes type)
    {
        // Entity is just a circle
        if (numCircles == 1 && numLines == 0 && numArcs == 0)
        {
            Circle c = baseEntityList[0] as Circle;
            if (c.Radius * 2 <= 1.75)
            {
                type = PossibleFeatureTypes.Punch;
            }
            else
            {
                type = PossibleFeatureTypes.Group1B1;
            }
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
                    totalDegrees += (entity as Arc).CentralAngle;
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
    
    //Checks the perimiter entity list to detect if any of the features there belong to group 4, then adds any we find to the perimiterFeature list 
    public void CheckGroup4()
    {
        if(PerimeterEntityList == null) { return;  }

        foreach (List<Entity> feature in PerimeterEntityList)
        {
            bool g4Detected = false;
            Line tempLine = null;
            CountEntities(feature, out int lineCount, out int arcCount, out int circleCount);

            if (lineCount != 2 || (arcCount != 2 && arcCount !=0)) { continue; }

            foreach (Entity entity in feature)
            {
                if (entity is Line && tempLine == null)
                {
                    tempLine = (entity as Line);  
                } else if(entity is Line)
                {
                    g4Detected = tempLine.DoesIntersect(entity);
                }
            }
            if (g4Detected)
            {
                PerimeterFeatures.Add(PerimeterFeatureTypes.Group4);
            }
        }
    }

    //Checks the perimiter entity list to detect if any of the features there belong to group 5, then adds any we find to the perimiterFeature list 
    public void CheckGroup5()
    {
        if(PerimeterEntityList == null) { return; }

        foreach (List<Entity> feature in PerimeterEntityList)
        {
            CountEntities(feature, out int lineCount, out int arcCount, out int circCount);
            if (lineCount < 2 || lineCount > 3 || circCount != 0 || arcCount > 2) { continue; }
            foreach (Entity entity in feature)
            {
                if(entity is Arc && ((entity as Arc).CentralAngle != 90 || (entity as Arc).CentralAngle != 180)) { break; }
            }
            
            // If the feature is group5, add it to the list! 
            if(HasTwoParalellLine(feature))
            {
                PerimeterFeatures.Add(PerimeterFeatureTypes.Group5);
                
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
                    
                    // Check for verticality
                    if((entityI.SlopeX == 0 && entityJ.SlopeX == 0) || (entityI.SlopeY == 0 && entityJ.SlopeY == 0)) 
                    {
                        return true;
                    }
                    
                    double slopeI = entityI.SlopeY / entityI.SlopeX;
                    double slopeJ = entityJ.SlopeY / entityJ.SlopeX;
                   
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
        perimeter = 0;
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
        if ((!(obj is Feature)) || (obj == null))
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
            //Genuinly my first time ever using lambda expression for something actually useful
            //sort both lists by length
            EntityList.Sort( (x, y) => x.Length.CompareTo(y.Length) );
            ((Feature)obj).EntityList.Sort( (x, y) => x.Length.CompareTo(y.Length) );

            //For each entity in this.EntityList check for a corresponding entity in tmpList
            //Remove the entity if it's found, and set the corresponding value in validArray to true
            bool equalLists = true;
            foreach(Entity j in ((Feature)obj).EntityList)
            {
                if (!EntityList.Contains(j)) { equalLists = false; }
            }
            return equalLists;
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
        ExtendedEntityList = new List<Entity>(EntityList);
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
                    if ((ExtendedEntityList[j] is Line) && ExtendedEntityList[i] != ExtendedEntityList[j])
                    {
                        // for each entity it checks if it can extend with every other entity and does so
                        // removes the two previous entities
                        // new extended lines are added in the extendTwoLines method
                        if (extendTwoLines((Line)ExtendedEntityList[i], (Line)ExtendedEntityList[j]))
                        {
                            extendedALine = true;
                            break;
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
                ExtendedLine tempLine = new ExtendedLine(line1, line2);//makes a new line object with extendedLine boolean to      
                ExtendedEntityList.Remove(line1);
                ExtendedEntityList.Remove(line2);
                ExtendedEntityList.Add(tempLine);
                return true;//extended two parallel lines into 1
            }
        }
        return false;
    }

    public bool seperateBaseEntities()
    {
        if (ExtendedEntityList.Count == 1 && ExtendedEntityList[0] is Circle && baseEntityList.Count ==0)
        {
            baseEntityList.Add((Circle)ExtendedEntityList[0]);
            return true;
        }
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
        if (seperateBaseEntitiesHelper(curPath, testedEntities, head))
        {
            baseEntityList = curPath.ToList();
            baseEntityList.Reverse();
            return true;
        }
        return false;
    }
    /*recursive helper function to find a closed shape with extended lines
     */
    public bool seperateBaseEntitiesHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head)
    {
        if (curPath.Count > 2)
        {
            //base case where the current entity touches the head (means its a closed shape)
            //checks if contained in testedEntities to avoid the second entity from triggering this
            //checks if current entity is the same as head to avoid a false true
            if (curPath.Peek() != head && curPath.Peek().EntityPointsAreTouching(head) && !testedEntities.Contains(curPath.Peek()))
            {
                return true;//path found
            }
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
                    if (seperateBaseEntitiesHelper(curPath, testedEntities, head))//recursive call with updated curPath
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
                    curPath.Clear();//clears curPath and adds the new head to it
                    curPath.Push(entity);
                    return seperateBaseEntitiesHelper(curPath, testedEntities, entity);
                }
            }
        }

        curPath.Pop();
        return false;//nothing is touching this entity so it is popped off of curPath
    }

    public List<Entity> findPathFromStartToTargetInEntityList( Entity start, Entity target)
    {
        if (EntityList.Contains(start) && EntityList.Contains(target))
        {
            Stack<Entity> path = new Stack<Entity>();
            if (findPathFromStartToTargetInEntityListHelper(path, new List<Entity>(), start, target)) { return path.ToList(); }
            else { return null; }
        }
        else { return null; }
    }

    public bool findPathFromStartToTargetInEntityListHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head, Entity target)
    {
        if (curPath.Peek() == target) { return true; }

        testedEntities.Add(curPath.Peek());//adds the current entitiy to the testedEntities

        foreach (Entity entity in EntityList)
        {
            if (entity != curPath.Peek())
            { // checks if entity in loop is not the curent entity being checked
                if (curPath.Peek().EntityPointsAreTouching(entity) && (!testedEntities.Contains(entity)))
                // checks that the entitiy has not already been tested and is touching the entity
                {
                    curPath.Push(entity);//adds to stack
                    if (seperateBaseEntitiesHelper(curPath, testedEntities, head))//recursive call with updated curPath
                    {
                        return true;
                    }
                }
            }
        }
        //this point in the function means nothing is touching current entity

        curPath.Pop();
        return false;//nothing is touching this entity so it is popped off of curPath
    }

    public bool seperatePerimeterEntities()
    {
        foreach(Entity entity in baseEntityList)
        {
            if(entity is ExtendedLine)
            {
                List<Entity> path = findPathFromStartToTargetInEntityList(((ExtendedLine)entity).Parent1, ((ExtendedLine)entity).Parent2);            
                PerimeterEntityList.Add(path);
            }
        }
        return true;
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
            maxX = ( ((Circle)EntityList[0]).Center.X + ((Circle)EntityList[0]).Radius );
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
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.X + ((Circle)EntityList[0]).Radius) > maxX ) 
            {
                    maxX = (((Circle)EntityList[0]).Center.X + ((Circle)EntityList[0]).Radius);
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
            maxY = (((Circle)EntityList[0]).Center.Y + ((Circle)EntityList[0]).Radius);
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
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.Y + ((Circle)EntityList[0]).Radius) > maxY)
            {
                maxY = (((Circle)EntityList[0]).Center.Y + ((Circle)EntityList[0]).Radius);
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
            minX = (((Circle)EntityList[0]).Center.X - ((Circle)EntityList[0]).Radius);
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
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.X - ((Circle)EntityList[0]).Radius) > minX)
            {
                minX = (((Circle)EntityList[0]).Center.X - ((Circle)EntityList[0]).Radius);
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
            minY = (((Circle)EntityList[0]).Center.Y - ((Circle)EntityList[0]).Radius);
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
            else if (EntityList[i] is Circle && (((Circle)EntityList[0]).Center.Y - ((Circle)EntityList[0]).Radius) > minY)
            {
                minY = (((Circle)EntityList[0]).Center.Y - ((Circle)EntityList[0]).Radius);
            }

        }
        return new Point(minX, minY);

    }

} 


