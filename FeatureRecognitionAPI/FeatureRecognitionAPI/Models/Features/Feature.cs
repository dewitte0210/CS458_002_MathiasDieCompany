﻿/*
 * For borders look at every feature that is not inside another feature (that is a border)
 * calc number up using number of borders
 *
 * for optimization before detecting features ignore all entity groups outside
 * the first border and calculates features only for that one
 */

using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHibernate.Dialect.Function;
using static FeatureRecognitionAPI.Models.Utility.Angles;

public class Feature
{
    [JsonProperty] public PossibleFeatureTypes FeatureType { get; set; }
    
    [JsonProperty] public List<Entity> EntityList { get; set; } //list of touching entities that make up the feature
    [JsonProperty] public bool KissCut;
    [JsonProperty] public int multipleRadius;
    [JsonProperty] public bool roundedCorner;
    [JsonProperty] public double perimeter;
    [JsonProperty] public double diameter;
    [JsonProperty] public int count;
    //[JsonProperty] public int NumChamfers = 0;
    public List<ChamferGroup> ChamferList = new List<ChamferGroup>();

    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]

    // list of entities after extending them all
    internal List<Entity> ExtendedEntityList { get; set; } 

    // what the list is sorted into from extendedEntityList which should only
    // contain entities that make up the base shape and possibly corner features
    internal List<Entity> baseEntityList;

    // 2-dimensional list where each list at each index is a group of
    // touching entities that make up a single perimeter feature for
    // the original feature
    //EXAMPLE: <[list for Mitered notch], [list for radius notch], [list for Group17], [list for chamfered corner]>
    // You will have to run detection for perimeter features for each index of this list
    internal List<Feature> PerimeterFeatureList;

    public int GetNumLines() { return numLines; }
    public int GetNumArcs() { return numArcs; }
    public int GetNumCircles() { return numCircles; }
    public int GetNumEllipses() { return numEllipses; }

    private int numEllipses = 0;
    private int numLines = 0;
    private int numArcs = 0;
    private int numCircles = 0;
    private const double LOW_ANGLE_TOLERANCE = 359.9;
    private const double HIGH_ANGLE_TOLERANCE = 360.09;

    #region Constructors

    /*
     * Constructor that passes in an EntityList for the feature. Feature detection is expected to be
     * called on a feature using this constructor. This was mostly used for testing when wanting to
     * avoid feature detection in the constructor. Could probably be deleted at this point since
     * feature detection was moved out of the constructors.
     *
     * @Param EntityList is the EntityList being passed into the feature. could be a base feature,
     * that includes perimeter features, or just the list for a perimeter feature
     * @Param kissCut stores whether the feature is kiss cut
     * @Param multipleRadius stores whether the feature has multiple radiuses for rounded corners
     */
    public Feature(List<Entity> entityList, bool KissCut, int multipleRadius)
    {
        EntityList = entityList;
        this.KissCut = KissCut;
        this.multipleRadius = multipleRadius;
        baseEntityList = new List<Entity>();
        ExtendedEntityList = new List<Entity>();
        PerimeterFeatureList = new List<Feature>();

        CalcPerimeter();
    }

    /*
     * Constructor that is expected to be used the most as it just passes in the EntityList for the
     * feature and detection, along with all other fields will be calculated based off this list in
     * a separate function
     *
     * @Param EntityList is the list being passed into the feature which includes all base entities
     * of the feature, including the perimeter features entities, unless the feature is just a
     * perimeter one
     */
    public Feature(List<Entity> entityList)
    {
        EntityList = entityList;
        ConstructFromEntityList();
    }

    /// <summary>
    /// create a feature when the feature type is known
    /// </summary>
    /// <param name="pft">PossibleFeatureType when known</param>
    /// <param name="entityList">Entity list that makes up the feature</param>
    public Feature(PossibleFeatureTypes pft, List<Entity> entityList)
    {
        FeatureType = pft;
        EntityList = entityList;

        ConstructFromEntityList();
    }

    #endregion

    // This got moved out so Initialization can be called after populating its EntityList
    public void ConstructFromEntityList()
    {
        count = 1;
        multipleRadius = 1;
        baseEntityList = new();
        ExtendedEntityList = new List<Entity>();
        PerimeterFeatureList = new List<Feature>();

        CountEntities(EntityList, out numLines, out numArcs, out numCircles, out numEllipses);

        //calculate and set the perimeter of the feature
        CalcPerimeter();
    }
    
    #region FeatureDetection

    public void CountEntities()
    {
        CountEntities(EntityList, out numLines, out numArcs, out numCircles, out numEllipses);
    }

    /*
     * Counts the Lines, Arcs, and Circles in the EntityList.
     *
     * @Param EntityList is the list that is being looped through. Note that it is passed by reference
     * and any changes to the list in this function will change the list in the scope of wherever this
     * function was called from
     * @Param numLines is the counted number of lines. The out keyword means that the value is returned
     * to the parameter passed when calling the function
     * @Param numArcs is the counted number of arcs. The out keyword means that the value is returned
     * to the parameter passed when calling the function
     * @Param numCircles is the counted number of circles. The out keyword means that the value is returned
     * to the paramter passed when calling the function. As far as I can tell there should only ever be one
     * circle in a feature, and should be the only entity in the list
     */
    public void CountEntities(List<Entity> entityList, out int numLines, out int numArcs, out int numCircles,
        out int numEllipses)
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
        if (baseEntityList.Count == 0) {baseEntityList = new(EntityList);} // should only happen if line extension and separation were skipped
        
        // BASE SHAPE DETECTION:
        if (!CheckGroup1B()
            && !CheckGroup1C() 
            && !CheckGroup6Base()
            && !CheckGroup1A()
            && !CheckGroup2A()
            && !CheckGroup10()
            && !CheckGroup11()
            && !CheckGroup12())
        {
            Console.WriteLine("Error: Cannot assign base feature type.");
            FeatureType = PossibleFeatureTypes.Unknown;
        }
        
        // PERIMETER DETECTION:
        FlagGroup3();
        CheckGroup4();
        CheckGroup5();
        CheckGroup6Perimeter();
        CheckGroup9();
        CheckGroup17();
            
        //calculate and set the perimeter of the feature
        CalcPerimeter();

        //check if the feature has multiple radii
        CheckMultipleRadius();
    }

    #region BaseDetection
    
    #region Group1A

    public bool CheckGroup1A()
    {
        if (numLines >= 4)
        {
            if (numArcs == 0)
            {
                FeatureType = PossibleFeatureTypes.Group1A1;
                return true;
            }
            FeatureType = PossibleFeatureTypes.Group1A2;
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region Group1B

    /*
     * Checks the baseEntityList to see if this feature is one of the Group 1B features
     *
     * @Param numCircles, numLines, numArcs is the number of the respective entities in the EntityList
     * @Param type is used as a return value with the out keyword
     * @Return true if the type was detected
     */
    internal bool CheckGroup1B()
    {
        // Entity is just a circle
        if (numCircles == 1 && numLines == 0 && numArcs == 0)
        {
            Circle c = baseEntityList[0] as Circle;
            if (c.Radius * 2 <= 1.75)
            {
                FeatureType = PossibleFeatureTypes.Punch;
            }
            else
            {
                FeatureType = PossibleFeatureTypes.Group1B1;
            }

            return true;
        }
        //Entity contains the correct number of lines and arcs to be a rounded rectangle add up the degree measuers
        //of the arcs and make sure they are 360
        if (numArcs == 2 && numLines == 2 && IsSubshapeRectangle())
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
                    FeatureType = PossibleFeatureTypes.Group1B2;
                    return true;
                }
            }
        }

        // set a dummy type and return false.
        FeatureType = PossibleFeatureTypes.Punch;
        return false;
    }

    #endregion

    #region Group1C

    public bool CheckGroup1C()
    {
        //Triange base shape needs 3 lines
        if (numLines != 3)
        {
            return false;
        }
        
        // TODO: This is wrong
        //If there are 3 lines and zero arcs then it should be a triangle
        if (numArcs == 0)
        {
            FeatureType = PossibleFeatureTypes.Group1C;
            return true;
        }
        if (numArcs > 3)
        {
            return false;
        }
        //At this point arcs is between 1-3 and lines = 3
            switch (numArcs)
            {
                case 1:
                    {
                        //Find the arc
                        int arcIndex = 0;
                        for (int i = 0; i < baseEntityList.Count; i++)
                        {
                            if (baseEntityList[i] is Arc)
                            {
                                arcIndex = i;
                                break;
                            }
                        }

                    //Array of 2 entities to contain lines touching the arc.
                    Entity[] touchingArc = new Entity[2];
                    int eIndex = 0;
                    //Find the two lines
                    for (int i = 0; i < baseEntityList.Count; i++)
                    {
                        if (baseEntityList[i] is Line && eIndex < 2)
                        {
                            if (Entity.IntersectLineWithArc((Line)baseEntityList[i],
                                    (Arc)baseEntityList[arcIndex]))
                            {
                                touchingArc[eIndex] = (Line)baseEntityList[i];
                                eIndex++;
                            }
                        }

                        if (eIndex == 2)
                        {
                            break;
                        }
                    }

                    if (touchingArc[0] is Line && touchingArc[1] is Line)
                    {
                        if (!Entity.IntersectLineWithLine((Line)touchingArc[0], (Line)touchingArc[1]))
                        {
                            FeatureType = PossibleFeatureTypes.Group1C;
                            return true;
                        }
                    }
                    return false;
                    }
                case 2:
                    {
                        //Basically same code as before, but instead of a parallel check ensure arcs aren't touching

                        int arcIndex = 0;
                        for (int i = 0; i < baseEntityList.Count; i++)
                        {
                            if (baseEntityList[i] is Arc)
                            {
                                arcIndex = i;
                                break;
                            }
                        }

                    Entity[] touchingArc = new Entity[2];
                    int eIndex = 0;
                    //Find the two lines
                    for (int i = 0; i < baseEntityList.Count; i++)
                    {
                        if (baseEntityList[i] is Line && eIndex < 2)
                        {
                            if (Entity.IntersectLineWithArc((Line)baseEntityList[i],
                                    (Arc)baseEntityList[arcIndex]))
                            {
                                touchingArc[eIndex] = (Line)baseEntityList[i];
                                eIndex++;
                            }
                        }
                        else if (baseEntityList[i] is Arc && eIndex < 2)
                        {
                            if (Entity.IntersectArcWithArc((Arc)baseEntityList[i],
                                    (Arc)baseEntityList[arcIndex]))
                            {
                                touchingArc[eIndex] = (Arc)baseEntityList[i];
                                eIndex++;
                            }
                        }

                            if (eIndex == 2)
                            {
                                break;
                            }
                        }

                        if (touchingArc[0] is Line && touchingArc[1] is Line)
                        {
                            FeatureType = PossibleFeatureTypes.Group1C;
                            return true;
                        }
                        return false;
                    }
                case 3:
                    {
                        Entity[] arcList = new Entity[2];
                        //Find two arcs
                        int arcIndex = 0;
                        for (int i = 0; i < baseEntityList.Count; i++)
                        {
                            if (baseEntityList[i] is Arc)
                            {
                                arcList[arcIndex] = (Arc)baseEntityList[i];
                                arcIndex++;
                            }

                            if (arcIndex == 2) break;
                        }

                    arcIndex = 0;
                    //ArcList has 2 arcs, check entities touching both (at least one line should be touching both arcs, so only 3 entities should be added )
                    Entity[] touchingArc = new Entity[3];
                    int eIndex = 0;
                    //Find the two lines
                    for (int i = 0; i < baseEntityList.Count; i++)
                    {
                        if (baseEntityList[i] is Line && eIndex < 4)
                        {
                            if (Entity.IntersectLineWithArc((Line)baseEntityList[i], (Arc)arcList[0])
                                || Entity.IntersectLineWithArc((Line)baseEntityList[i], (Arc)arcList[1]))
                            {
                                touchingArc[eIndex] = (Line)baseEntityList[i];
                                eIndex++;
                            }
                        }
                        else if (baseEntityList[i] is Arc && eIndex < 4)
                        {
                            //If not equal to arc at 0 and 1
                            if (!arcList[0].Equals((Arc)baseEntityList[i])
                                && !arcList[1].Equals((Arc)baseEntityList[i])
                                //And if the arc intersects with the arc at 0 or at 1
                                && (Entity.IntersectArcWithArc((Arc)baseEntityList[i], (Arc)arcList[0])
                                    || Entity.IntersectArcWithArc((Arc)baseEntityList[i], (Arc)arcList[1])))
                            {
                                touchingArc[eIndex] = (Arc)baseEntityList[i];
                                eIndex++;
                            }
                        }

                            if (eIndex == 3)
                            {
                                break;
                            }
                        }

                        //Should have a list of 4 entities, if any of them are arcs, return false (arc is touching an arc)
                        foreach (Entity entity in touchingArc)
                        {
                            if (entity is Arc)
                            {
                                return false;
                            }
                        }

                        FeatureType = PossibleFeatureTypes.Group1C;
                        return true;
                    }
                default:
                    return false;
            }
        }

        //If somehow there is no decision made by this point then there is an error

    #endregion

    #region Group2A

    /**
     * Checks if a feature is Group 2A (elliptical features).
     * 
     * Returns the possible feature type.
     */
    internal bool CheckGroup2A()
    {
        if ((numArcs >= 2 || numEllipses >= 2) && numCircles == 0)
        {
            //Possible ellipse with arcs
            if (numArcs > 2 && numLines == 0 && numEllipses == 0)
            {
                if (DoAnglesAddTo360())
                {
                    if (IsEllipse())
                    {
                        FeatureType = PossibleFeatureTypes.Group2A1;
                        return true;
                    }
                }
            }
            //Possible bowtie
            else if (numLines == 2)
            {
                if (IsBowtie() && IsSubshapeRectangle())
                {
                    FeatureType = PossibleFeatureTypes.Group2A2;
                    return true;
                }
            }
        }
        //Ellipse entity check
        else if (numLines == 0 && numArcs == 0 && numCircles == 0 && numEllipses == 1)
        {
            if ((baseEntityList[0] as Ellipse).IsFullEllipse)
            {
                FeatureType = PossibleFeatureTypes.Group2A1;
                return true;
            }
        }

        FeatureType = PossibleFeatureTypes.Punch;
        return false;
    }

    /**
     * Checks if a list of arcs forms an ellipse
     */
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
        if (!(connectedInOrder[0] as Arc).Start.Equals((connectedInOrder[connectedInOrder.Count - 1] as Arc).End))
        {
            return false;
        }

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

    /**
     * Given a list of entities that could be a form of a bowtie, this function ensures a bowtie is
     * the feature
     */
    internal bool IsBowtie()
    {
        //  Counts the num of concave/convex pieces
        int concaveCount = 0;
        int convexCount = 0;
        //  Temp variables used to help determine when the curve switches concavity
        int tempConcaveCount = 0;
        int tempConvexCount = 0;
        for (int i = 0; i < baseEntityList.Count; i++)
        {
            // If a line is hit, concavity must be recorded
            if (baseEntityList[i] is Line)
            {
                if (tempConcaveCount > 0)
                {
                    concaveCount++;
                    tempConcaveCount = 0;
                }
                else if (tempConvexCount > 0)
                {
                    convexCount++;
                    tempConvexCount = 0;
                }
            }
            else
            {
                if (DetermineConcavity(baseEntityList[i]))
                {
                    //  If previous curve was convex, there is a switch in concavity
                    if (tempConvexCount > 0)
                    {
                        convexCount++;
                        tempConvexCount = 0;
                    }
                    // Checks that the angles of connecting curves are the same
                    if (i != 0 && tempConcaveCount > 0)
                    {
                        if (!IsSmoothCurve(baseEntityList[i - 1], baseEntityList[i])) { return false; }
                    }
                    tempConcaveCount++;
                }
                //  If previous curve was concave, there is a switch in concavity
                else
                {
                    if (tempConcaveCount > 0)
                    {
                        concaveCount++;
                        tempConcaveCount = 0;
                    }
                    // Checks that the angles of connecting curves are the same
                    if (i != 0 && tempConvexCount > 0)
                    {
                        if (!IsSmoothCurve(baseEntityList[i - 1], baseEntityList[i])) { return false; }
                    }
                    tempConvexCount++;
                }
            }
        }
        // Anything leftover gets added
        if (tempConcaveCount > 0) { concaveCount++; }
        else if (tempConvexCount > 0) { convexCount++; }
        return concaveCount == 2 && convexCount == 4;
    }

    /**
     * Combs through the base entity list to determine if the entity is concave to the shape or not
     * @param index - The index of the entity being checked
     */
    private bool DetermineConcavity(Entity entity)
    {
        if (!(entity is Arc || entity is Ellipse)) { return false; }
        //  Variables used to extend the line that is used for concavity detection to ensure it passes through
        //  the entire shape
        Point minPoint = FindMinPoint();
        Point maxPoint = FindMaxPoint();
        double maxLength = 2 * Point.Distance(maxPoint, minPoint);
        Line ray;

        int numIntersections = 0;
        int numEndPointIntersections = 0;
        //  Finds the middle angle of the curve and calculates the ray
        if (entity is Arc arc) 
        { ray = arc.VectorFromCenter(Angles.DegToRadians(arc.AngleInMiddle())); }
        else
        {
            Ellipse tempEntity = entity as Ellipse;
            double startAngle = tempEntity.StartParameter + Math.Atan2(tempEntity.MajorAxisEndPoint.Y - tempEntity.Center.Y, tempEntity.MajorAxisEndPoint.X - tempEntity.Center.X);
            double endAngle = tempEntity.EndParameter + Math.Atan2(tempEntity.MajorAxisEndPoint.Y - tempEntity.Center.Y, tempEntity.MajorAxisEndPoint.X - tempEntity.Center.X);
            ray = (entity as Ellipse).vectorFromCenter((endAngle - startAngle) / 2);
        }

        //  Entends the ray
        Point unitVector = new Point((ray.EndPoint.X - ray.StartPoint.X) / ray.Length, (ray.EndPoint.Y - ray.StartPoint.Y) / ray.Length);
        Point newEndPoint = new Point(ray.StartPoint.X + maxLength * unitVector.X, ray.StartPoint.Y + maxLength * unitVector.Y);
        ray = new Line(ray.StartPoint.X, ray.StartPoint.Y, newEndPoint.X, newEndPoint.Y);

        //  Runs through the base list and finds the num of intersections with the shape
        //  Checks for end point intersections because it will detect 2 end point intersections
        //  per actual intersection
        for (int i = 0; i < baseEntityList.Count; i++)
        {
            if (!baseEntityList[i].DoesIntersect(ray))
            {
                continue;
            }

            numIntersections++;
            if (baseEntityList[i] is Line line)
            {
                Point? intersection = Entity.GetIntersectPoint(ray, line);
                if (intersection == null)
                {
                    continue;
                }

                if (intersection.Equals(line.StartPoint) || intersection.Equals(line.EndPoint))
                    numEndPointIntersections++;
            }
            else if (baseEntityList[i] is Arc arc1)
            {
                Point? intersection = Entity.GetIntersectPoint(ray, arc1);
                if (intersection == null)
                {
                    continue;
                }

                if (intersection.Equals(arc1.Start) || intersection.Equals(arc1.End))
                {
                    numEndPointIntersections++;
                }
            }

            else if (baseEntityList[i] is Ellipse ellipse)
            {
                Point? intersection = Entity.GetIntersectPoint(ray, ellipse);
                if (intersection == null)
                {
                    continue;
                }

                if (intersection.Equals(ellipse.StartPoint) || intersection.Equals(ellipse.EndPoint)){
                    numEndPointIntersections++;
                }
            }
        }

        //  Even num of intersections = concave
        return (numIntersections - (numEndPointIntersections / 2)) % 2 == 0;
    }

    private static bool IsSmoothCurve(Entity entity1, Entity entity2)
    {
        if (!(entity1 is Arc || entity1 is Ellipse) || !(entity2 is Arc || entity2 is Ellipse)) { return true; }
        if (entity1 is Arc && entity2 is Arc)
        {
            Arc arc1 = (Arc)entity1;
            Arc arc2 = (Arc)entity2;
            return Math.Abs(Math.Round(arc1.EndAngle, 4) - (Math.Round(arc2.StartAngle, 4))) < 1.0;
        }
        else if (entity1 is Arc && entity2 is Ellipse)
        {
            Arc arc = (Arc)entity1;
            Ellipse ellipse = (Ellipse)entity2;
            return Math.Abs(Math.Round(arc.EndAngle * Math.PI / 180, 4) - (Math.Round(ellipse.StartParameter + ellipse.Rotation, 4))) < 1.0;
        }
        else if (entity1 is Ellipse && entity2 is Arc)
        {
            Ellipse ellipse = (Ellipse)entity1;
            Arc arc = (Arc)entity2;
            return Math.Abs(Math.Round(ellipse.EndParameter + ellipse.Rotation, 4) - (Math.Round(arc.StartAngle * Math.PI / 180, 4))) < 1.0;
        }
        else
        {
            Ellipse ellipse1 = (Ellipse)entity1;
            Ellipse ellipse2 = (Ellipse)entity2;
            return Math.Abs(Math.Round(ellipse1.EndParameter + ellipse1.Rotation, 4) - (Math.Round(ellipse2.StartParameter + ellipse2.Rotation, 4))) < 1.0;
        }
    }

    /**
     * Retrieves 2 lines from the base entity list and determines if there is a
     * rectangle that forms if the two lines are connected
     */
    internal bool IsSubshapeRectangle()
    {
        Line baseLine1 = new Line(0, 0, 0, 0);
        Line baseLine2 = new Line(0, 0, 0, 0);
        bool gotLine1 = false;
        bool gotLine2 = false;
        int index = 0;
        //Retreive 2 lines for check
        while (!gotLine1 || !gotLine2)
        {
            //For some reason there were not 2 lines -> return
            if (index == baseEntityList.Count)
            {
                return false;
            }

            if (baseEntityList[index] is Line)
            {
                if (!gotLine1)
                {
                    baseLine1 = (Line)baseEntityList[index];
                    gotLine1 = true;
                }
                else
                {
                    baseLine2 = (Line)baseEntityList[index];
                    gotLine2 = true;
                }
            }

            index++;
        }

        // Temp variables for correct line check since 4 lines can be formed from the baseLine endpoints
        Line tempLine1 = new Line(baseLine1.StartPoint.X, baseLine1.StartPoint.Y, baseLine2.StartPoint.X, baseLine2.StartPoint.Y);
        Line tempLine2 = new Line(baseLine1.EndPoint.X, baseLine1.EndPoint.Y, baseLine2.EndPoint.X, baseLine2.EndPoint.Y);
        Line tempLine3 = new Line(baseLine1.StartPoint.X, baseLine1.StartPoint.Y, baseLine2.EndPoint.X, baseLine2.EndPoint.Y);
        Line tempLine4 = new Line(baseLine1.EndPoint.X, baseLine1.EndPoint.Y, baseLine2.StartPoint.X, baseLine2.StartPoint.Y);
        // Variables for final quadrilateral lines
        Line newLine1;
        Line newLine2;
        // Checks the lengths of each line to ensure the right line is used to form the quadrilateral
        if (Math.Round(tempLine1.Length, 4) + Math.Round(tempLine2.Length, 4) < Math.Round(tempLine3.Length, 4) + Math.Round(tempLine4.Length, 4))
        {
            newLine1 = tempLine1;
            newLine2 = tempLine2;
        }
        else if (Math.Round(tempLine1.Length, 4) + Math.Round(tempLine2.Length, 4) > Math.Round(tempLine3.Length, 4) + Math.Round(tempLine4.Length, 4))
        {
            newLine1 = tempLine3;
            newLine2 = tempLine4;
        }
        else
        {
            if (Math.Round(tempLine1.Length, 4) < Math.Round(tempLine2.Length, 4))
            {
                newLine1 = tempLine1;
            }
            else
            {
                newLine1 = tempLine2;
            }
            if (Math.Round(tempLine3.Length, 4) < Math.Round(tempLine4.Length, 4))
            {
                newLine2 = tempLine3;
            }
            else
            {
                newLine2 = tempLine4;
            }
        }
        return Math.Round(baseLine1.Length, 4).Equals(Math.Round(baseLine2.Length, 4)) 
            && newLine1.isPerpendicular(baseLine1) && newLine1.isPerpendicular(baseLine2) 
            && newLine2.isPerpendicular(baseLine1) && newLine2.isPerpendicular(baseLine2);
    }

    /**
     * Checks to see if the point on the center most angle of the arc is concave to
     * the line (within the bounds) or convex (extends past the bounds)
     */
    private static bool IsArcConcave(Arc arc, Line line)
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

        Point edgeOfArc = new Point(arc.Radius * Math.Cos(middleAngle * Math.PI / 180) + arc.Center.X,
            arc.Radius * Math.Sin(middleAngle * Math.PI / 180) + arc.Center.Y);
        //Essentially vectors to use basic linear algebra so that they are perpendicular to
        //  the vector that extends from the center of the arc to the edge point. Need to
        //  create 2 vectors because the touching line that was grabbed is random
        Line cw = new Line(edgeOfArc.X, edgeOfArc.Y, (arc.Center.Y - edgeOfArc.Y) + edgeOfArc.X,
            -1 * (arc.Center.X - edgeOfArc.X) + edgeOfArc.Y);
        Line ccw = new Line(edgeOfArc.X, edgeOfArc.Y, -1 * (arc.Center.Y - edgeOfArc.Y) + edgeOfArc.X,
            (arc.Center.X - edgeOfArc.X) + edgeOfArc.Y);
        //Slope of perpendicular line will be vertical
        if (line.DoesIntersect(cw) || line.DoesIntersect(ccw))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Group10

    /**
     * Checks the feature to see if it is group 10.
     * 
     * Returns the possible feature type.
     */
    internal bool CheckGroup10()
    {
        if (numLines == 2 && numArcs == 2)
        {
            // Fetch the lines and arcs
            List<Line> lines = new List<Line>();
            List<Arc> arcs = new List<Arc>();
            for (int i = 0; i < baseEntityList.Count; i++)
            {
                if (baseEntityList[i] is Line) { lines.Add((Line)baseEntityList[i]); }
                else if (baseEntityList[i] is Arc) { arcs.Add((Arc)baseEntityList[i]); }
            }
            // Find the bigger arc for correct end point orientation for math calcs
            Arc biggerArc;
            if (arcs[0].Radius > arcs[1].Radius)
            {
                biggerArc = arcs[0];
            }
            else
            {
                biggerArc = arcs[1];
            }
            // Flip end points for calc if they are touching the smaller arc
            for (int i = 0; i < lines.Count; i++)
            {
                Point intersect = Entity.GetIntersectPoint(lines[i], biggerArc);
                if (!lines[i].EndPoint.Equals(intersect))
                {
                    Point temp = lines[i].StartPoint;
                    lines[i].StartPoint = lines[i].EndPoint;
                    lines[i].EndPoint = temp;
                }
            }

            // Runs only if the arcs start and end at the same angle
            if (arcs[0].StartAngle.Equals(arcs[1].StartAngle) && arcs[0].EndAngle.Equals(arcs[1].EndAngle))
            {
                // Start and end angles stored in variables for readability
                double startAngle = Math.Round(Angles.DegToRadians(arcs[0].StartAngle), 4);
                double endAngle = Math.Round(Angles.DegToRadians(arcs[0].EndAngle), 4);
                // Case 1: Both lines are vertical
                if (Math.Round(lines[0].SlopeX, 4) == 0 && Math.Round(lines[1].SlopeX, 4) == 0)
                {
                    if (Math.Round(startAngle + endAngle, 4) == Math.Round(2 * Math.PI, 4))
                    {
                        FeatureType = PossibleFeatureTypes.Group10;
                        return true;
                    }
                }
                // Case 2: Only one is vertical
                else if (Math.Round(lines[0].SlopeX, 4) == 0 || Math.Round(lines[1].SlopeX, 4) == 0)
                {
                    // Angle of line stored in variable for readability
                    double lineAngle;
                    if (Math.Round(lines[0].SlopeX, 4) == 0)
                    {
                        lineAngle = Math.Round(Math.Atan2(lines[1].EndPoint.Y - lines[1].StartPoint.Y, lines[1].EndPoint.X - lines[1].StartPoint.X), 4);
                    }
                    else
                    {
                        lineAngle = Math.Round(Math.Atan2(lines[0].EndPoint.Y - lines[0].StartPoint.Y, lines[0].EndPoint.X - lines[0].StartPoint.X), 4);
                    }
                    if (((startAngle == Math.Round(Math.PI / 2, 4) || endAngle == Math.Round(Math.PI / 2, 4))
                        || (endAngle == Math.Round(3 * Math.PI / 2, 4) || startAngle == Math.Round(3 * Math.PI / 2, 4)))
                        && (lineAngle == startAngle
                        || lineAngle == endAngle))
                    {
                        FeatureType = PossibleFeatureTypes.Group10;
                        return true;
                    }
                }
                // Case 3: Lines are not vertical, can run Atan() function
                else
                {
                    if ((Math.Round(Math.Atan2(lines[0].EndPoint.Y - lines[0].StartPoint.Y, lines[0].EndPoint.X - lines[0].StartPoint.X), 4) == startAngle
                        || Math.Round(Math.Atan2(lines[0].EndPoint.Y - lines[0].StartPoint.Y, lines[0].EndPoint.X - lines[0].StartPoint.X), 4) == endAngle)
                        && (Math.Round(Math.Atan2(lines[1].EndPoint.Y - lines[1].StartPoint.Y, lines[1].EndPoint.X - lines[1].StartPoint.X), 4) == startAngle
                        || Math.Round(Math.Atan2(lines[1].EndPoint.Y - lines[1].StartPoint.Y, lines[1].EndPoint.X - lines[1].StartPoint.X), 4) == endAngle))
                    {
                        FeatureType = PossibleFeatureTypes.Group10;
                        return true;
                    }
                }
            }
        }
        FeatureType = PossibleFeatureTypes.Unknown;
        return false;
    }

    #endregion

    #region Group11

    /**
     * Checks the feature to see if it is group 11.
     * 
     * Returns the possible feature type.
     */
    internal bool CheckGroup11()
    {
        if (numEllipses == 0 && numCircles == 0)
        {
            // Case 1
            if (numArcs == 2 && numLines == 0)
            {
                // Keeps track of the bigger/smaller arc for the concavity check
                Arc bigArc = baseEntityList[0] as Arc;
                int bigIndex = 0;
                Arc smallArc = baseEntityList[1] as Arc;
                int smallIndex = 1;
                if (bigArc.Radius < smallArc.Radius)
                {
                    Arc temp = bigArc;
                    bigArc = smallArc;
                    bigIndex = 1;
                    smallArc = temp;
                    smallIndex = 0;
                }
                if (bigArc.Start.Equals(smallArc.Start) && bigArc.End.Equals(smallArc.End) && DetermineConcavity(bigArc) && !DetermineConcavity(smallArc))
                {
                    FeatureType = PossibleFeatureTypes.Group11;
                    return true;
                }
            }
            // Case 2
            else if (numArcs == 1 && numLines == 1)
            {
                // Fetch arc and line
                Arc arc1;
                Line line1;
                if (baseEntityList[0] is Arc)
                {
                    arc1 = (Arc)baseEntityList[0];
                    line1 = (Line)baseEntityList[1];
                }
                else
                {
                    arc1 = (Arc)baseEntityList[1];
                    line1 = (Line)baseEntityList[0];
                }
                // Check that end points connect
                if ((line1.StartPoint.Equals(arc1.Start) || line1.EndPoint.Equals(arc1.Start))
                    && (line1.StartPoint.Equals(arc1.End) || line1.EndPoint.Equals(arc1.End)))
                {
                    FeatureType = PossibleFeatureTypes.Group11;
                    return true;
                }
            }
            // Case 3
            else if (numArcs == 3 && numLines == 1)
            {
                // Fetch arcs and line
                // keeps track of index for concavity and endpoint checks
                Arc bigArc = null;
                int bigArcIndex = -1;
                Arc side1 = null;
                int side1Index = -1;
                Arc side2 = null;
                int side2Index = -1;
                Line line1 = null;
                for (int i = 0; i < baseEntityList.Count; i++)
                {
                    if (baseEntityList[i] is Arc)
                    {
                        if (bigArc is null)
                        {
                            bigArc = (Arc)baseEntityList[i];
                            bigArcIndex = i;
                        }
                        else if (side1 is null)
                        {
                            side1 = (Arc)baseEntityList[i];
                            side1Index = i;
                        }
                        else if (side2 is null)
                        {
                            side2 = (Arc)baseEntityList[i];
                            side2Index = i;
                        }
                    }
                    else
                    {
                        line1 = (Line)baseEntityList[i];
                    }
                }
                // Swap the arcs to get the correct one under the correct label
                Arc temp;
                int tempIndex;
                if (bigArc.Radius < side1.Radius)
                {
                    temp = bigArc;
                    tempIndex = bigArcIndex;
                    bigArc = side1;
                    bigArcIndex = side1Index;
                    side1 = temp;
                    side1Index = tempIndex;
                }
                if (bigArc.Radius < side2.Radius)
                {
                    temp = bigArc;
                    tempIndex = bigArcIndex;
                    bigArc = side2;
                    bigArcIndex = side2Index;
                    side2 = temp;
                    side2Index = tempIndex;
                }
                bool isSide1Convex = !DetermineConcavity(side1);
                bool isSide2Convex = !DetermineConcavity(side2);
                bool isBigArcConvex = !DetermineConcavity(bigArc);
                if (isSide1Convex && isSide2Convex && isBigArcConvex
                    && line1.EntityPointsAreTouching(side1) && line1.EntityPointsAreTouching(side2)
                    && bigArc.EntityPointsAreTouching(side1) && bigArc.EntityPointsAreTouching(side2))
                {
                    FeatureType = PossibleFeatureTypes.Group11;
                    return true;
                }
            }
        }
        FeatureType = PossibleFeatureTypes.Unknown;
        return false;
    }

    #endregion

    #region Group12

    /**
     * Checks the feature to see if it is group 12.
     * 
     * Returns the possible feature type.
     */
    internal bool CheckGroup12()
    {
        if (numCircles == 0 && numEllipses == 0 && numLines == 2)
        {
            if (numArcs == 2)
            {
                if (IsSubshapeRectangle())
                {
                    FeatureType = PossibleFeatureTypes.Group12a;
                    return true;
                }
            }
            else if (numArcs == 0)
            {
                if (Angles.IsPerpendicular((Line)baseEntityList[0], (Line)baseEntityList[1]))
                {
                    FeatureType = PossibleFeatureTypes.Group12b;
                    return true;
                }
            }
        }
        FeatureType = PossibleFeatureTypes.Unknown;
        return false;
    }

    #endregion

    #region Group6base

    /*
     * Checks the feature it is being called on to see if it is a group 6 base feature (trapezoid with radius corners).
     *
     * @return True if it is Group 6, false if not
     */
    public bool CheckGroup6Base()
    {
        if (numLines != 4 || numArcs != 4)
        {
            return false;
        }

        // Number of different matching central angles of corner features
        int matchingPairs = 0;

        for (int i = 0; i < EntityList.Count; i++)
        {
            for (int j = i + 1; j < EntityList.Count; j++)
            {
                if (EntityList[i] is Arc && EntityList[j] is Arc)
                {
                    if (Math.Abs((EntityList[i] as Arc).CentralAngle - (EntityList[j] as Arc).CentralAngle) <
                        Entity.EntityTolerance)
                    {
                        matchingPairs++;
                    }
                }
            }
        }

        if (matchingPairs == 2)
        {
            return true;
        }

        return false;
    }

    #endregion

    #endregion

    #region PerimeterDetection
    

    /*  todo: break out chamfered lines from parent feature
     *  remove chamfered line entity from parent feature and extend the lines
     *  to make the parent shape clean
     *
     *  then we can make group 1 rectangle detection better and front end
     *  will read the new chamfer feature
     */
    #region Group3

    /*  chamfered corner detection
            given 3 lines...
                if the two matching angles are equal and greater than 90
                and the angle between edge 1 and 3 are less than 180
                it is a possible chamfer

            if a shape has multiple possible chamfers
            2-3 chamfers
                if an edge is a possible chamfer, if there are any lines parallel to it,
                those should also be a possible chamfer
            4 corners
                have the user select a base chamfer
                or just use the count as it is and visualize from 4 shortest
                
        calculates in bulk so do not call per entity
        only runs detection on perimeter features since chamfers
          are only on the outside of a part
        returns true if no problems??
    */

    /// <summary>
    /// Gets a list of only lines from a list of entities
    /// </summary>
    /// <param name="entityList"></param>
    /// <returns>list of lines</returns>
    internal static List<Line> GetLinesFromEntityList(List<Entity> entityList)
    {
        List<Line> lineList = [];
        foreach (Entity entity in entityList)
        {
            if (entity is Line line)
            {
                lineList.Add(line);
            }
        }
        return lineList;
    }

    /// <summary>
    /// Searches for a line that shares start/end points with a given line
    /// </summary>
    /// <param name="originLine">the original line</param>
    /// <param name="lineList">list of lines to search through to find another line</param>
    /// <param name="fromStart">whether to search from start or end of line</param>
    /// <returns>null if no line is found, otherwise the touching line</returns>
    internal static (Line?, bool) GetTouchingLine(Line originLine, List<Line> lineList, bool fromStart = false)
    {
        Line? touchingLine = null;
        bool wasFlipped = false;

        foreach (Line searchLine in lineList)
        {
            // ignore the origin line and flipped version
            if (originLine.Equals(searchLine) || originLine.Equals(searchLine.swapStartEnd())) continue;
    
            Point originPoint = fromStart? originLine.StartPoint : originLine.EndPoint;

            // if end meets start or start meets end
            if (originPoint.Equals(fromStart ? searchLine.EndPoint : searchLine.StartPoint))
            {
                touchingLine = searchLine;
                break;
            }
            // if end meets end or start meets start
            else if (originPoint.Equals(fromStart ? searchLine.StartPoint : searchLine.EndPoint))
            {
                touchingLine = searchLine.swapStartEnd();
                wasFlipped = true;
                break;
            }
        }
        return (touchingLine, wasFlipped);
    }
    
    /// <summary>
    /// Breaks up a 1D line list into groups of touching lines that are ordered.
    /// Ordering is important because consistent line orientation is needed
    /// for accurate angle detection
    /// </summary>
    /// <param name="lineList">1D list of lines</param>
    /// <returns>list of line groups that are touching</returns>
    internal static List<List<Line>> GetOrderedLines(List<Line> lineList)
    {
        List<Line> baseLineList = lineList.ToList();
        List<List<Line>> orderedLineList = [];
        
        while (baseLineList.Count > 0)
        {
            // search from end
            Line currentEndLine = baseLineList[0];
            Line currentStartLine = baseLineList[0];
            baseLineList.RemoveAt(0);
            List<Line> lineGroup = [currentEndLine];
            bool exhaustedEndSearch = false;
            bool exhaustedStartSearch = false;

            while (!exhaustedEndSearch)
            {
                (Line? possibleLine, bool wasFlipped) = GetTouchingLine(currentEndLine, lineList);

                //if null or already in linegroup, meaning found end of line loop
                if (possibleLine == null 
                    || lineGroup.Contains(possibleLine)
                    || lineGroup.Contains(possibleLine.swapStartEnd()))
                {
                    exhaustedEndSearch = true;
                    break;
                }
                
                currentEndLine = possibleLine;
                baseLineList.Remove(wasFlipped ? currentEndLine.swapStartEnd() : currentEndLine);
                lineGroup.Add(possibleLine);
            }
            while (!exhaustedStartSearch)
            {
                (Line? possibleLine, bool wasFlipped) = GetTouchingLine(currentStartLine, lineList);

                //if null or already in lineGroup, meaning found end of line loop
                if (possibleLine == null 
                    || lineGroup.Contains(possibleLine)
                    || lineGroup.Contains(possibleLine.swapStartEnd()))
                {
                    exhaustedStartSearch = true;
                    break;
                }
                
                currentStartLine = possibleLine;
                baseLineList.Remove(wasFlipped ? currentStartLine.swapStartEnd() : currentStartLine);
                lineGroup.Insert(0, possibleLine);
            }
            orderedLineList.Add(lineGroup);            
        }
        return orderedLineList;
    }
    
    /// <summary>
    /// Gets a list of possible chamfer lines from a base list of lines
    /// </summary>
    /// <param name="orderedLineList"> list of lines, possible chamfers in this list
    /// will be flagged as such</param>
    /// <returns>list of lines where each line has a chamfer type of possible</returns>
    private void SetPossibleChamfers(List<List<Line>> orderedLineList)
    {
        foreach (List<Line> lineGroup in orderedLineList)
        {
            if (lineGroup.Count < 3) continue;
            for (int i = 0; i < lineGroup.Count; i++)
            {
                Line lineA = lineGroup[i];
                Line lineB = lineGroup[(i + 1) % lineGroup.Count];
                Line lineC = lineGroup[(i + 2) % lineGroup.Count];

                //need to verify orientation of lines
                Angle ab = GetAngle(lineA, lineB);
                Angle bc = GetAngle(lineB, lineC);
                Angle ac = GetAngle(lineA, lineC);

                //meets single chamfer conditions
                if (ab.Equals(bc))
                {
                    // measuring counterclockwise or clockwise
                    if ((ab.GetDegrees() < 180 && ac.GetDegrees() < 180 && ac.GetDegrees() > 0)
                        || (ab.GetDegrees() > 180 && ac.GetDegrees() > 180 && ac.GetDegrees() < 360))
                    {
                        ChamferList.Add(new ChamferGroup(EntityList.IndexOf(lineA), 
                            EntityList.IndexOf(lineB), EntityList.IndexOf(lineC), lineB));
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Processes and removes possible chamfers if they do not meet the criteria.
    /// If a chamfer has no chamfers parallel to it but does have lines parallel
    /// then it is not a chamfer and is removed from the chamfer list.
    /// </summary>
    /// <param name="lineList"> unordered list of lines </param>
    private void RemoveFalseChamfers(List<Line> lineList)
    {
        List<ChamferGroup> chamferGroupsToRemove = new();
        foreach (ChamferGroup chamferGroup in ChamferList)
        {
            bool isParallelToSomething = false;
            bool hasParallelChamfer = false;
                    
            // if parallel to a line... 
            foreach (Line line in lineList)
            {
                // ignore same line
                if (chamferGroup.ChamferIndex.Equals(line))
                {
                    continue;
                }
                    
                if (IsParallel(chamferGroup.Chamfer, line))
                {
                    isParallelToSomething = true;
                }
            }
            // and it is not a chamfer...
            if (isParallelToSomething)
            {
                foreach (ChamferGroup compChamferGroup in ChamferList)
                {
                    if (chamferGroup != compChamferGroup && IsParallel(chamferGroup.Chamfer, compChamferGroup.Chamfer))
                    {
                        hasParallelChamfer = true;
                    }
                }
            }
            // it is not a chamfer
            if (isParallelToSomething && !hasParallelChamfer)
            {
                chamferGroupsToRemove.Add(chamferGroup);
                break;
            }
        }
        foreach (ChamferGroup cgToRemove in chamferGroupsToRemove)
        {
            ChamferList.Remove(cgToRemove);
        }
        // remaining possible chamfers meet above case so confirm
        //foreach (Line line in possibleChamferList)
        foreach (ChamferGroup chamferGroup in ChamferList)
        {
            chamferGroup.Confirmed = true;
        }
    }

    private void FlagGroup3()
    {
        if (FeatureType is not (PossibleFeatureTypes.Group1A1 or PossibleFeatureTypes.Group1A2)) return;
        
        // copy of base entity list with just lines
        List<Line> lineList = GetLinesFromEntityList(EntityList).ToList();
        SetPossibleChamfers(GetOrderedLines(lineList));
        
        if (lineList.Count < 3) return;
        switch (ChamferList.Count)
        {
            case <= 0:
                return;
            // check potential chamfers
            // if only one chamfer, it is confirmed to be a chamfer
            case 1:
                ChamferList[0].Confirmed = true;
                break;
            // if 2 to 3 chamfers, only confirm if a parallel line to it
            // is also possible/confirmed chamfers
            case >= 2 and <= 3:
            {
                RemoveFalseChamfers(lineList);
                break;
            }
        }

        // if more than 4 chamfers we run into the octagon problem
        // so we cannot confirm what lines are chamfers
        // TODO: implement better check for octagon problem, perhaps with frontend
    }

    #endregion

    #region Group4
    /*
     * Checks the feature it is being called on to see if it is a group 4 feature.
     *
     * @return True if it is Group 4, false if not
     */
    public void CheckGroup4()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature.numLines != 2 || (feature.numArcs != 2 && feature.numArcs != 0)))
            {
                foreach (Entity entity in feature.EntityList)
                {
                    if (entity is Line tempLine && tempLine.DoesIntersect(entity))
                    {
                        feature.FeatureType = PossibleFeatureTypes.Group4;
                    }
                }
            }
        }
    }

    #endregion
    
    #region Group9

    internal void CheckGroup9()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            for (int i = 0; i < feature.EntityList.Count; i++)
            {
                for (int j = 0; j < feature.EntityList[i].AdjList.Count; j++)
                {
                    if (feature.EntityList[i].AdjList[j].KissCut)
                    {
                        feature.KissCut = true;
                        feature.FeatureType = PossibleFeatureTypes.Group9;

                    }
                }
            }
        }
    }

    #endregion

    #region Group5
    /// <summary>
    /// Checks the feature it is being called on to see if it is a group 5 feature.
    /// </summary>
    internal void CheckGroup5()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature is not { numLines: 2, numArcs: 1 } && feature is not { numLines: 3, numArcs: 0 or 2 }) && !feature.KissCut)
            {
                bool con = true;
                foreach (Entity entity in feature.EntityList)
                {
                    if (entity is Arc arc && (arc.CentralAngle != 90 && arc.CentralAngle != 180))
                    {
                        con = false;
                    }
                }

                if (con && HasTwoParalellLine(feature.EntityList))
                {
                    feature.FeatureType = PossibleFeatureTypes.Group5;
                }
            }
        }
    }

    #endregion

    #region Group6
    /// <summary>
    /// Checks the feature it is being called on to see if it contains a group 6 feature
    /// </summary>
    internal void CheckGroup6Perimeter()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature.numLines < 2 || feature.numCircles != 0 || feature.numArcs < 3 || feature.numArcs > 4))
            {
                bool con = true;
                foreach (Entity entity in feature.EntityList)
                {
                    if (entity is Arc && ((entity as Arc).CentralAngle != 90 && (entity as Arc).CentralAngle != 180))
                    {
                        con = false;
                    }
                }

                if (con && HasTwoParalellLine(feature.EntityList))
                {
                    feature.FeatureType = PossibleFeatureTypes.Group6;
                }
            }
        }
    }

    #endregion
    
    #region Group17

    /*
     * Checks the feature it is being called on to see if it is a group 17 feature.
     *
     * @return True if it is Group 17, false if not
     */
    internal void CheckGroup17()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature.numLines != 2 || feature.numCircles != 0 || feature.numArcs != 1))
            {
                bool con = true;
                foreach (Entity entity in feature.EntityList)
                {
                    if (entity is Arc arc && WithinTolerance(arc.CentralAngle, 180))
                    {
                        con = false;
                    }
                }

                if (con)
                {
                    feature.FeatureType = PossibleFeatureTypes.Group17;
                }
            }
        }
    }

    #endregion

    #endregion
    
    #endregion

    #region OverrideFunctions

    /*
     * Overriding the Equals method to compare two Feature objects
     *
     * @Param obj is the object being compared to this
     * @Return true if the objects are equal
     */
    public override bool Equals(object obj)
    {
        if (!(obj is Feature) || (obj == null))
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
        if (((Feature)obj).numLines == this.numLines
            && ((Feature)obj).numCircles == this.numCircles
            && ((Feature)obj).numArcs == this.numArcs
            && Math.Abs(((Feature)obj).perimeter - this.perimeter) < Entity.EntityTolerance)
        {
            //Genuinly my first time ever using lambda expression for something actually useful
            //sort both lists by length
            EntityList.Sort((x, y) => x.Length.CompareTo(y.Length));
            ((Feature)obj).EntityList.Sort((x, y) => x.Length.CompareTo(y.Length));

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

    #endregion

    #region ExtendEntities

    /*
     *  Recursive function that calls extendAllEntitiesHelper. Sets extendedEntityList to EntityList.
     *  This is the main function that should be called to extend entities
     */
    public void ExtendAllEntities()
    {
        ExtendedEntityList = new List<Entity>(EntityList);
        ExtendAllEntitiesHelper();
    }

    /*
     * This is a recursive helper function to extend every line in ExtendedEntityList. It will loop through ExtendedEntityList,
     * previously set to EntityList, until it can find a Line to extend. Will recurss if extended. Base case is no more lines to extend
     * Should be N^N runtime seeing as the nested for loops is N^2, then it is called recursively with N-1 every time.
     * This makes it (((N!)^2) * N!) which is
     */
    private void ExtendAllEntitiesHelper()
    {
        bool extendedALine = false;
        int i = 0;
        do
        {
            if (ExtendedEntityList[i] is Line)
            {
                int j = 0;
                do
                {
                    if ((ExtendedEntityList[j] is Line) && ExtendedEntityList[i] != ExtendedEntityList[j])
                    {
                        // for each entity it checks if it can extend with every other entity and does so
                        // removes the two previous entities
                        // new extended lines are added in the extendTwoLines method
                        if (ExtendTwoLines((Line)ExtendedEntityList[i], (Line)ExtendedEntityList[j]))
                        {
                            extendedALine = true;
                        }
                    }

                    j++;
                } while (!extendedALine && j < ExtendedEntityList.Count);
            }

            i++;
        } while (!extendedALine && i < ExtendedEntityList.Count);

        if (extendedALine)
        {
            ExtendAllEntitiesHelper();
        }
    }

    /* Method that takes two lines and extends them to touch if they are:
     * 1. not already touching
     * 2. are parallel or perpendicular
     * adds extended line(parallel) or lines(perpendicular) to extendedEntityList
     * Perpendicular functionality has been commented out due to inconsistent slopes of lines,
     * which means a perpendicular angle of intersection is not guaranteed on features it should be
     *
     * @Param line1 is the first line being extended
     * @Param line2 is the second line being extended
     * @Return true if successfully extended. Could be false if the two lines don't have an intersect point,
     * aren't the same infinite line, or already touch
     */
    public bool ExtendTwoLines(Line line1, Line line2)
    {
        if (!line1.DoesIntersect(line2) && !line1.KissCut || !line2.KissCut)
            //makes sure you're not extending lines that already touch
            // Makes sure KissCut lines are not extended

        {
            if (line1.isSameInfiniteLine(line2))
            {
                ExtendedLine tempLine = new ExtendedLine(line1, line2); // makes a new extended line object 
                ChangeAdjListForExtendedLine(tempLine, line1, line2);
                ExtendedEntityList.Remove(line1);
                ExtendedEntityList.Remove(line2);
                ExtendedEntityList.Add(tempLine);
                return true; // extended two parallel lines into 1
            }
        }

        return false;
    }

    private void ChangeAdjListForExtendedLine(ExtendedLine exLine, Line line1, Line line2)
    {
        // maeke the extended line's adjacency list
        exLine.AdjList = new List<Entity>(line1.AdjList);
        exLine.AdjList.AddRange(line2.AdjList);
        exLine.AdjList.Remove(line1);
        exLine.AdjList.Remove(line2);
        
        // replace line1 and line2 with exLine in the adjacency lists for entities touching line1 and line2
        foreach (Entity e in line1.AdjList)
        {
            e.AdjList.Remove(line1);
            e.AdjList.Add(exLine);
        }
        foreach (Entity e in line2.AdjList)
        {
            e.AdjList.Remove(line2);
            e.AdjList.Add(exLine);
        }
    }

    #endregion

    #region SeperateBaseEntities

    /*
     * Function that seperates the base entities, which will have been extended, if possible, at this point,
     * from ExtendedEntityList into baseEntityList. Most logic for seperation lies in seperateBaseEntitiesHelper
     *
     * @Return true if successfully seperates base entities
     */
    public bool SeperateBaseEntities()
    {
        if (ExtendedEntityList[0] is Circle or Ellipse) // case where the feature contains a circle or ellipse
        {
            if (ExtendedEntityList.Count == 1 && baseEntityList.Count == 0) // it should be the only entity in the list
            {
                baseEntityList.Add(ExtendedEntityList[0]); // adds the circle to the baseEntityList
                return true;
            }
        }

        // lists to pass to the helper function
        Stack<Entity> curPath = new Stack<Entity>();
        List<Entity> testedEntities = new List<Entity>();

        Entity head = ExtendedEntityList[0]; // default head is the first index of ExtendedEntityList
        foreach (Entity entity in ExtendedEntityList)
        // this finds the entity with the greatest length and makes it the head to hopefully reduce runtime
        {
            if (entity.Length > head.Length)
            {
                head = entity;
            }
        }

        curPath.Push(head); // pushes the head to the current Path
        if (SeperateBaseEntitiesHelper(curPath, testedEntities, head))
            // if it can find a Path
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
     * @Param Path is the current Path that has been taken
     * @Param testedEntities is a list of entities that have been visited
     * @Param head is the target entity that is trying to loop back through
     * @Return true if a Path has been found
     */
    private bool SeperateBaseEntitiesHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head)
    {
        if (curPath.Count > 2)
        {
            //base case where the current entity touches the head (means its a closed shape)
            //checks if contained in visitedEntities to avoid the second entity from triggering this
            //checks if current entity is the same as head to avoid a false true
            if (curPath.Peek() != head && curPath.Peek().EntityPointsAreTouching(head) &&
                !testedEntities.Contains(curPath.Peek()))
            {
                return true; //Path found
            }
        }

        testedEntities.Add(curPath.Peek()); //adds the current entitiy to the visitedEntities

        foreach (Entity entity in ExtendedEntityList)
        {
            if (entity != curPath.Peek())
            {
                // checks if entity in loop is not the curent entity being checked
                if (curPath.Peek().EntityPointsAreTouching(entity) && (!testedEntities.Contains(entity)))
                // checks that the entitiy has not already been tested and is touching the entity
                {
                    curPath.Push(entity); //adds to stack
                    if (SeperateBaseEntitiesHelper(curPath, testedEntities, head)) //recursive call with updated Path
                    {
                        return true;
                    }
                }
            }
        }
        //this point in the function means nothing is touching current entity

        if (curPath.Peek() == head)
            //if the function of the head reaches this point it means it has not found a Path back to the head
        {
            foreach (Entity entity in ExtendedEntityList)
            {
                if (entity is Ellipse)
                {
                    continue;
                }
                if (!testedEntities
                        .Contains(entity)) // finds the first entity that has not been tested and selects it as the head
                {
                    curPath.Clear(); //clears Path and adds the new head to it
                    curPath.Push(entity);
                    return SeperateBaseEntitiesHelper(curPath, testedEntities, entity);
                }
            }
        }

        curPath.Pop();
        return false; //nothing is touching this entity so it is popped off of Path
    }

    #endregion

    #region PerimeterFeatureFunctions

    /*
     * Function that uses finds the Path from the two parents of all extended lines and adds the Path as a group of
     * entities at new index in PerimeterFeatureList
     *
     * @Return true if a valid Path is found and seperated successfully
     */
    public void SeperatePerimeterEntities()
    {
        // lists to pass to the helper function
        List<Entity> path = new List<Entity>();

        foreach (Entity entity in baseEntityList) // removes all base entities from ExtendedEntityList
        {
            ExtendedEntityList.Remove(entity);
        }

        AddBackParents();
        // at this point ExtendedEntityList should only contain perimeter features

        List<Entity> unusedEntities = new List<Entity>(ExtendedEntityList);

        //seperatePerimeterEntitiesHelper(Path, unusedEntities, null);

        while (unusedEntities.Count > 0)
        {
            GetTouchingList(path, unusedEntities, null);
            if (path.Count > 0)
            {
                PerimeterFeatureList.Add(new Feature(new(path)));
            }

            path.Clear();
        }
    }

    /* Recursive function that adds all entities in unusedEntities that intersect curEntity into Path
     *
     * @Param Path is the list of touching entities
     * @Param unusedEntities are all available entities to add
     * @Param curEntity is the current entity being checked
     */
    public void GetTouchingList(List<Entity> path, List<Entity> unusedEntities, Entity curEntity)
    {
        if (curEntity is null)
        {
            curEntity = unusedEntities[0];
            path.Add(unusedEntities[0]);
            unusedEntities.RemoveAt(0);
        }

        List<Entity> touchingList = new List<Entity>();
        for (int i = 0;
             i < unusedEntities.Count;
             i++) // adds all entities in unusedEntities that touch curEntitty to Path and touchinglist and removes them from unusedEntities
        {
            if (curEntity.DoesIntersect(unusedEntities[i]))
            {
                touchingList.Add(unusedEntities[i]);
                path.Add(unusedEntities[i]);
                unusedEntities.Remove(unusedEntities[i]);
                i--; // i needs to stay the same since everything to the right of the moved entity is shifted left once
            }
        }

        foreach (Entity entity in touchingList)
        {
            GetTouchingList(path, unusedEntities, entity);
        }
    }

    // Adds back all parents of extended lines that are not in baseEntityList back into ExtendedEntityList
    private void AddBackParents()
    {
        for (int i = 0; i < ExtendedEntityList.Count; i++)
        {
            if (ExtendedEntityList[i] is ExtendedLine && (!baseEntityList.Contains(ExtendedEntityList[i])))
            {
                AddBackParentsHelper((ExtendedLine)ExtendedEntityList[i], ExtendedEntityList);
                i--;
            }
        }
    }

    private void AddBackParentsHelper(ExtendedLine exLine, List<Entity> targetList)
    {
        bool addedParent = false;
        if (exLine.Parent1 is ExtendedLine)
        {
            AddBackParentsHelper((ExtendedLine)exLine.Parent1, targetList);
        }
        else
        {
            targetList.Add(exLine.Parent1);
            addedParent = true;
        }

        if (exLine.Parent2 is ExtendedLine)
        {
            AddBackParentsHelper((ExtendedLine)exLine.Parent2, targetList);
        }
        else
        {
            targetList.Add(exLine.Parent2);
            addedParent = true;
        }

        if (addedParent)
        {
            foreach (Entity e in exLine.AdjList)
            {
                if (e.DoesIntersect(exLine.Parent1))
                {
                    e.AdjList.Add(exLine.Parent1);
                }
                else
                {
                    e.AdjList.Add(exLine.Parent2);
                }

                e.AdjList.Remove(exLine);
            }
        }
        
        targetList.Remove(exLine); // targetList will not have a parent that is an extended line in it
    }

    #endregion

    public Point FindMaxPoint()
    {
        double maxX = double.MinValue;
        double maxY = double.MinValue;
        foreach (Entity entity in EntityList)
        {
            maxX = Math.Max(maxX, entity.MaxX());
            maxY = Math.Max(maxY, entity.MaxY());
        }

        return new Point(maxX, maxY);
    }

    public Point FindMinPoint()
    {
        double minX = double.MaxValue;
        double minY = double.MaxValue;

        foreach (Entity entity in EntityList)
        {
            minX = Math.Min(minX, entity.MinX());
            minY = Math.Min(minY, entity.MinY());
        }

        return new Point(minX, minY);
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

        if (sumAngles > LOW_ANGLE_TOLERANCE && sumAngles < HIGH_ANGLE_TOLERANCE)
        {
            return true;
        }

        return false;
    }

    /*
     * Function that checks if the list passed in has at least one set of parallel lines
     *
     * @Param entities is the Entity list that is checked
     * @Return true if a set of parrallel lines is found
     */
    private static bool HasTwoParalellLine(List<Entity> entities)
    {
        for (int i = 0; i < entities.Count(); i++)
        {
            if (entities[i] is Line)
            {
                for (int j = 0; j < entities.Count(); j++)
                {
                    if (j == i || entities[j] is not Line)
                    {
                        continue;
                    }

                    Line entityI = (entities[i] as Line);
                    Line entityJ = (entities[j] as Line);

                    // Check for verticality
                    if ((Math.Abs(entityI.SlopeX) > Entity.EntityTolerance || Math.Abs(entityI.SlopeX) > 10000000) &&
                        (Math.Abs(entityJ.SlopeX) > Entity.EntityTolerance || Math.Abs(entityJ.SlopeX) > 10000000) ||
                        (Math.Abs(entityI.SlopeY) > Entity.EntityTolerance || Math.Abs(entityI.SlopeY) > 10000000) &&
                        (Math.Abs(entityJ.SlopeY) > Entity.EntityTolerance || Math.Abs(entityJ.SlopeY) > 10000000))
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

    /*
     * Function that calculates the perimeter of this feature by going through every entity in EntityList and adding the length.
     * This should only be called once, and probably by the constructor, but the perimeter = 0 is a safeguard in case this is
     * called more than once.
     */
    public void CalcPerimeter()
    {
        perimeter = 0;
        for (int i = 0; i < EntityList.Count; i++)
        {
            perimeter += EntityList[i].Length;
        }

        if (FeatureType == PossibleFeatureTypes.Group1B1 || FeatureType == PossibleFeatureTypes.Punch)
        {
            diameter = perimeter / Math.PI;
        }
    }

    /*
     * Function that checks the number of unique radius lengths in a feature.
     * multipleRadius is initially set to 1, incremented for each unique radius found.
     */
    public void CheckMultipleRadius()
    {
        // Only can be run on Group 1A2, 1C, and 2A2
        if (FeatureType == PossibleFeatureTypes.Group1A2 || FeatureType == PossibleFeatureTypes.Group1C ||
            FeatureType == PossibleFeatureTypes.Group2A2)
        {
            // TODO: Add logic to check for multiple radii on bowtie features
            if (FeatureType == PossibleFeatureTypes.Group2A2) return;

            // Create a new list containing only the arc entities
            List<Entity> arcList = new List<Entity>();
            foreach (Entity entity in baseEntityList)
            {
                if (entity is Arc)
                {
                    arcList.Add(entity);
                }
            }

            // Remove duplicate radii arcs
            for (int i = 0; i < arcList.Count; i++)
            {
                for (int j = i + 1; j < arcList.Count; j++)
                {
                    if (Math.Abs((arcList[i] as Arc).Radius - (arcList[j] as Arc).Radius) < Entity.EntityTolerance)
                    {
                        arcList.Remove(arcList[j]);
                        j--;
                    }
                }
            }

            // Count the number of unique radii left in the list
            for (int i = 0; i < arcList.Count; i++)
            {
                for (int j = i + 1; j < arcList.Count; j++)
                {
                    if (Math.Abs((arcList[i] as Arc).Radius - (arcList[j] as Arc).Radius) > Entity.EntityTolerance)
                    {
                        multipleRadius += 1;
                    }
                }
            }
        }
    }
}