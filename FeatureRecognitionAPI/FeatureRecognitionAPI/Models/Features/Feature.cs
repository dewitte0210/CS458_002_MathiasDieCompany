/*
 * For borders look at every feature that is not inside another feature (that is a border)
 * calc number up using number of borders
 *
 * for optimization before detecting features ignore all entity groups outside
 * the first border and calculates features only for that one
 */

using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static FeatureRecognitionAPI.Models.Utility.Angles;
using static FeatureRecognitionAPI.Models.Utility.Intersect;
using static FeatureRecognitionAPI.Models.Utility.MdcMath;

namespace FeatureRecognitionAPI.Models.Features;

public class Feature
{
    private PossibleFeatureTypes? _featureType;
    [JsonProperty]
    public PossibleFeatureTypes? FeatureType
    {
        get => _featureType;
        set
        {
            _featureType = value;
            bool isRecognized = FeatureType != PossibleFeatureTypes.Unknown;

            if (EntityList.Count != 0) EntityList.ForEach(e => e.IsRecognized = isRecognized);
            if (BaseEntityList.Count != 0) BaseEntityList.ForEach(e => e.IsRecognized = isRecognized);
            if (ExtendedEntityList.Count != 0) ExtendedEntityList.ForEach(e => e.IsRecognized = isRecognized);
            if (PerimeterFeatureList.Count != 0) PerimeterFeatureList.ForEach(eList => { eList.EntityList.ForEach(e => e.IsRecognized = isRecognized); });
        }
    }

    // list of touching entities that make up the feature
    // don't rename these unless you also change the front end 
    [JsonProperty] public List<Entity> EntityList { get; set; } 
    [JsonProperty] public bool KissCut { get; set; }
    [JsonProperty] public int multipleRadius { get; set; }
    [JsonProperty] public bool roundedCorner { get; set; }
    [JsonProperty] public double perimeter { get; set; }
    [JsonProperty] public double diameter { get; set; }
    [JsonProperty] public int count { get; set; }
    public List<ChamferGroup> ChamferList { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]

    // list of entities after extending them all
    internal List<Entity> ExtendedEntityList { get; set; } 

    // what the list is sorted into from extendedEntityList which should only
    // contain entities that make up the base shape and possibly corner features
    internal List<Entity> BaseEntityList;

    /// <summary>
    /// 2-dimensional list where each list at each index is a group of
    /// touching entities that make up a single perimeter feature for the original feature
    /// EXAMPLE: '[list for Mitered notch], [list for radius notch], [list for Group17], [list for chamfered corner]'
    /// You will have to run detection for perimeter features for each index of this list
    /// </summary>
    internal List<Feature> PerimeterFeatureList;

    public int GetNumLines() { return _numLines; }
    public int GetNumArcs() { return _numArcs; }
    public int GetNumCircles() { return _numCircles; }
    public int GetNumEllipses() { return _numEllipses; }

    private int _numEllipses;
    private int _numLines;
    private int _numArcs;
    private int _numCircles;
    
    #region Constructors

    /// <summary>
    /// Constructor that passes in an EntityList for the feature. Feature detection is expected to be
    /// called on a feature using this constructor. This was mostly used for testing when wanting to
    /// avoid feature detection in the constructor. Could probably be deleted at this point since
    /// feature detection was moved out of the constructors.
    /// </summary>
    /// <param name="entityList"> the EntityList being passed into the feature. could be a base feature,
    /// that includes perimeter features, or just the list for a perimeter feature </param>
    /// <param name="kissCut"> stores whether the feature is kiss cut </param>
    /// <param name="multipleRadius"> stores whether the feature has multiple radii for rounded corners </param>
    public Feature(List<Entity> entityList, bool kissCut, int multipleRadius)
    {
        EntityList = entityList;
        KissCut = kissCut;
        this.multipleRadius = multipleRadius;
        BaseEntityList = new List<Entity>();
        ExtendedEntityList = new List<Entity>();
        PerimeterFeatureList = new List<Feature>();
        ChamferList = new();

        CalcPerimeter();
    }

    /// <summary>
    /// Constructor that is expected to be used the most as it just passes in the EntityList for the
    /// feature and detection, along with all other fields will be calculated based off this list in
    /// a separate function
    /// </summary>
    /// <param name="entityList"> the list being passed into the feature which includes all base entities
    /// of the feature, including the perimeter features entities, unless the feature is just a
    /// perimeter one </param>
    public Feature(List<Entity> entityList)
    {
        EntityList = entityList;
        BaseEntityList = new();
        ExtendedEntityList = new List<Entity>();
        PerimeterFeatureList = new List<Feature>();
        ChamferList = new();
        ConstructFromEntityList();
    }

    /// <summary>
    /// create a feature when the feature type is known
    /// </summary>
    /// <param name="pft">PossibleFeatureType when known</param>
    /// <param name="entityList">Entity list that makes up the feature</param>
    public Feature(PossibleFeatureTypes pft, List<Entity> entityList)
    {
        EntityList = entityList;
        BaseEntityList = new List<Entity>();
        ExtendedEntityList = new List<Entity>();
        PerimeterFeatureList = new List<Feature>();
        ChamferList = new();

        ConstructFromEntityList();
        //have to call after everything because feature type expects entity lists
        FeatureType = pft;
    }

    #endregion

    // This got moved out so Initialization can be called after populating its EntityList
    public void ConstructFromEntityList()
    {
        count = 1;
        multipleRadius = 1;
        BaseEntityList = new();
        ExtendedEntityList = new List<Entity>();
        PerimeterFeatureList = new List<Feature>();

        CountEntities(EntityList, out _numLines, out _numArcs, out _numCircles, out _numEllipses);

        //calculate and set the perimeter of the feature
        CalcPerimeter();
    }
    
    #region FeatureDetection


    /// <summary>
    /// Counts the entities in the EntityList.
    /// </summary>
    /// <param name="entityList"> the list that is being looped through. Note that it is passed by reference
    /// and any changes to the list in this function will change the list in the scope of wherever this
    /// function was called from </param>
    /// <param name="numLines"> the counted number of lines. The out keyword means that the value is returned
    /// to the parameter passed when calling the function </param>
    /// <param name="numArcs"> the counted number of arcs. The out keyword means that the value is returned
    /// to the parameter passed when calling the function </param>
    /// <param name="numCircles"> the counted number of circles. The out keyword means that the value is returned
    /// to the parameter passed when calling the function. As far as I can tell there should only ever be one
    /// circle in a feature, and should be the only entity in the list</param>
    /// <param name="numEllipses"></param>
    public static void CountEntities(List<Entity> entityList, out int numLines, out int numArcs, out int numCircles,
        out int numEllipses)
    {
        numLines = 0;
        numArcs = 0;
        numCircles = 0;
        numEllipses = 0;

        //count the number of each entity type
        foreach (Entity entity in entityList)
        {
            if (entity is Line) numLines++;
            else if (entity is Arc) numArcs++;
            else if (entity is Circle) numCircles++;
            else if (entity is Ellipse) numEllipses++;
            else
            {
                Console.WriteLine("Error: Cannot detect entity type.");
                break;
            }
        }
    }

