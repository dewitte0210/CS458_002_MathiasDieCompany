/*
 * For borders look at every feature that is not inside another feature (that is a border)
 * calc number up using number of borders
 *
 * for optimization before detecting features ignore all entity groups outside
 * the first border and calculates feautrues only for that one
 */

using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class Feature
{
    [JsonProperty] public PossibleFeatureTypes FeatureType { get; set; }

    // A list of all the perimeter features attached to this features.
    [JsonProperty] public List<PerimeterFeatureTypes> PerimeterFeatures { get; set; }

    [JsonProperty] public List<Entity> EntityList { get; set; } //list of touching entities that make up the feature
    [JsonProperty] public bool kissCut;
    [JsonProperty] public int multipleRadius;
    [JsonProperty] public bool roundedCorner;
    [JsonProperty] public double perimeter;
    [JsonProperty] public double diameter;
    [JsonProperty] public int count;

    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]

    internal List<Entity> ExtendedEntityList { get; set; } // list of entities after extending them all

    // what the list is sorted into from extendedEntityList which should only
    // contain entities that make up the base shape and possibly corner features
    internal List<Entity> baseEntityList;

    internal List<List<Entity>> PerimeterEntityList; // 2 dimensional list where each list at each index is a group of
    // touching entities that make up a single perimeter feature for
    // the original feature
    //EXAMPLE: <[list for Mitiered notch], [list for raduis notch], [list for Group17], [list for chamfered corner]>
    // You will have to run detection for perimeter features for each index of this list

    public int getNumLines() { return numLines; }
    public int getNumArcs() { return numArcs; }
    public int getNumCircles() { return numCircles; }
    public int getNumEllipses() { return numEllipses; }

    private int numEllipses = 0;
    private int numLines = 0;
    private int numArcs = 0;
    private int numCircles = 0;
    private const double LOW_ANGLE_TOLERANCE = 359.9;
    private const double HIGH_ANGLE_TOLERANCE = 360.09; 
    
    #region Constructors

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
    public Feature(List<Entity> entityList, bool kissCut, int multipleRadius)
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
        this.multipleRadius = 1;
        this.EntityList = EntityList;
        this.baseEntityList = EntityList;
        this.PerimeterFeatures = new List<PerimeterFeatureTypes>();
        ExtendedEntityList = new List<Entity>();
        PerimeterEntityList = new List<List<Entity>>();

        CountEntities(baseEntityList, out numLines, out numArcs, out numCircles, out numEllipses);

        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    #endregion

    #region FeatureDetection

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
        //check two conditions possible to make Group1B (with no perimeter features)
        if (CheckGroup1B(numCircles, numLines, numArcs, out PossibleFeatureTypes type))
        {
            FeatureType = type;
        }
        else if (CheckGroup1C(out type))
        {
            FeatureType = type;
        }
        else if (CheckGroup6Base())
        {
            FeatureType = PossibleFeatureTypes.Group6;
        }
        //check two conditions possible to make Group1A (with no perimeter features)
        else if (numLines >= 4)
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
        else if (CheckGroup4())
        {
            FeatureType = PossibleFeatureTypes.Group4;
        }
        else if (CheckGroup5())
        {
            FeatureType = PossibleFeatureTypes.Group5;
        }
        else if (CheckGroup6())
        {
            FeatureType = PossibleFeatureTypes.Group6;
        }
        else if (CheckGroup17())
        {
            FeatureType = PossibleFeatureTypes.Group17;
        }
        else
        {
            //Console.WriteLine("Error: Cannot assign feature type.");
        }

        //calculate and set the perimeter of the feature
        calcPerimeter();

        //check if the feature has multiple radii
        checkMultipleRadius();
    }


    #region Group1B

    /*
     * Checks the baseEntityList to see if this feature is one of the Group 1B features
     *
     * @Param numCircles, numLines, numArcs is the number of the respective entities in the EntityList
     * @Param type is used as a return value with the out keyword
     * @Return true if the type was detected
     */
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

    #endregion

    #region Group1C

    public bool CheckGroup1C(out PossibleFeatureTypes type)
    {
        //Use local variables for lines cirlces arcs and elipses
        int lines, circles, arcs, elipses;

        //gives the count of lines arcs circles and elipses
        CountEntities(baseEntityList, out lines, out arcs, out circles, out elipses);

        //Triange base shape needs 3 lines
        if (lines != 3)
        {
            type = PossibleFeatureTypes.Unknown;
            return false;
        }

        //If there are 3 lines and zero arcs then it should be a triangle
        else if (arcs == 0)
        {
            type = PossibleFeatureTypes.Group1C;
            this.FeatureType = PossibleFeatureTypes.Group1C;
            return true;
        }
        else if (arcs > 3)
        {
            type = PossibleFeatureTypes.Unknown;
            return false;
        }
        //At this point arcs is between 1-3 and lines = 3
        else
        {
            switch (arcs)
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
                            if (((Arc)baseEntityList[arcIndex]).IntersectLineWithArc((Line)baseEntityList[i],
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
                        if (!touchingArc[0].IntersectLineWithLine((Line)touchingArc[0], (Line)touchingArc[1]))
                        {
                            type = PossibleFeatureTypes.Group1C;
                            this.FeatureType = PossibleFeatureTypes.Group1C;
                            return true;
                        }
                    }

                    type = PossibleFeatureTypes.Unknown;
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
                            if (((Arc)baseEntityList[arcIndex]).IntersectLineWithArc((Line)baseEntityList[i],
                                    (Arc)baseEntityList[arcIndex]))
                            {
                                touchingArc[eIndex] = (Line)baseEntityList[i];
                                eIndex++;
                            }
                        }
                        else if (baseEntityList[i] is Arc && eIndex < 2)
                        {
                            if (((Arc)baseEntityList[arcIndex]).IntersectArcWithArc((Arc)baseEntityList[i],
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
                        type = PossibleFeatureTypes.Group1C;
                        this.FeatureType = PossibleFeatureTypes.Group1C;
                        return true;
                    }

                    type = PossibleFeatureTypes.Unknown;
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
                            if (arcList[arcIndex].IntersectLineWithArc((Line)baseEntityList[i], (Arc)arcList[0])
                                || arcList[arcIndex].IntersectLineWithArc((Line)baseEntityList[i], (Arc)arcList[1]))
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
                                && (arcList[0].IntersectArcWithArc((Arc)baseEntityList[i], (Arc)arcList[0])
                                    || arcList[1].IntersectArcWithArc((Arc)baseEntityList[i], (Arc)arcList[1])))
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
                            type = PossibleFeatureTypes.Unknown;
                            return false;
                        }
                    }

                    this.FeatureType = PossibleFeatureTypes.Group1C;
                    type = PossibleFeatureTypes.Group1C;
                    return true;
                }
                default:
                    type = PossibleFeatureTypes.Unknown;
                    return false;
            }
        }

        //If somehow there is no decision made by this point then there is an error
    }

    #endregion

    #region Group2A

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
                        type = PossibleFeatureTypes.Group2A1;
                        return true;
                    }
                }
            }
            //Possible bowtie
            else if (numLines == 2)
            {
                if (IsBowtie() && getBothLinesAndDetermineParallelization())
                {
                    type = PossibleFeatureTypes.Group2A2;
                    return true;
                }
            }
        }
        //Ellipse entity check
        else if (numLines == 0 && numArcs == 0 && numCircles == 0 && numEllipses == 1)
        {
            if ((baseEntityList[0] as Ellipse).IsFullEllipse)
            {
                type = PossibleFeatureTypes.Group2A1;
                return true;
            }
        }

        type = PossibleFeatureTypes.Punch;
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
                if (DetermineConcavity(baseEntityList[i], i))
                {
                    //  If previous curve was convex, there is a switch in concavity
                    if (tempConvexCount > 0)
                    {
                        convexCount++;
                        tempConvexCount = 0;
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
    private bool DetermineConcavity(Entity entity, int index)
    {
        if (!(entity is Arc || entity is Ellipse)) { return false; }
        //  Variables used to extend the line that is used for concavity detection to ensure it passes through
        //  the entire shape
        Point minPoint = FindMinPoint();
        Point maxPoint = FindMaxPoint();
        //double maxLength = Math.Sqrt(Math.Pow(maxPoint.X - minPoint.X, 2) + Math.Pow(maxPoint.Y - minPoint.Y, 2));
        double maxLength = 100; //TODO DELETE AND UNCOMMENT LINE ABOVE!!!!!!!!!
        Line ray;

        int numIntersections = 0;
        int numEndPointIntersections = 0;
        //  Finds the middle angle of the curve and calculates the ray
        if (entity is Arc arc) 
        { ray = arc.VectorFromCenter(arc.degreesToRadians(arc.AngleInMiddle())); }
        else
        { ray = (entity as Ellipse).vectorFromCenter(((entity as Ellipse).EndParameter - (entity as Ellipse).StartParameter) / 2); }

        //  Entends the ray
        Point unitVector = new Point((ray.EndPoint.X - ray.StartPoint.X) / ray.Length, (ray.EndPoint.Y - ray.StartPoint.Y) / ray.Length);
        Point newEndPoint = new Point(ray.StartPoint.X + maxLength * unitVector.X, ray.StartPoint.Y + maxLength * unitVector.Y);
        ray = new Line(ray.StartPoint.X, ray.StartPoint.Y, newEndPoint.X, newEndPoint.Y);

        //  Runs through the base list and finds the num of intersections with the shape
        //  Checks for end point intersections because it will detect 2 end point intersections
        //  per actual intersection
        for (int i = 0; i < baseEntityList.Count; i++)
        {
            if (baseEntityList[i].DoesIntersect(ray))
            {
                numIntersections++;
                if (baseEntityList[i] is Line)
                {
                    Line currEntity = (baseEntityList[i] as Line);
                    if (ray.getIntersectPoint(ray, currEntity).Equals(currEntity.StartPoint) || ray.getIntersectPoint(ray, currEntity).Equals(currEntity.EndPoint))
                    {
                        numEndPointIntersections++;
                    }
                }
                else if (baseEntityList[i] is Arc)
                {
                    Arc currEntity = (baseEntityList[i] as Arc);
                    if (ray.getIntersectPoint(ray, currEntity).Equals(currEntity.Start) || ray.getIntersectPoint(ray, currEntity).Equals(currEntity.End))
                    {
                        numEndPointIntersections++;
                    }
                }
                else if(baseEntityList[i] is Ellipse)
                {
                    Ellipse currEntity = (baseEntityList[i] as Ellipse);
                    double major = Math.Sqrt(Math.Pow((baseEntityList[i] as Ellipse).MajorAxisEndPoint.X - (baseEntityList[i] as Ellipse).Center.X, 2) + Math.Pow((baseEntityList[i] as Ellipse).MajorAxisEndPoint.Y - (baseEntityList[i] as Ellipse).Center.Y, 2));
                    if (ray.getIntersectPoint(ray, currEntity).Equals(currEntity.PointOnEllipseGivenAngleInRadians(major, major * currEntity.MinorToMajorAxisRatio, currEntity.StartParameter)) || ray.getIntersectPoint(ray, currEntity).Equals(currEntity.PointOnEllipseGivenAngleInRadians(major, major * currEntity.MinorToMajorAxisRatio, currEntity.EndParameter)))
                    {
                        numEndPointIntersections++;
                    }
                }
                else { continue; }
            }
        }
        //  Even num of intersections = concave
        return (numIntersections - (numEndPointIntersections / 2)) % 2 == 0;
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

    #region Group6base

    /*
     * Checks the feature it is being called on to see if it is a group 6 base feature (trapeziod with radius corners).
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


    #region Group3

    //Chamfered Corners 
    public bool CheckGroup3()
    {
        int numPossibleChamfers = 0;
        int numConfirmedChamfers = 0;

        List<Line> possibleChamferList;
        List<Line> confirmerdChamferList;

        /*   chamfer detection
			given 3 lines...
				if the two matching angles are equal and less than 90
				and the angle between edge 1 and 3 are less than 180
				it is a possible chamfer

			if a shape has multiple possible chamfers
			2-3 chamfers
				if an edge is a possible chamfer, if there are any lines parallel to it,
				those should also be a possible chamfer
			4 corners
				have the user select a base chamfer
				or just use the count as it is and visualize from 4 shortest
	   */

        

        //if (this.FeatureType == PossibleFeatureTypes.Group1A1)
        //{
        //}

        return false;
    }

    #endregion

    #region Group4

    /*
     * Checks the feature it is being called on to see if it is a group 4 feature.
     *
     * @return True if it is Group 4, false if not
     */
    public bool CheckGroup4()
    {
        if (numLines != 2 || (numArcs != 2 && numArcs != 0))
        {
            return false;
        }

        Line tempLine = null;
        foreach (Entity entity in EntityList)
        {
            if (entity is Line && tempLine == null)
            {
                tempLine = (entity as Line);
            }
            else if (entity is Line)
            {
                if (tempLine.DoesIntersect(entity))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Group5

    /*
     * Checks the feature it is being called on to see if it is a group 5 feature.
     *
     * @return True if it is Group 5, false if not
     */
    internal bool CheckGroup5()
    {
        if (numLines < 2 || numLines > 3 || numCircles != 0 || numArcs > 2)
        {
            return false;
        }

        foreach (Entity entity in EntityList)
        {
            if (entity is Arc && ((entity as Arc).CentralAngle != 90 && (entity as Arc).CentralAngle != 180))
            {
                return false;
            }
        }

        if (HasTwoParalellLine(EntityList))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Group6

    /*
     * Checks the feature it is being called on to see if it is a group 6 feature.
     *
     * @return True if it is Group 6, false if not
     */
    internal bool CheckGroup6()
    {
        if (numLines < 2 || numCircles != 0 || numArcs < 2 || numArcs > 4)
        {
            return false;
        }

        foreach (Entity entity in EntityList)
        {
            if (entity is Arc && ((entity as Arc).CentralAngle != 90 && (entity as Arc).CentralAngle != 180))
            {
                return false;
            }
        }

        if (HasTwoParalellLine(EntityList))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Group17

    /*
     * Checks the feature it is being called on to see if it is a group 17 feature.
     *
     * @return True if it is Group 17, false if not
     */
    internal bool CheckGroup17()
    {
        if (numLines != 2 || numCircles != 0 || numArcs != 1)
        {
            return false;
        }

        foreach (Entity entity in EntityList)
        {
            if (entity is Arc && ((entity as Arc).CentralAngle <= 180))
            {
                return false;
            }
        }

        return true;
    }

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
     * This is a recursive helper function to extend every line in ExtendedEntityList. It will loop through ExtendedEntityList,
     * previously set to EntityList, until it can find a Line to extend. Will recurss if extended. Base case is no more lines to extend
     * Should be N^N runtime seeing as the nested for loops is N^2, then it is called recursively with N-1 every time.
     * This makes it (((N!)^2) * N!) which is
     */
    private void extendAllEntitiesHelper()
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
                        if (extendTwoLines((Line)ExtendedEntityList[i], (Line)ExtendedEntityList[j]))
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
            if (line1.isSameInfinateLine(line2))
            {
                ExtendedLine tempLine = new ExtendedLine(line1, line2); // makes a new extended line object     
                ExtendedEntityList.Remove(line1);
                ExtendedEntityList.Remove(line2);
                ExtendedEntityList.Add(tempLine);
                return true; // extended two parallel lines into 1
            }
        }

        return false;
    }

    #endregion

    #region SeperateBaseEntities

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
            else
            {
                return false;
            } // means that it contains a circle but is not the only entity
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
     * @Param path is the current path that has been taken
     * @Param testedEnttiies is a list of enties that have been visited
     * @Param head is the target entity that is trying to loop back through
     * @Return true if a path has been found
     */
    private bool seperateBaseEntitiesHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head)
    {
        if (curPath.Count > 2)
        {
            //base case where the current entity touches the head (means its a closed shape)
            //checks if contained in visitedEntities to avoid the second entity from triggering this
            //checks if current entity is the same as head to avoid a false true
            if (curPath.Peek() != head && curPath.Peek().EntityPointsAreTouching(head) &&
                !testedEntities.Contains(curPath.Peek()))
            {
                return true; //path found
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
                    if (seperateBaseEntitiesHelper(curPath, testedEntities, head)) //recursive call with updated path
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
                if (!testedEntities
                        .Contains(entity)) // finds the first entity that has not been tested and selects it as the head
                {
                    curPath.Clear(); //clears path and adds the new head to it
                    curPath.Push(entity);
                    return seperateBaseEntitiesHelper(curPath, testedEntities, entity);
                }
            }
        }

        curPath.Pop();
        return false; //nothing is touching this entity so it is popped off of path
    }

    #endregion

    #region PerimeterFeatureFunctions

    /*
     * function that finds a path from start to target in Entity list
     *
     * @Param start is the entity that this algorithm starts
     * @Param target is the entity that is trying to be reached
     * @Return the path from start to target. Null if no viable path
     */
    private List<Entity> findPathFromStartToTargetInEntityList(Entity start, Entity target)
    {
        if (EntityList.Contains(start) && EntityList.Contains(target))
        {
            Stack<Entity> path = new Stack<Entity>();
            path.Push(start);
            if (findPathFromStartToTargetInEntityListHelper(path, new List<Entity>(), start, target))
            {
                List<Entity> temp = path.ToList();
                temp.Reverse();
                return temp;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    /*
     * Helper function to find the path from head to traget. Contains the actual logic for this task
     *
     * @Param path is the current path taken
     * @Param visitedEntities contains all visited entities
     * @Param head is the starting node
     * @Param target is the entity trying to be reached
     * @Return true if path is found
     */
    private bool findPathFromStartToTargetInEntityListHelper(Stack<Entity> curPath, List<Entity> testedEntities,
        Entity head, Entity target)
    {
        if (curPath.Peek() == target)
        {
            return true;
        }

        testedEntities.Add(curPath.Peek()); //adds the current entitiy to the visitedEntities

        foreach (Entity entity in EntityList)
        {
            if (entity != curPath.Peek())
            {
                // checks if entity in loop is not the curent entity being checked
                if (curPath.Peek().EntityPointsAreTouching(entity) && (!testedEntities.Contains(entity)))
                    // checks that the entitiy has not already been tested and is touching the entity
                {
                    curPath.Push(entity); //adds to stack
                    if (seperateBaseEntitiesHelper(curPath, testedEntities, head)) //recursive call with updated path
                    {
                        return true;
                    }
                }
            }
        }
        //this point in the function means nothing is touching current entity

        if (curPath.Peek() == head)
        {
            return false;
        } // if the current entity is the head it means nothing is touching it and there is no path

        curPath.Pop(); // nothing is touching this entity so it is popped off of path
        return false; // returns false so the previous recursive call can check the next touching entity
    }

    /*
     * Function that uses finds the path from the two parents of all extended lines and adds the path as a group of
     * entities at new index in PerimeterEntityList
     *
     * @Return true if a valid path is found and seperated successfully
     */
    public void seperatePerimeterEntities()
    {
        // lists to pass to the helper function
        List<Entity> path = new List<Entity>();

        foreach (Entity entity in baseEntityList) // removes all base entities from ExtendedEntityList
        {
            ExtendedEntityList.Remove(entity);
        }

        addBackParents();
        // at this point ExtendedEntityList should only contain perimeter features

        List<Entity> unusedEntities = new List<Entity>(ExtendedEntityList);

        //seperatePerimeterEntitiesHelper(path, unusedEntities, null);

        while (unusedEntities.Count > 0)
        {
            getTouchingList(path, unusedEntities, null);
            if (path.Count > 0)
            {
                PerimeterEntityList.Add(new List<Entity>(path));
            }

            path.Clear();
        }
    }

    /*
        public void seperatePerimeterEntitiesHelper(List<Entity> path, List<Entity> unusedEntities, Entity curEntity)
        {
            if (unusedEntities.Count > 0) // base case: all entities have been used in a perimeter feature
            {
                if (curEntity is null) // means not at any entity currently
                {
                    if (path.Count > 0)
                    {
                        PerimeterEntityList.Add(new List<Entity>(path)); // adds all perimeter feature paths besides last one
                    }
                    path.Clear();
                    path.Add(unusedEntities[0]);
                    unusedEntities.RemoveAt(0);
                    seperatePerimeterEntitiesHelper(path, unusedEntities, path.Last());
                }
                else // means there is a current entity
                {
                    bool intersected;
                    do
                    {
                        intersected = false;
                        foreach (Entity entity in unusedEntities)
                        {
                            if (curEntity.DoesIntersect(entity)) // add every unused entity that intersects current entity
                            {
                                path.Add(entity);
                                unusedEntities.Remove(entity);
                                intersected = true;
                                seperatePerimeterEntitiesHelper(path, unusedEntities, path.Last()); // reruns with touching entity now being current entity

                                break;
                            }
                        }

                    } while (intersected);

                    if (path.Count > 0)
                    {
                        PerimeterEntityList.Add(new List<Entity>(path));
                    }
                    seperatePerimeterEntitiesHelper(path, unusedEntities, null); // all touching entities to current entitiy have been found, rerun with null current entity
                }
            }
        }
    */
    /* Recursive function that adds all entities in unusedEntities that intersect curEntity into path
     *
     * @Param path is the list of touching entities
     * @Param unusedEntities are all available entities to add
     * @Param curEntity is the current entity being checked
     */
    public void getTouchingList(List<Entity> path, List<Entity> unusedEntities, Entity curEntity)
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
             i++) // adds all entities in unusedEntities that touch curEntitty to path and touchinglist and removes them from unusedEntities
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
            getTouchingList(path, unusedEntities, entity);
        }
    }

    // Adds back all parents of all extended lines in targetList
    private void addBackParents(List<Entity> targetList)
    {
        foreach (Entity entity in targetList)
        {
            if (entity is ExtendedLine)
            {
                addBackParentsHelper((ExtendedLine)entity, targetList);
            }
        }
    }

    // Adds back all parents of extended lines that are not in baseEntityList back into ExtendedEntityList
    private void addBackParents()
    {
        for (int i = 0; i < ExtendedEntityList.Count; i++)
        {
            if (ExtendedEntityList[i] is ExtendedLine && (!baseEntityList.Contains(ExtendedEntityList[i])))
            {
                addBackParentsHelper((ExtendedLine)ExtendedEntityList[i], ExtendedEntityList);
                i--;
            }
        }
    }

    private void addBackParentsHelper(ExtendedLine exLine, List<Entity> targetList)
    {
        if (exLine.Parent1 is ExtendedLine)
        {
            addBackParentsHelper((ExtendedLine)exLine.Parent1, targetList);
        }
        else
        {
            targetList.Add(exLine.Parent1);
        }

        if (exLine.Parent2 is ExtendedLine)
        {
            addBackParentsHelper((ExtendedLine)exLine.Parent2, targetList);
        }
        else
        {
            targetList.Add(exLine.Parent2);
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
    private bool HasTwoParalellLine(List<Entity> entities)
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
    public void calcPerimeter()
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
    public void checkMultipleRadius()
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