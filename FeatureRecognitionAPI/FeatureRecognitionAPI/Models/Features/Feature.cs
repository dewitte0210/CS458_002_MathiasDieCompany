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
using NHibernate.SqlCommand;

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
    public double diameter;
    [JsonProperty]
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
    public int numEllipses = 0;
    public int getNumEllipses() { return numEllipses; }

    /*
     * Constructor that passes in an entityList for the feature. Feature detection is expected to be
     * called on a feature using this constructor. This was mostly used for testing when wanting to
     * avoid feature detection in the constructor. Could probably be deleted at this point since
     * feature detection was moved out of the constructors.
     * 
     * @Param entityList is the entityList being passed into the feature. could be a base feature,
     * that includes perimeter features, or just the list for a perimeter feature
     * @Param kissCut stores whether the feature is kiss cut
     * @Param multipleRadius stores whther the feature has multiple radiuses for rounded corners
     */
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

    /*
     * Constructor that is expected to be used the most as it just passes in the entityList for the 
     * feature and detection, along with all other fields will be calculated based off this list in
     * a seperate function
     * 
     * @Param EntityList is the list being passed into the feature which includes all base entities
     * of the feature, including the perimeter features entities, unless the feature is just a
     * perimeter one
     */
    public Feature(List<Entity> EntityList)
    {
        this.count = 1;
        this.EntityList = EntityList;
        this.baseEntityList = EntityList;
        this.PerimeterFeatures = new List<PerimeterFeatureTypes>();
        ExtendedEntityList = new List<Entity>();
        PerimeterEntityList = new List<List<Entity>>();

        CountEntities(baseEntityList, out numLines, out numArcs, out numCircles, out numEllipses);
        
        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    /*
     * Counts the Lines, Arcs, and Circles in the entityList. 
     * 
     * @Param entityList is the list that is being looped through. Note that it is passed by reference
     * and any changes to the list in this function will change the list in the scope of wherever this
     * function was called from
     * @Param numLines is the counted number of lines. The out keyword means that the value is returned
     * to the paramter passed when calling the function
     * @Param numArcs is the counted number of arcs. The out keyword means that the value is returned
     * to the paramter passed when calling the function
     * @Param numCircles is the counted number of circles. The out keyword means that the value is returned
     * to the paramter passed when calling the function. As far as I can tell there should only ever be one
     * circle in a feature, and should be the only entity in the list
     */
    public void CountEntities(List<Entity> entityList, out int numLines, out int numArcs, out int numCircles, out int numEllipses)
    {
        numLines = 0;
        numArcs = 0;
        numCircles = 0;
        numEllipses = 0;

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
            else if (entityList[i] is Ellipse)
            {
                numEllipses++;
            }
            else
            {
                Console.WriteLine("Error: Cannot detect entity type.");
                break;
            }
        }
    }

    /*
     * Function that calls several other functions to determine this feature's type. Outside of testing this
     * should be called on every feature, including seperated perimeter features
     */
    public void DetectFeatures()
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
        else if (CheckGroup2A(out type))
        {
            FeatureType = type;
        }
        else
        {
            //Console.WriteLine("Error: Cannot assign feature type.");
        }

        //Finally Add the perimeter features
        CheckGroup4();
        CheckGroup5();
        CheckGroup6(); 

        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    /* 
     * Checks the baseEntityList to see if this feature is one of the Group 1B features
     * 
     * @Param numCircles, numLines, numArcs is the number of the respective entities in the EntityList
     * @Param type is used as a return value with the out keyword
     * @Return true if the type was detected
     */
    internal bool CheckGroup1C()
    {
        if (numLines != 3) return false;
        else if (numArcs == 0)
        {
            //Check for arcs, if there are non see if all lines are touching

            //if there are arcs, track which lines touch which arcs, see if these are triangles and mark the corner features
        }
    }
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
        else if (numArcs == 2 && numLines == 2 && getBothLinesAndDetermineParallelization())
        {
            if (DoAnglesAddTo360())
            {
                Arc arc1 = new Arc(0, 0, 0, 0, 0);
                Arc arc2 = new Arc(0, 0, 0, 0, 0);
                Line line = new Line(0, 0, 0, 0);
                bool gotArc1 = false;
                bool gotArc2 = false;
                bool gotLine = false;
                int index = 0;
                //Get one of the 2 lines and arcs to run isArcConcave
                while (!gotArc1 || !gotLine || !gotArc2)
                {
                    if (baseEntityList[index] is Arc)
                    {
                        if (!gotArc1)
                        {
                            arc1 = (Arc)baseEntityList[index];
                            gotArc1 = true;
                        }
                        else
                        {
                            arc2 = (Arc)baseEntityList[index];
                            gotArc2 = true;
                        }
                    }
                    else if (baseEntityList[index] is Line)
                    {
                        line = (Line)baseEntityList[index];
                        gotLine = true;
                    }
                    index++;
                }
                if (!IsArcConcave(arc1, line) && !IsArcConcave(arc2, line))
                {
                    type = PossibleFeatureTypes.Group1B2;
                    return true;
                }
            }
        }

        // set a dummy type and return false.
        type = PossibleFeatureTypes.Punch;
        return false;
    }

    internal bool CheckGroup2A(out PossibleFeatureTypes type)
    {
        if (numArcs >= 2 && numCircles == 0)
        {
            //Possible ellipse with arcs
            if (numArcs > 2 && numLines == 0)
            {
                if (DoAnglesAddTo360())
                {
                    if (IsEllipse())
                    {
                        type = PossibleFeatureTypes.Group2A;
                        return true;
                    }
                }
            }
            //Possible bowtie
            else if (numLines == 2 && getBothLinesAndDetermineParallelization())
            {
                if (IsBowtie())
                {
                    type = PossibleFeatureTypes.Group2A;
                    return true;
                }
            }
        }
        //Ellipse entity check
        else if (numLines == 0 && numArcs == 0 && numCircles == 0 && numEllipses == 1)
        {
            if ((baseEntityList[0] as Ellipse).IsFullEllipse)
            {
                type = PossibleFeatureTypes.Group2A;
                return true;
            }
        }
        type = PossibleFeatureTypes.Punch;
        return false;
    }

     //Checks if the angles of all the arcs add up to 360
    internal bool DoAnglesAddTo360()
    {
        double sumAngles = 0;
        baseEntityList.ForEach(entity =>
        {
            if (entity is Arc)
            {
                sumAngles += (entity as Arc).CentralAngle;
            }
        });
        if (sumAngles > 359.9 && sumAngles < 360.09)
        {
            return true;
        }
        return false;
    }

    internal bool IsEllipse()
    {
        //Ensures the porgam will not crash if called in other circumstances
        if (numCircles != 0 || numLines != 0)
        {
            return false;
        }
        //List that will be sorted in order of touching arcs
        List<Entity> connectedInOrder = new List<Entity>();
        connectedInOrder.Add(baseEntityList[0]);
        while (connectedInOrder.Count != baseEntityList.Count)
        {
            if (!SortEllipseListHelper(connectedInOrder, connectedInOrder[connectedInOrder.Count - 1] as Arc))
            {
                //Prevents from running infinitely in certain circumstances
                return false;
            }
        }
        //If end points do not connect, it is not an ellipse
        if (!(connectedInOrder[0] as Arc).Start.Equals((connectedInOrder[connectedInOrder.Count - 1] as Arc).End)) { return false; }
        return true;
    }

    /**
     * Detects the next touching arc in a base entity list of arcs
     */
    private bool SortEllipseListHelper(List<Entity> connectedInOrder, Arc arc1)
    {
        for (int i = 0; i < baseEntityList.Count; i++)
        {
            if (arc1.End.Equals((baseEntityList[i] as Arc).Start))
            {
                connectedInOrder.Add(baseEntityList[i]);
                return true;
            }
        }
        //Ensures the while loop in IsEllipse does not run infinitely if feature is not a closed loop
        return false;
    }

    internal bool IsBowtie()
    {
        //Sharp bowties with arcs
        if (numArcs == 2)
        {
            Arc arc1 = new Arc(0, 0, 0, 0, 0);
            Arc arc2 = new Arc(0, 0, 0, 0, 0);
            Line line = new Line(0, 0, 0, 0);
            bool gotArc1 = false;
            bool gotArc2 = false;
            bool gotLine = false;
            int index = 0;
            //Get one of the 2 lines and arcs to run isArcConcave
            while (!gotArc1 || !gotLine || !gotArc2) {
                //For some reason there was not a line and arc, return
                if (index == baseEntityList.Count)
                {
                    return false;
                }
                if (baseEntityList[index] is Arc)
                {
                    if (!gotArc1)
                    {
                        arc1 = (Arc)baseEntityList[index];
                        gotArc1 = true;
                    }
                    else
                    {
                        arc2 = (Arc)baseEntityList[index];
                        gotArc2 = true;
                    }
                }
                else if (baseEntityList[index] is Line)
                {
                    line = (Line)baseEntityList[index];
                    gotLine = true;
                }
                index++;
            }
            return (IsArcConcave(arc1, line) && IsArcConcave(arc2, line));
        }
        return false;
    }

    /**
     * Retrieves 2 lines from the base entity list to determine parallelization
     */
    private bool getBothLinesAndDetermineParallelization()
    {
        Line line1 = new Line(0, 0, 0, 0);
        Line line2 = new Line(0, 0, 0, 0);
        bool gotLine1 = false;
        bool gotLine2 = false;
        int index = 0;
        //Get one of the 2 lines and arcs to run isArcConcave
        while (!gotLine1 || !gotLine2)
        {
            //For some reason there was not a line and arc, return
            if (index == baseEntityList.Count)
            {
                return false;
            }
            if (baseEntityList[index] is Line)
            {
                if (!gotLine1)
                {
                    line1 = (Line)baseEntityList[index];
                    gotLine1 = true;
                }
                else
                {
                    line2 = (Line)baseEntityList[index];
                    gotLine2 = true;
                }
            }
            index++;
        }
        return line1.isParallel(line2);
    }

    /**
     * Checks to see if the point on the center most angle of the arc is concave to
     * the line (within the bounds) or convex (extends past the bounds)
     */
    private bool IsArcConcave(Arc arc, Line line)
    {
        double middleAngle; 
        if (arc.EndAngle - arc.StartAngle < 0)
        {
            middleAngle = arc.EndAngle - ((arc.EndAngle - arc.StartAngle + 360) / 2);
        }
        else
        {
             middleAngle = arc.EndAngle - ((arc.EndAngle - arc.StartAngle) / 2);
        }
        if (middleAngle < 0)
        {
            middleAngle += 360;
        }
        Point edgeOfArc = new Point(arc.Radius * Math.Cos(middleAngle * Math.PI / 180) + arc.Center.X, arc.Radius * Math.Sin(middleAngle * Math.PI / 180) + arc.Center.Y);
        //Essentially vectors to use basic linear algebra so that they are perpendicular to
        //  the vector that extends from the center of the arc to the edge point. Need to
        //  create 2 vectors because the touching line that was grabbed is random
        Line cw = new Line(edgeOfArc.X, edgeOfArc.Y, (arc.Center.Y - edgeOfArc.Y) + edgeOfArc.X, -1 * (arc.Center.X - edgeOfArc.X) + edgeOfArc.Y);
        Line ccw = new Line(edgeOfArc.X, edgeOfArc.Y, -1 * (arc.Center.Y - edgeOfArc.Y) + edgeOfArc.X, (arc.Center.X - edgeOfArc.X) + edgeOfArc.Y);
        //Slope of perpendicular line will be vertical
        if (line.DoesIntersect(cw) || line.DoesIntersect(ccw))
        {
            return true;
        }
        return false;
    }
    
    /*
     * Checks the perimiterEntityList to detect if any of the features there belong to group 4,
     * then adds any we find to the perimiterFeature list 
     */
    public void CheckGroup4()
    {
        if(PerimeterEntityList == null) { return;  }

        foreach (List<Entity> feature in PerimeterEntityList)
        {
            bool g4Detected = false;
            Line tempLine = null;
            CountEntities(feature, out int lineCount, out int arcCount, out int circCount, out int ellipseCount);

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

    /* Checks the perimiter entity list to detect if any of the features there belong to group 5, 
     * then adds any we find to the perimiterFeature list 
     */
    internal void CheckGroup5()
    {
        if(PerimeterEntityList == null) { return; }

        foreach (List<Entity> feature in PerimeterEntityList)
        {
            CountEntities(feature, out int lineCount, out int arcCount, out int circCount, out int ellipseCount);
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

    // Very simillar check to Group5, changes the entity count constraints
    internal void CheckGroup6()
    {
        if (PerimeterEntityList == null){ return; }
        
        foreach (List<Entity> feature in PerimeterEntityList)
        {
            CountEntities(feature, out int lineCount, out int arcCount, out int circCount, out int ellipseCount);
            if(lineCount < 2 || arcCount < 2 || arcCount > 4 || circCount != 0) { return; } 
            foreach(Entity entity in feature)
            {
                if(entity is Arc && ((entity as Arc).CentralAngle != 90 || (entity as Arc).CentralAngle != 180)) { break; }
            }

            // If the feature is group6, add it to the list! 
            if(HasTwoParalellLine(feature))
            {
                PerimeterFeatures.Add(PerimeterFeatureTypes.Group6);
            }
        }
    }

    /*
     * Overriding the Equals method to compare two Feature objects
     * 
     * @Param obj is the object being compared to this
     * @Return true if the objects are equal
    */
    public override bool Equals(object obj)
    {
        if ((!(obj is Feature)) || (obj == null))
        {
            return false;
        }
        else if (obj == this) return true;
        /*
        * Way to quickly determine that it's likely that the features are equal.
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
        if ( ((Feature)obj).numLines == this.numLines
            && ((Feature)obj).numCircles == this.numCircles
            && ((Feature)obj).numArcs == this.numArcs
            && Math.Abs( ((Feature)obj).perimeter - this.perimeter) < Entity.EntityTolerance )
        {
            //Genuinly my first time ever using lambda expression for something actually useful
            //sort both lists by length
            EntityList.Sort( (x, y) => x.Length.CompareTo(y.Length) );
            ((Feature)obj).EntityList.Sort( (x, y) => x.Length.CompareTo(y.Length) );

            //For each entity in this.EntityList check for a corresponding entity obj.EntityList
            bool equalLists = true;
            foreach (Entity j in ((Feature)obj).EntityList)
            {
                if (!EntityList.Any(e => Math.Abs(e.Length - j.Length) < Entity.EntityTolerance)) 
                { 
                    equalLists = false;
                    break;
                }
            }

            return equalLists;
        }
        else return false;
    }

    public bool Compare(object obj)
    {
        if ((!(obj is Feature)) || (obj == null))
        {
            return false;
        }
        else if (obj == this) return true;
     
        if (((Feature)obj).numLines == this.numLines
            && ((Feature)obj).numCircles == this.numCircles
            && ((Feature)obj).numArcs == this.numArcs
            && Math.Abs(((Feature)obj).perimeter - this.perimeter) < Entity.EntityTolerance)
        {
            //Genuinly my first time ever using lambda expression for something actually useful
            //sort both lists by length
            EntityList.Sort((x, y) => x.Length.CompareTo(y.Length));
            ((Feature)obj).EntityList.Sort((x, y) => x.Length.CompareTo(y.Length));

            //For each entity in this.EntityList check for a corresponding entity in tmpList
            //Remove the entity if it's found, and set the corresponding value in validArray to true
            bool equalLists = true;
            foreach (Entity j in ((Feature)obj).EntityList)
            {
                if (!EntityList.Any(e => Math.Abs(e.Length - j.Length) < Entity.EntityTolerance))
                {
                    equalLists = false;
                    break;
                }
            }

            return equalLists;
        }
        else return false;
    }


    /*
     * Recursive function that calls extendAllEntitiesHelper. Useful for testing if you want to extend entities
     * on a certain list without changing the baseEntityList
     * 
     * @Param myEntityList parameter that extendedEntityList is set equal to
    */
    public void extendAllEntities(List<Entity> myEntityList)
    {
        ExtendedEntityList = myEntityList;
        extendAllEntitiesHelper();
    }

    /*
     *  Recursive function that calls extendAllEntitiesHelper. Sets extendedEntityList to EntityList. 
     *  This is the main function that should be called to extend entities
    */
    public void extendAllEntities()
    {
        ExtendedEntityList = new List<Entity>(EntityList);
        extendAllEntitiesHelper();
    }

    /*
     * This is a recursive helper function to extend every line in ExtendedEntityList.
     * Should be N^N runtime seeing as the nested for loops is N^2, then it is called recursively with N-1 every time.
     * This makes it (((N!)^2) * N!) which is 
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
    }

    /* Method that takes two lines and extends them to touch if they are:
     * 1. not already touching
     * 2. are parallel or perpendicular
     * adds extended line(parallel) or lines(perpendicular) to extendedEntityList
     * Perpendicular functionality has been commented out due to inconsistent slopes of lines,
     * which means a perpendicular angle of intersection is not garunteed on features it should be
     * 
     * @Param line1 is the first line being extended
     * @Param line2 is the second line being extended
     * @Return true if succesfully extended. Could be false if the two lines dont have an intersect point,
     * arent the same infinate line, or already touch
     */
    private bool extendTwoLines(Line line1, Line line2)
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

    /*
     * Function that seperates the base entities, which will have been extended, if possible, at this point,
     * from ExtendedEntityList into baseEntityList. Most logic for seperation lies in seperateBaseEntitiesHelper
     * 
     * @Return true if successfully seperates base entities
     */
    public bool seperateBaseEntities()
    {
        if (ExtendedEntityList[0] is Circle) // case where the feature contains a circle
        {
            if (ExtendedEntityList.Count == 1 && baseEntityList.Count == 0) // it should be the only entity in the list
            {
                baseEntityList.Add((Circle)ExtendedEntityList[0]); // adds the circle to the baseEntityList
                return true;
            }
            else { return false; } // means that it contains a circle but is not the only entity
        }

        // lists to pass to the helper function
        Stack<Entity> curPath = new Stack<Entity>();
        List<Entity> testedEntities = new List<Entity>();

        Entity head = ExtendedEntityList[0]; // default head is the first index of ExtendedEntityList
        foreach(Entity entity in ExtendedEntityList)
            // this finds the entity with the greatest length and makes it the head to hopefully reduce runtime
        {
            if (entity.Length > head.Length)
            {
                head = entity;
            }
        }

        curPath.Push(head); // pushes the head to the current path
        if (seperateBaseEntitiesHelper(curPath, testedEntities, head))
            // if it can find a path
        {
            baseEntityList = curPath.ToList(); // converts the stack to an Entity<List>
            baseEntityList.Reverse(); // reverses the order of it since the iterator that converts the stack flips it
            return true;
        }
        return false;
    }

    /*
     * recursive helper function to find a closed shape with extended lines
     * 
     * @Param curPath is the current path that has been taken
     * @Param testedEnttiies is a list of enties that have been visited
     * @Param head is the target entity that is trying to loop back through
     * @Return true if a path has been found
     */
    private bool seperateBaseEntitiesHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head)
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

    /*
     * function that finds a path from start to target in Entity list
     * 
     * @Param start is the entity that this algorithm starts
     * @Param target is the entity that is trying to be reached
     * @Return the path from start to target. Null if no viable path
     */
    private List<Entity> findPathFromStartToTargetInEntityList( Entity start, Entity target)
    {
        if (EntityList.Contains(start) && EntityList.Contains(target))
        {
            Stack<Entity> path = new Stack<Entity>();
            if (findPathFromStartToTargetInEntityListHelper(path, new List<Entity>(), start, target)) 
            {
                List<Entity> temp = path.ToList();
                temp.Reverse();
                return temp; 
            }
            else { return null; }
        }
        else { return null; }
    }

    /*
     * Helper function to find the path from head to traget. Contains the actual logic for this task
     * 
     * @Param curPath is the current path taken
     * @Param testedEntities contains all visited entities
     * @Param head is the starting node
     * @Param target is the entity trying to be reached
     * @Return true if path is found
     */
    private bool findPathFromStartToTargetInEntityListHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head, Entity target)
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

    /*
     * Function that uses finds the path from the two parents of all extended lines and adds the path as a group of
     * entities at new index in PerimeterEntityList
     * 
     * @Return true if a valid path is found and seperated successfully
     */
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