    /// <summary>
    /// Function that calls several other functions to determine this feature's type. Outside of testing this
    /// should be called on every feature, including seperated perimeter features
    /// </summary>
    public void DetectFeatures()
    {
        if (BaseEntityList.Count == 0)
        {
            BaseEntityList = new(EntityList);
            PerimeterFeatureList.Clear();
        } // should only happen if line extension and separation were skipped
        
        CountEntities(BaseEntityList, out _numLines, out _numArcs, out _numCircles, out _numEllipses); // recount entities in baseShape
        
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

        foreach (Feature feature in PerimeterFeatureList)
        {
            // if null set equal to
            feature.FeatureType ??= PossibleFeatureTypes.Unknown;
        }
            
        //calculate and set the perimeter of the feature
        CalcPerimeter();

        //check if the feature has multiple radii
        CheckMultipleRadius();
    }

    #region BaseDetection
    
    #region Group1A

    private bool CheckGroup1A()
    {
        if (_numLines >= 4)
        {
            if (_numArcs == 0)
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

    /// <summary>
    /// Checks the baseEntityList to see if this feature is one of the Group 1B features
    /// </summary>
    /// <returns></returns>
    private bool CheckGroup1B()
    {
        // Entity is just a circle
        if (_numCircles == 1 && _numLines == 0 && _numArcs == 0 && BaseEntityList[0] is Circle circle)
        {
            FeatureType = circle.Radius * 2 <= 1.75 ? PossibleFeatureTypes.Punch : PossibleFeatureTypes.Group1B1;
            return true;
        }
        //Entity contains the correct number of lines and arcs to be a rounded rectangle add up the degree measures
        //of the arcs and make sure they are 360
        if (_numArcs == 2 && _numLines == 2 && IsSubshapeRectangle() && DoAnglesAddTo360())
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
                if (BaseEntityList[index] is Arc foundArc)
                {
                    if (!gotArc1)
                    {
                        arc1 = foundArc;
                        gotArc1 = true;
                    }
                    else
                    {
                        arc2 = foundArc;
                        gotArc2 = true;
                    }
                }
                else if (BaseEntityList[index] is Line foundLine)
                {
                    line = foundLine;
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

        // set to Unknown and return false.
        FeatureType = PossibleFeatureTypes.Unknown;
        return false;
    }

    #endregion

    #region Group1C

    public bool CheckGroup1C()
    {
        //Triangle base shape needs 3 lines
        if (_numLines != 3 || _numArcs > 3) return false;

        // TODO: This is wrong
        //If there are 3 lines and zero arcs then it should be a triangle
        if (_numArcs == 0)
        {
            FeatureType = PossibleFeatureTypes.Group1C;
            return true;
        }

        //At this point arcs is between 1-3 and lines = 3
        switch (_numArcs)
        {
            case 1:
            {
                //Find the arc
                int arcIndex = 0;
                for (int i = 0; i < BaseEntityList.Count; i++)
                {
                    if (BaseEntityList[i] is Arc)
                    {
                        arcIndex = i;
                        break;
                    }
                }

                //Array of 2 entities to contain lines touching the arc.
                Entity[] touchingArc = new Entity[2];
                int eIndex = 0;
                //Find the two lines
                foreach (Entity baseEntity in BaseEntityList)
                {
                    if (baseEntity is Line line && eIndex < 2)
                    {
                        if (DoesIntersect(line, (Arc)BaseEntityList[arcIndex]))
                        {
                            touchingArc[eIndex] = line;
                            eIndex++;
                        }
                    }
                    if (eIndex == 2) break;
                }

                if (touchingArc[0] is Line && touchingArc[1] is Line)
                {
                    if (!DoesIntersect((Line)touchingArc[0], (Line)touchingArc[1]))
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
                for (int i = 0; i < BaseEntityList.Count; i++)
                {
                    if (BaseEntityList[i] is Arc)
                    {
                        arcIndex = i;
                        break;
                    }
                }

                Entity[] touchingArc = new Entity[2];
                int eIndex = 0;
                //Find the two lines
                foreach (Entity baseEntity in BaseEntityList)
                {
                    if (baseEntity is Line line && eIndex < 2)
                    {
                        if (DoesIntersect(line, (Arc)BaseEntityList[arcIndex]))
                        {
                            touchingArc[eIndex] = line;
                            eIndex++;
                        }
                    }
                    else if (baseEntity is Arc arc && eIndex < 2)
                    {
                        if (DoesIntersect(arc, (Arc)BaseEntityList[arcIndex]))
                        {
                            touchingArc[eIndex] = arc;
                            eIndex++;
                        }
                    }

                    if (eIndex == 2) break;
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
                foreach (Entity baseEntity in BaseEntityList)
                {
                    if (baseEntity is Arc arc)
                    {
                        arcList[arcIndex] = arc;
                        arcIndex++;
                    }
                    if (arcIndex == 2) break;
                }

                //ArcList has 2 arcs, check entities touching both (at least one line should be touching both arcs, so only 3 entities should be added )
                Entity[] touchingArc = new Entity[3];
                int eIndex = 0;
                //Find the two lines
                foreach (Entity baseEntity in BaseEntityList)
                {
                    if (baseEntity is Line line && eIndex < 4)
                    {
                        if (DoesIntersect(line, (Arc)arcList[0]) || DoesIntersect(line, (Arc)arcList[1]))
                        {
                            touchingArc[eIndex] = line;
                            eIndex++;
                        }
                    }
                    else if (baseEntity is Arc arc && eIndex < 4)
                    {
                        //If not equal to arc at 0 and 1 and if the arc intersects with the arc at 0 or at 1
                        if (!arcList[0].Equals(arc) && !arcList[1].Equals(arc) 
                                && (DoesIntersect(arc, (Arc)arcList[0]) || DoesIntersect(arc, (Arc)arcList[1])))
                        {
                            touchingArc[eIndex] = arc;
                            eIndex++;
                        }
                    }
                    if (eIndex == 3) break;
                }

                //Should have a list of 4 entities, if any of them are arcs, return false (arc is touching an arc)
                foreach (Entity entity in touchingArc)
                {
                    if (entity is Arc) return false;
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

    /// <summary>
    /// Checks if a feature is Group 2A (elliptical features).
    /// </summary>
    /// <returns> Returns the possible feature type. </returns>
    private bool CheckGroup2A()
    {
        if ((_numArcs >= 2 || _numEllipses >= 2) && _numCircles == 0)
        {
            //Possible ellipse with arcs
            if (_numArcs > 2 && _numLines == 0 && _numEllipses == 0)
            {
                if (DoAnglesAddTo360() && IsEllipse())
                {
                    FeatureType = PossibleFeatureTypes.Group2A1;
                    return true;
                }
            }
            //Possible bowtie
            else if (_numLines == 2 && IsBowtie() && IsSubshapeRectangle())
            {
                FeatureType = PossibleFeatureTypes.Group2A2;
                return true;
            }
        }
        //Ellipse entity check
        else if (_numLines == 0 && _numArcs == 0 && _numCircles == 0 && _numEllipses == 1)
        {
            if (BaseEntityList[0] is Ellipse { IsFullEllipse: true })
            {
                FeatureType = PossibleFeatureTypes.Group2A1;
                return true;
            }
        }

        FeatureType = PossibleFeatureTypes.Punch;
        return false;
    }

    /// <summary>
    /// Checks if a list of arcs forms an ellipse
    /// </summary>
    /// <returns> true if it is an ellipse </returns>
    private bool IsEllipse()
    {
        //Ensures the program will not crash if called in other circumstances
        if (_numCircles != 0 || _numLines != 0 || _numArcs == 0) return false;

        //List that will be sorted in order of touching arcs

        List<Arc>? connectedInOrder = null;
        foreach (Entity entity in BaseEntityList)
        {
            if (entity is Arc firstArc)
            {
                connectedInOrder = [firstArc];
                break;
            }
        }
        if (connectedInOrder == null) return false;
        
        while (connectedInOrder.Count != BaseEntityList.Count)
        {
            // ^1 means index count - 1
            //Prevents from running infinitely in certain circumstances
            if (!SortEllipseListHelper(connectedInOrder, connectedInOrder[^1])) return false;
        }

        //If end points do not connect, it is not an ellipse
        if (!connectedInOrder[0].Start.Equals(connectedInOrder[^1].End)) return false;
        
        return true;
    }

    /// <summary>
    /// Detects the next touching arc in a base entity list of arcs.
    /// Ensures the while loop in IsEllipse does not run infinitely if feature is not a closed loop
    /// </summary>
    private bool SortEllipseListHelper(List<Arc> connectedInOrder, Arc arc1)
    {
        foreach (Entity entity in BaseEntityList)
        {
            if (entity is Arc arc && arc1.End.Equals(arc.Start))
            {
                connectedInOrder.Add(arc);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Given a list of entities that could be a form of a bowtie, 
    /// this function ensures a bowtie is the feature
    /// </summary>
    private bool IsBowtie()
    {
        //  Counts the num of concave/convex pieces
        int concaveCount = 0;
        int convexCount = 0;
        //  Temp variables used to help determine when the curve switches concavity
        int tempConcaveCount = 0;
        int tempConvexCount = 0;
        for (int i = 0; i < BaseEntityList.Count; i++)
        {
            // If a line is hit, concavity must be recorded
            if (BaseEntityList[i] is Line)
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
                if (IsConcave(BaseEntityList[i]))
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
                        if (!IsSmoothCurve(BaseEntityList[i - 1], BaseEntityList[i])) return false;
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
                        if (!IsSmoothCurve(BaseEntityList[i - 1], BaseEntityList[i])) return false;
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

    /// <summary>
    /// Combs through the base entity list to determine if the entity is concave to the shape or not
    /// </summary>
    private bool IsConcave(Entity entity)
    {
        if (!(entity is Arc || entity is Ellipse)) { return false; }
        //  Variables used to extend the line that is used for concavity detection to ensure it passes through
        //  the entire shape
        Point minPoint = FindMinPoint();
        Point maxPoint = FindMaxPoint();
        double maxLength = 2 * Point.Distance(maxPoint, minPoint);
        Line? ray = null;

        int numIntersections = 0;
        int numEndPointIntersections = 0;
        //  Finds the middle angle of the curve and calculates the ray
        if (entity is Arc arc)
        {
            ray = arc.VectorFromCenter(DegToRadians(arc.AngleInMiddle()));
        }
        else
        {
            if (entity is Ellipse tempEntity)
            {
                double startAngle = tempEntity.StartParameter + Math.Atan2(tempEntity.MajorAxisEndPoint.Y - tempEntity.Center.Y, tempEntity.MajorAxisEndPoint.X - tempEntity.Center.X);
                double endAngle = tempEntity.EndParameter + Math.Atan2(tempEntity.MajorAxisEndPoint.Y - tempEntity.Center.Y, tempEntity.MajorAxisEndPoint.X - tempEntity.Center.X);
                ray = tempEntity.VectorFromCenter((endAngle - startAngle) / 2);
            }
        }
        // if ray couldn't get a value return false
        if (ray == null) return false;

        //  Extends the ray
        Point unitVector = new((ray.End.X - ray.Start.X) / ray.GetLength(), (ray.End.Y - ray.Start.Y) / ray.GetLength());
        Point newEndPoint = new(ray.Start.X + maxLength * unitVector.X, ray.Start.Y + maxLength * unitVector.Y);
        ray = new Line(ray.Start.X, ray.Start.Y, newEndPoint.X, newEndPoint.Y);

        //  Runs through the base list and finds the num of intersections with the shape
        //  Checks for end point intersections because it will detect 2 end point intersections
        //  per actual intersection
        foreach (Entity baseEntity in BaseEntityList)
        {
            if (!DoesIntersect(baseEntity, ray)) continue;

            Point? intersection = GetIntersectPoint(ray, baseEntity);
            if (intersection == null) continue;
            if (intersection.Equals(baseEntity.Start) || intersection.Equals(baseEntity.End)) numEndPointIntersections++;
        }

        //  Even num of intersections = concave
        return (numIntersections - (numEndPointIntersections / 2)) % 2 == 0;
    }

    private static bool IsSmoothCurve(Entity entity1, Entity entity2)
    {
        if (entity1 is not (Arc or Ellipse) || entity2 is not (Arc or Ellipse)) return true;

        if (entity1 is Arc ent1Arc)
        {
            if (entity2 is Arc ent2Arc)
            {
                return Math.Round(Math.Abs(ent1Arc.EndAngle - ent2Arc.StartAngle), 4) < 1.0;
            }
            if (entity2 is Ellipse ent2Ellipse)
            {
                return Math.Round(Math.Abs(ent1Arc.EndAngle * Math.PI / 180 - (ent2Ellipse.StartParameter + ent2Ellipse.Rotation)), 4) < 1.0;
            }
        }
        else if (entity1 is Ellipse ent1Ellipse)
        {
            double ent1EllipseCalc = ent1Ellipse.EndParameter + ent1Ellipse.Rotation;
            if (entity2 is Arc ent2Arc2)
            {
                return Math.Round(Math.Abs(ent1EllipseCalc - ent2Arc2.StartAngle * Math.PI / 180), 4) < 1.0;
            }
            if (entity2 is Ellipse ent2Ellipse2)
            {
                return Math.Round(Math.Abs(ent1EllipseCalc - (ent2Ellipse2.StartParameter + ent2Ellipse2.Rotation)), 4) < 1.0;
            }
        }

        // todo verify this should return true
        return true;
    }

    /// <summary>
    /// Retrieves 2 lines from the base entity list and determines if there is a
    /// rectangle that forms if the two lines are connected
    /// </summary>
    internal bool IsSubshapeRectangle()
    {
        Line baseLine1 = new Line(0, 0, 0, 0);
        Line baseLine2 = new Line(0, 0, 0, 0);
        bool gotLine1 = false;
        bool gotLine2 = false;
        int index = 0;
        //Retrieve 2 lines for check
        while (!gotLine1 || !gotLine2)
        {
            //For some reason there were not 2 lines -> return
            if (index == BaseEntityList.Count)
            {
                return false;
            }

            if (BaseEntityList[index] is Line foundLine)
            {
                if (!gotLine1)
                {
                    baseLine1 = foundLine;
                    gotLine1 = true;
                }
                else
                {
                    baseLine2 = foundLine;
                    gotLine2 = true;
                }
            }

            index++;
        }

        // Temp variables for correct line check since 4 lines can be formed from the baseLine endpoints
        Line tempLine1 = new(baseLine1.Start.X, baseLine1.Start.Y, baseLine2.Start.X, baseLine2.Start.Y);
        Line tempLine2 = new(baseLine1.End.X, baseLine1.End.Y, baseLine2.End.X, baseLine2.End.Y);
        Line tempLine3 = new(baseLine1.Start.X, baseLine1.Start.Y, baseLine2.End.X, baseLine2.End.Y);
        Line tempLine4 = new(baseLine1.End.X, baseLine1.End.Y, baseLine2.Start.X, baseLine2.Start.Y);
        // Variables for final quadrilateral lines
        Line newLine1;
        Line newLine2;
        // Checks the lengths of each line to ensure the right line is used to form the quadrilateral
        if (Math.Round(tempLine1.GetLength(), 4) + Math.Round(tempLine2.GetLength(), 4) < Math.Round(tempLine3.GetLength(), 4) + Math.Round(tempLine4.GetLength(), 4))
        {
            newLine1 = tempLine1;
            newLine2 = tempLine2;
        }
        else if (Math.Round(tempLine1.GetLength(), 4) + Math.Round(tempLine2.GetLength(), 4) > Math.Round(tempLine3.GetLength(), 4) + Math.Round(tempLine4.GetLength(), 4))
        {
            newLine1 = tempLine3;
            newLine2 = tempLine4;
        }
        else
        {
            newLine1 = Math.Round(tempLine1.GetLength(), 4) < Math.Round(tempLine2.GetLength(), 4) ? tempLine1 : tempLine2;
            newLine2 = Math.Round(tempLine3.GetLength(), 4) < Math.Round(tempLine4.GetLength(), 4) ? tempLine3 : tempLine4;
        }

        return Math.Round(baseLine1.GetLength(), 4).Equals(Math.Round(baseLine2.GetLength(), 4))
               && IsPerpendicular(newLine1, baseLine1) && IsPerpendicular(newLine1, baseLine2)
               && IsPerpendicular(newLine2, baseLine1) && IsPerpendicular(newLine2, baseLine2);
    }

    /// <summary>
    /// Checks to see if the point on the center most angle of the arc is concave to
    /// the line (within the bounds) or convex (extends past the bounds)
    /// </summary>
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
        if (DoesIntersect(line, cw) || DoesIntersect(line, ccw))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Group10

    /// <summary>
    /// Checks the feature to see if it is group 10.
    /// </summary>
    /// <returns> Returns the possible feature type. </returns>
    private bool CheckGroup10()
    {
        if (_numLines == 2 && _numArcs == 2)
        {
            // Fetch the lines and arcs
            List<Line> lines = new List<Line>();
            List<Arc> arcs = new List<Arc>();
            foreach (Entity entity in BaseEntityList)
            {
                if (entity is Line line) { lines.Add(line); }
                else if (entity is Arc arc) { arcs.Add(arc); }
            }
            // Find the bigger arc for correct end point orientation for math calculations
            Arc biggerArc = arcs[0].Radius > arcs[1].Radius ? arcs[0] : arcs[1];
            // Flip end points for calc if they are touching the smaller arc
            foreach (Line line in lines)
            {
                Point? intersect = GetIntersectPoint(line, biggerArc);
                if (intersect != null && !line.End.Equals(intersect))
                {
                    (line.Start, line.End) = (line.End, line.Start);
                }
            }

            // Runs only if the arcs start and end at the same angle
            if (arcs[0].StartAngle.Equals(arcs[1].StartAngle) && arcs[0].EndAngle.Equals(arcs[1].EndAngle))
            {
                // Start and end angles stored in variables for readability
                double startAngle = Math.Round(DegToRadians(arcs[0].StartAngle), 4);
                double endAngle = Math.Round(DegToRadians(arcs[0].EndAngle), 4);
                double line1Atan = Math.Round(Math.Atan2(lines[1].End.Y - lines[1].Start.Y, lines[1].End.X - lines[1].Start.X), 4);
                double line0Atan = Math.Round(Math.Atan2(lines[0].End.Y - lines[0].Start.Y, lines[0].End.X - lines[0].Start.X), 4);
                // Case 1: Both lines are vertical
                if (DEQ(lines[0].GetSlopeX(), 0) 
                    && DEQ(lines[1].GetSlopeX(), 0))
                {
                    if (DEQ(startAngle + endAngle, 2 * Math.PI, 4))
                    {
                        FeatureType = PossibleFeatureTypes.Group10;
                        return true;
                    }
                }
                // Case 2: Only one is vertical
                else if (DEQ(lines[0].GetSlopeX(), 0) 
                         || DEQ(lines[1].GetSlopeX(), 0))
                {
                    // Angle of line stored in variable for readability
                     
                    double lineAngle = DEQ(lines[0].GetSlopeX(), 0) ? line1Atan : line0Atan;
                    
                    //todo look at other doubleEquals replacements that use rounding
                    if (((DEQ(startAngle, Math.PI / 2, 4) || DEQ(endAngle, Math.PI / 2, 4))
                          || (DEQ(endAngle, 3 * Math.PI / 2, 4) || DEQ(startAngle, 3 * Math.PI / 2, 4)))
                        && (DEQ(lineAngle, startAngle) || DEQ(lineAngle, endAngle)))
                    {
                        FeatureType = PossibleFeatureTypes.Group10;
                        return true;
                    }
                }
                // Case 3: Lines are not vertical, can run Atan() function
                else
                {
                    double aTan0 = Math.Atan2(lines[0].End.Y - lines[0].Start.Y, lines[0].End.X - lines[0].Start.X);
                    double aTan1 = Math.Atan2(lines[1].End.Y - lines[1].Start.Y, lines[1].End.X - lines[1].Start.X);
                    
                    if ((DEQ(aTan0, startAngle) || DEQ(aTan0, endAngle)) && (DEQ(aTan1, startAngle) || DEQ(aTan1, endAngle)))
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

    /// <summary>
    /// Checks the feature to see if it is group 11.
    /// </summary>
    /// <returns> Returns the possible feature type. </returns>
    private bool CheckGroup11()
    {
        if (_numEllipses == 0 && _numCircles == 0)
        {
            // Case 1
            if (_numArcs == 2 && _numLines == 0)
            {
                // Keeps track of the bigger/smaller arc for the concavity check
                if (BaseEntityList[0] is Arc bigArc && BaseEntityList[1] is Arc smallArc)
                {
                    if (bigArc.Radius < smallArc.Radius)
                    {
                        (bigArc, smallArc) = (smallArc, bigArc);
                    }
                    if (bigArc.Start.Equals(smallArc.Start) && bigArc.End.Equals(smallArc.End) 
                        && IsConcave(bigArc) && !IsConcave(smallArc))
                    {
                        FeatureType = PossibleFeatureTypes.Group11;
                        return true;
                    }
                }
            }
            // Case 2
            else if (_numArcs == 1 && _numLines == 1)
            {
                // Fetch arc and line
                Arc? arc1 = null;
                Line? line1 = null;
                if (BaseEntityList[0] is Arc fetchedArc && BaseEntityList[1] is Line fetchedLine)
                {
                    arc1 = fetchedArc;
                    line1 = fetchedLine;
                }
                else if (BaseEntityList[1] is Arc fetchedArc2 && BaseEntityList[0] is Line fetchedLine2)
                {
                    arc1 = fetchedArc2;
                    line1 = fetchedLine2;
                }

                if (arc1 != null && line1 != null)
                {
                    // Check that end points connect
                    if ((line1.Start.Equals(arc1.Start) || line1.End.Equals(arc1.Start))
                        && (line1.Start.Equals(arc1.End) || line1.End.Equals(arc1.End)))
                    {
                        FeatureType = PossibleFeatureTypes.Group11;
                        return true;
                    }
                }
            }
            // Case 3
            else if (_numArcs == 3 && _numLines == 1)
            {
                // Fetch arcs and line
                // keeps track of index for concavity and endpoint checks
                Arc? bigArc = null;
                Arc? side1 = null;
                Arc? side2 = null;
                Line? line1 = null;
                foreach (Entity baseEntity in BaseEntityList)
                {
                    if (baseEntity is Arc arc)
                    {
                        if (bigArc is null)     bigArc = arc;
                        else if (side1 is null) side1 = arc;
                        else if (side2 is null) side2 = arc;
                    }
                    else
                    {
                        line1 = (Line)baseEntity;
                    }
                }
                // Swap the arcs to get the correct one under the correct label
                if (bigArc != null && side1 != null && bigArc.Radius < side1.Radius)
                {
                    (bigArc, side1) = (side1, bigArc);
                }
                if (bigArc != null && side2 != null && bigArc.Radius < side2.Radius)
                {
                    (bigArc, side2) = (side2, bigArc);
                }
                if (side1 != null && side2 != null && bigArc != null
                    && !IsConcave(side1) && !IsConcave(side2) && !IsConcave(bigArc)
                    && AreEndpointsTouching(line1, side1) && AreEndpointsTouching(line1, side2)
                    && AreEndpointsTouching(bigArc, side1) && AreEndpointsTouching(bigArc, side2))
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
    
    /// <summary>
    /// Checks the feature to see if it is group 12.
    /// </summary>
    /// <returns> Returns the possible feature type. </returns>
    private bool CheckGroup12()
    {
        if (_numCircles == 0 && _numEllipses == 0 && _numLines == 2)
        {
            if (_numArcs == 2)
            {
                if (IsSubshapeRectangle())
                {
                    FeatureType = PossibleFeatureTypes.Group12a;
                    return true;
                }
            }
            else if (_numArcs == 0 && IsPerpendicular((Line)BaseEntityList[0], (Line)BaseEntityList[1]))
            {
                FeatureType = PossibleFeatureTypes.Group12b;
                return true;
            }
        }
        FeatureType = PossibleFeatureTypes.Unknown;
        return false;
    }

    #endregion

    #region Group6base

    /// <summary>
    /// Checks the feature it is being called on to see if it is a group 6 base feature (trapezoid with radius corners).
    /// </summary>
    /// <returns> True if it is Group 6, false if not </returns>
    private bool CheckGroup6Base()
    {
        if (_numLines != 4 || _numArcs != 4)
        {
            return false;
        }

        // Number of different matching central angles of corner features
        int matchingPairs = 0;

        for (int i = 0; i < EntityList.Count; i++)
        {
            for (int j = i + 1; j < EntityList.Count; j++)
            {
                if (EntityList[i] is Arc iArc && EntityList[j] is Arc jArc)
                {
                    if (Math.Abs(iArc.CentralAngle - jArc.CentralAngle) < Entity.EntityTolerance)
                    {
                        matchingPairs++;
                    }
                }
            }
        }

        if (matchingPairs == 2)
        {
            // MDC said they don't have a trapezoid feature, and it would just be entered as a radius rectangle
            // FeatureType = PossibleFeatureTypes.Group6;
            FeatureType = PossibleFeatureTypes.Group1A2;
            return true;
        }

        return false;
    }

    #endregion

    #endregion

    #region PerimeterDetection

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
        return entityList.OfType<Line>().ToList();
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
            if (originLine.Equals(searchLine) || originLine.Equals(searchLine.SwapStartEnd())) continue;
    
            Point originPoint = fromStart? originLine.Start : originLine.End;

            // if end meets start or start meets end
            if (originPoint.Equals(fromStart ? searchLine.End : searchLine.Start))
            {
                touchingLine = searchLine;
                break;
            }
            // if end meets end or start meets start

            if (originPoint.Equals(fromStart ? searchLine.Start : searchLine.End))
            {
                touchingLine = searchLine.SwapStartEnd();
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

            // rider says the control variable is not used, but it IS
            while (!exhaustedEndSearch)
            {
                (Line? possibleLine, bool wasFlipped) = GetTouchingLine(currentEndLine, lineList);

                //if null or already in line group, meaning found end of line loop
                if (possibleLine == null 
                    || lineGroup.Contains(possibleLine)
                    || lineGroup.Contains(possibleLine.SwapStartEnd()))
                {
                    exhaustedEndSearch = true;
                    break;
                }
                
                currentEndLine = possibleLine;
                baseLineList.Remove(wasFlipped ? currentEndLine.SwapStartEnd() : currentEndLine);
                lineGroup.Add(possibleLine);
            }
            while (!exhaustedStartSearch)
            {
                (Line? possibleLine, bool wasFlipped) = GetTouchingLine(currentStartLine, lineList, true);

                //if null or already in lineGroup, meaning found end of line loop
                if (possibleLine == null 
                    || lineGroup.Contains(possibleLine)
                    || lineGroup.Contains(possibleLine.SwapStartEnd()))
                {
                    exhaustedStartSearch = true;
                    break;
                }
                
                currentStartLine = possibleLine;
                baseLineList.Remove(wasFlipped ? currentStartLine.SwapStartEnd() : currentStartLine);
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
                if (chamferGroup.ChamferIndex.Equals(lineList.IndexOf(line)))
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
            case <= 3:
            {
                RemoveFalseChamfers(lineList);
                break;
            }
        }
        // if more than 4 chamfers we run into the octagon problem
        // so we cannot confirm what lines are chamfers
    }

    #endregion

    #region Group4
    
    /// <summary>
    /// Checks the feature it is being called on to see if it is a group 4 feature.
    /// </summary>
    private void CheckGroup4()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature._numLines != 2 || (feature._numArcs != 2 && feature._numArcs != 0)))
            {
                foreach (Entity entity in feature.EntityList)
                {
                    if (entity is Line tempLine && DoesIntersect(tempLine, entity))
                    {
                        feature.FeatureType = PossibleFeatureTypes.Group4;
                    }
                }
            }
        }


        // for (int i = 0; i < PerimeterFeatureList.Count(); i++)
        // {
        //     // TODO check for chamfer
        //     if (PerimeterFeatureList[i] == 1 && PerimeterFeatureList[i] is Line line)
        //     {
        //         // Break up both of the lines touching the kiss cut line
        //         Point LineAIntersect = Entity.GetIntersectPoint(line, ((Line)PerimeterFeatureList[i][0].AdjList[0]));
        //         Point LineBIntersect = Entity.GetIntersectPoint(line, ((Line)PerimeterFeatureList[i][0].AdjList[1]));
        //
        //
        //         if (LineAIntersect.Equals(line.StartPoint))
        //         {
        //             
        //         }
        //         else
        //         {
        //             
        //         }
        //         
        //         
        //         
        //     }
        //     
        // }

        
        
        
        // check if a line that is not in base shape is touching kiss-cut line in adjacency list

        /*if (numLines == 4)
    {
        // check if a line is kiss cut
        // set list as Kiss-Cut
    }*/
        // loop through baseEntityList, if a line is kiss cut kiss-cut=True, then return
    }

    #endregion
    
    #region Group9

    private void CheckGroup9()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            foreach (Entity entity in feature.EntityList)
            {
                foreach (Entity adjEntity in entity.AdjList)
                {
                    if (adjEntity.KissCut)
                    {
                        feature.KissCut = true;
                        feature.FeatureType = PossibleFeatureTypes.Group9;
                        return;
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
    private void CheckGroup5()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature is not { _numLines: 2, _numArcs: 1 } && feature is not { _numLines: 3, _numArcs: 0 or 2 }) && !feature.KissCut)
            {
                bool con = true;
                foreach (Entity entity in feature.EntityList)
                {
                    if (entity is Arc arc && !DEQ(arc.CentralAngle, 90) && !DEQ(arc.CentralAngle, 180))
                    {
                        con = false;
                    }
                }

                if (con && HasTwoParallelLine(feature.EntityList))
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
    private void CheckGroup6Perimeter()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature._numLines < 2 || feature._numCircles != 0 || feature._numArcs < 3 || feature._numArcs > 4))
            {
                bool con = true;
                foreach (Entity entity in feature.EntityList)
                {
                    if (entity is Arc arc && !DEQ(arc.CentralAngle, 90) && !DEQ(arc.CentralAngle, 180))
                    {
                        con = false;
                    }
                }

                if (con && HasTwoParallelLine(feature.EntityList))
                {
                    feature.FeatureType = PossibleFeatureTypes.Group6;
                }
            }
        }
    }

    #endregion
    
    #region Group17

    /// <summary>
    /// Checks the feature it is being called on to see if it is a group 17 feature.
    /// </summary>
    private void CheckGroup17()
    {
        foreach (Feature feature in PerimeterFeatureList)
        {
            if (!(feature._numLines != 2 || feature._numCircles != 0 || feature._numArcs != 1))
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

    /// <summary>
    /// if you want to use this look into what variables are used for hash code 
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(EntityList, KissCut);
    }

    /// <summary>
    /// Overriding the Equals method to compare two Feature objects
    /// </summary>
    /// <param name="obj"> the object being compared to this </param>
    /// <returns> true if the objects are equal </returns>
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;

        if (!(obj is Feature feature)) return false;

        // performs reference compare not value compare
        if (feature == this) return true;
        
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
         * If there are the same number of arcs lines and circles, and perimeters match,
         * then check to see if all entities have a corresponding entity with matching values
         */
        if (feature._numLines != _numLines
            || feature._numCircles != _numCircles
            || feature._numArcs != _numArcs
            || Math.Abs(feature.perimeter - perimeter) >= Entity.EntityTolerance)
        {
            return false;
        }

        //sort both lists by length
        EntityList.Sort((x, y) => x.GetLength().CompareTo(y.GetLength()));
        feature.EntityList.Sort((x, y) => x.GetLength().CompareTo(y.GetLength()));

        //For each entity in this.EntityList check for a corresponding entity obj.EntityList
        foreach (Entity j in feature.EntityList)
        {
            if (!EntityList.Any(e => Math.Abs(e.GetLength() - j.GetLength()) < Entity.EntityTolerance))
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region ExtendEntities

    /// <summary>
    /// Recursive function that calls extendAllEntitiesHelper. Sets extendedEntityList to EntityList.
    /// This is the main function that should be called to extend entities
    /// </summary>
    public void ExtendAllEntities()
    {
        ExtendedEntityList = new List<Entity>(EntityList);
        ExtendAllEntitiesHelper();
    }

    /// <summary>
    /// This is a recursive helper function to extend every line in ExtendedEntityList. It will loop through ExtendedEntityList,
    /// previously set to EntityList, until it can find a Line to extend. Will recurse if extended. Base case is no more lines to extend
    /// Should be N^N runtime seeing as the nested for loops is N^2, then it is called recursively with N-1 every time.
    /// This makes it (((N!)^2) * N!) which is
    /// </summary>
    private void ExtendAllEntitiesHelper()
    {
        bool extendedALine = false;
        int i = 0;
        do
        {
            if (ExtendedEntityList[i] is Line line)
            {
                int j = 0;
                do
                {
                    if ((ExtendedEntityList[j] is Line) && !line.Equals(ExtendedEntityList[j]) )
                    {
                        // for each entity it checks if it can extend with every other entity and does so
                        // removes the two previous entities
                        // new extended lines are added in the extendTwoLines method
                        if (ExtendTwoLines(line, (Line)ExtendedEntityList[j]))
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

    /// <summary>
    /// Method that takes two lines and extends them to touch if they are:
    /// 1. not already touching
    /// 2. are parallel or perpendicular
    /// adds extended line(parallel) or lines(perpendicular) to extendedEntityList
    /// Perpendicular functionality has been commented out due to inconsistent slopes of lines,
    /// which means a perpendicular angle of intersection is not guaranteed on features it should be
    /// </summary>
    /// <param name="line1"> the first line being extended </param>
    /// <param name="line2"> the second line being extended</param>
    /// <returns> true if successfully extended. Could be false if the two lines don't have an intersect point,
    /// aren't the same infinite line, or already touch </returns>
    private bool ExtendTwoLines(Line line1, Line line2)
    {
        //makes sure you're not extending lines that already touch
        // Makes sure KissCut lines are not extended
        if (!DoesIntersect(line1, line2) && !line1.KissCut && !line2.KissCut)
        {
            if (EntityTools.IsCollinear(line1, line2))
            {
                // makes a new extended line object
                ExtendedLine tempLine = new ExtendedLine(line1, line2);
                ChangeAdjListForExtendedLine(tempLine, line1, line2);
                // extended two parallel lines into 1
                ExtendedEntityList.Remove(line1);
                ExtendedEntityList.Remove(line2);
                ExtendedEntityList.Add(tempLine);
                return true; 
            }
        }
        return false;
    }

    private static void ChangeAdjListForExtendedLine(ExtendedLine exLine, Line line1, Line line2)
    {
        // make the extended line's adjacency list
        exLine.AdjList = new List<Entity>(line1.AdjList);
        exLine.AdjList.AddRange(line2.AdjList);
        exLine.AdjList.Remove(line1);
        exLine.AdjList.Remove(line2);
        
        // replace line1 and line2 with exLine in the adjacency lists for entities touching line1 and line2
        for (int i = 0; i < line1.AdjList.Count; i++)
        {
            List<Entity> tempList = new(line1.AdjList[i].AdjList);
            tempList.Remove(line1);
            tempList.Add(exLine);
            line1.AdjList[i].AdjList = tempList;
        }
        for (int i = 0; i < line2.AdjList.Count; i++)
        {
            List<Entity> tempList = new(line2.AdjList[i].AdjList);
            tempList.Remove(line2);
            tempList.Add(exLine);
            line2.AdjList[i].AdjList = tempList;
        }
    }

    #endregion

    #region SeperateBaseEntities

    /// <summary>
    /// Function that separates the base entities, which will have been extended, if possible, at this point,
    /// from ExtendedEntityList into baseEntityList. Most logic for separation lies in separateBaseEntitiesHelper
    /// </summary>
    /// <returns> true if successfully separates base entities </returns>
    public bool SeparateBaseEntities()
    {
        // case where the feature contains a circle or ellipse
        if (ExtendedEntityList[0] is Circle or Ellipse)
        {
            // it should be the only entity in the list
            if (ExtendedEntityList.Count == 1 && BaseEntityList.Count == 0)
            {
                // adds the circle to the baseEntityList
                BaseEntityList.Add(ExtendedEntityList[0]);
                return true;
            }
        }

        // lists to pass to the helper function
        Stack<Entity> curPath = new Stack<Entity>();
        List<Entity> testedEntities = new List<Entity>();

        // default head is the first index of ExtendedEntityList
        Entity head = ExtendedEntityList[0];
        foreach (Entity entity in ExtendedEntityList)
            // this finds the entity with the greatest length and makes it the head to hopefully reduce runtime
        {
            if (entity.GetLength() > head.GetLength())
            {
                head = entity;
            }
        }

        // pushes the head to the current Path
        curPath.Push(head);
        if (SeparateBaseEntitiesHelper(curPath, testedEntities, head))
            // if it can find a Path
        {
            // converts the stack to an Entity<List>
            BaseEntityList = curPath.ToList();
            // reverses the order of it since the iterator that converts the stack flips it
            BaseEntityList.Reverse();
            return true;
        }

        return false;
    }

    /// <summary>
    /// recursive helper function to find a closed shape with extended lines
    /// </summary>
    /// <param name="curPath"> the current Path that has been taken </param>
    /// <param name="testedEntities"> a list of entities that have been visited </param>
    /// <param name="head"> the target entity that is trying to loop back through.
    /// comparisons with head are done by reference to make sure head is from one of the lists</param>
    /// <returns> true if a Path has been found </returns>
    private bool SeparateBaseEntitiesHelper(Stack<Entity> curPath, List<Entity> testedEntities, Entity head)
    {
        if (curPath.Count > 2)
        {
            //base case where the current entity touches the head (means it's a closed shape)
            //checks if contained in visitedEntities to avoid the second entity from triggering this
            //checks if current entity is the same as head to avoid a false true
            if (curPath.Peek() != head && AreEndpointsTouching(curPath.Peek(), head) 
                && !testedEntities.Contains(curPath.Peek()))
            {
                return true; //Path found
            }
        }

        //adds the current entity to the visitedEntities
        testedEntities.Add(curPath.Peek()); 

        foreach (Entity entity in ExtendedEntityList)
        {
            if (entity != curPath.Peek())
            {
                // checks if entity in loop is not the current entity being checked
                if (AreEndpointsTouching(curPath.Peek(), entity) && !testedEntities.Contains(entity))
                {
                    // checks that the entity has not already been tested and is touching the entity
                    //adds to stack
                    curPath.Push(entity);
                    //recursive call with updated Path
                    if (SeparateBaseEntitiesHelper(curPath, testedEntities, head))
                    {
                        return true;
                    }
                }
            }
        }
        //this point in the function means nothing is touching current entity

        //if the function of the head reaches this point it means it has not found a Path back to the head
        if (curPath.Peek() == head)
        {
            foreach (Entity entity in ExtendedEntityList)
            {
                if (entity is Ellipse)
                {
                    continue;
                }
                // finds the first entity that has not been tested and selects it as the head
                if (!testedEntities.Contains(entity)) 
                {
                    //clears Path and adds the new head to it
                    curPath.Clear(); 
                    curPath.Push(entity);
                    return SeparateBaseEntitiesHelper(curPath, testedEntities, entity);
                }
            }
        }

        curPath.Pop();
        return false; //nothing is touching this entity so it is popped off of Path
    }

    #endregion

    #region PerimeterFeatureFunctions

    /// <summary>
    /// Function that uses finds the Path from the two parents of all extended lines and adds the Path as a group of
    /// entities at new index in PerimeterFeatureList
    /// </summary>
    public void SeparatePerimeterEntities()
    {
        // lists to pass to the helper function
        List<Entity> path = new();

        foreach (Entity entity in BaseEntityList) // removes all base entities from ExtendedEntityList
        {
            ExtendedEntityList.Remove(entity);
        }

        AddBackParents();
        // at this point ExtendedEntityList should only contain perimeter features

        List<Entity> unusedEntities = new(ExtendedEntityList);

        //separatePerimeterEntitiesHelper(Path, unusedEntities, null);

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

    /// <summary>
    /// Recursive function that adds all entities in unusedEntities that intersect curEntity into Path
    /// </summary>
    /// <param name="path"> the list of touching entities</param>
    /// <param name="unusedEntities"> all available entities to add</param>
    /// <param name="currentEntity"> is the current entity being checked </param>
    private void GetTouchingList(List<Entity> path, List<Entity> unusedEntities, Entity? currentEntity)
    {
        if (currentEntity is null)
        {
            currentEntity = unusedEntities[0];
            path.Add(unusedEntities[0]);
            unusedEntities.RemoveAt(0);
        }

        List<Entity> touchingList = new();
        for (int i = 0; i < unusedEntities.Count; i++) // adds all entities in unusedEntities that touch curEntity to Path and touchingList and removes them from unusedEntities
        {
            if (DoesIntersect(currentEntity, unusedEntities[i]))
            {
                touchingList.Add(unusedEntities[i]);
                path.Add(unusedEntities[i]);
                unusedEntities.Remove(unusedEntities[i]);
                // I needs to stay the same since everything to the right of the moved entity is shifted left once
                i--; 
            }
        }

        foreach (Entity entity in touchingList)
        {
            GetTouchingList(path, unusedEntities, entity);
        }
    }

    /// <summary>
    /// Adds back all parents of extended lines that are not in baseEntityList back into ExtendedEntityList
    /// </summary>
    private void AddBackParents()
    {
        for (int i = 0; i < ExtendedEntityList.Count; i++)
        {
            if (ExtendedEntityList[i] is ExtendedLine && (!BaseEntityList.Contains(ExtendedEntityList[i])))
            {
                AddBackParentsHelper((ExtendedLine)ExtendedEntityList[i], ExtendedEntityList);
                i--;
            }
        }
    }

    private void AddBackParentsHelper(ExtendedLine exLine, List<Entity> targetList)
    {
        bool addedParent = false;
        if (exLine.Parent1 is ExtendedLine parent1)
        {
            AddBackParentsHelper(parent1, targetList);
        }
        else
        {
            targetList.Add(exLine.Parent1);
            addedParent = true;
        }

        if (exLine.Parent2 is ExtendedLine parent2)
        {
            AddBackParentsHelper(parent2, targetList);
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
                e.AdjList.Add(DoesIntersect(e, exLine.Parent1) ? exLine.Parent1 : exLine.Parent2);
                e.AdjList.Remove(exLine);
            }
        }
        
        // targetList will not have a parent that is an extended line in it
        targetList.Remove(exLine); 
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

    /// <summary>
    /// Checks if the angles of all the arcs add up to 360
    /// </summary>
    private bool DoAnglesAddTo360()
    {
        double sumAngles = 0;
        BaseEntityList.ForEach(entity =>
        {
            if (entity is Arc arcEntity) sumAngles += arcEntity.CentralAngle;
        });

        return sumAngles is > 359.9 and < 360.09;
    }

    /// <summary>
    /// Function that checks if the list passed in has at least one set of parallel lines
    /// </summary>
    /// <param name="entities"> the Entity list that is checked </param>
    /// <returns> true if a set of parallel lines is found </returns>
    private static bool HasTwoParallelLine(List<Entity> entities)
    {
        foreach (Entity e1 in entities)
        {
            foreach (Entity e2 in entities)
            {
                if (e1 is Line line1 && e2 is Line line2)
                {
                    if (line1 == line2) continue;
                    if (IsParallel(line1, line2)) return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Function that calculates the perimeter of this feature by going through every
    /// entity in EntityList and adding the length. This should only be called once, and
    /// probably by the constructor, but the perimeter = 0 is a safeguard in case this is
    /// called more than once.
    /// </summary>
    private void CalcPerimeter()
    {
        perimeter = 0;
        foreach (Entity entity in EntityList)
        {
            perimeter += entity.GetLength();
        }

        if (FeatureType == PossibleFeatureTypes.Group1B1 || FeatureType == PossibleFeatureTypes.Punch)
        {
            diameter = perimeter / Math.PI;
        }
    }

    /// <summary>
    /// Function that checks the number of unique radius lengths in a feature.
    /// multipleRadius is initially set to 1, incremented for each unique radius found.
    /// </summary>
    private void CheckMultipleRadius()
    {
        // Only can be run on Group 1A2, 1C, and 2A2
        if (FeatureType is not (PossibleFeatureTypes.Group1A2
            or PossibleFeatureTypes.Group1C or PossibleFeatureTypes.Group2A2)) return;
        
        // TODO: Add logic to check for multiple radii on bowtie features
        if (FeatureType == PossibleFeatureTypes.Group2A2) return;

        // Create a new list containing only the arc entities
        List<Arc> arcList = BaseEntityList.OfType<Arc>().ToList();

        // Remove duplicate radii arcs
        for (int i = 0; i < arcList.Count; i++)
        {
            for (int j = i + 1; j < arcList.Count; j++)
            {
                if (Math.Abs(arcList[i].Radius - arcList[j].Radius) < Entity.EntityTolerance)
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
                if (arcList[i] is Arc iArc && arcList[j] is Arc jArc)
                {
                    if (Math.Abs(iArc.Radius - jArc.Radius) > Entity.EntityTolerance)
                    {
                        multipleRadius += 1;
                    }
                }
            }
        }
    }
}