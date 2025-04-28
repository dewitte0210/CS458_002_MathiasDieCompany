using FeatureRecognitionAPI.Models.Entities;
using static FeatureRecognitionAPI.Models.Utility.Angles;

namespace FeatureRecognitionAPI.Models.Utility;

public static class EntityTools
{
    /// <summary>
    /// Determine if two lines are collinear. Two lines are collinear if they are parallel
    /// and lie on the same infinite line. NOT TESTED YET
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns> true if the two given lines are collinear </returns>
    public static bool IsCollinear(Line line1, Line line2)
    {
        if (!IsParallel(line1, line2)) return false;

        //turn the 2 lines into 3 vectors based on line1 start
        Point p1 = line1.GetDelta();
        Point p2 = new Line(line1.Start, line2.Start).GetDelta();
        Point p3 = new Line(line1.Start, line2.End).GetDelta();

        //if they are collinear these should both be 0
        double cross1 = double.Abs(CrossProduct(p1, p2));
        double cross2 = double.Abs(CrossProduct(p1, p3));

        return MdcMath.DoubleEquals(cross1, 0) && MdcMath.DoubleEquals(cross2, 0);
    }

    /// <summary>
    /// Extends two lines to their intersect point. Changes the value of the lines passed through.
    /// Only tested for non-touching line segments for chamfered line extension,
    /// change if you need advanced behavior
    /// </summary>
    /// <param name="line1"> nullable line A </param>
    /// <param name="line2"> nullable line B </param>
    /// <returns> returns whether the lines were successfully extended or not </returns>
    public static bool ExtendTwoLines(Line? line1, Line? line2)
    {
        if (line1 == null || line2 == null) return false;

        // todo: if collinear merge into one line
        if (IsCollinear(line1, line2)) return false;

        //if parallel do not extend 
        if (IsParallel(line1, line2)) return false;

        Point? intPoint = Intersect.GetInfiniteLineIntersect(line1, line2);
        if (intPoint == null) return false;

        // todo: update line adjacency lists
        double line1StartDistance = Point.Distance(line1.Start, intPoint);
        double line1EndDistance = Point.Distance(line1.End, intPoint);
        //if start is closer to intersect set that as new start
        if (line1StartDistance < line1EndDistance)
        {
            line1.Start = intPoint;
        }
        else
        {
            line1.End = intPoint;
        }

        double line2StartDistance = Point.Distance(line2.Start, intPoint);
        double line2EndDistance = Point.Distance(line2.End, intPoint);
        if (line2StartDistance < line2EndDistance)
        {
            line2.Start = intPoint;
        }
        else
        {
            line2.End = intPoint;
        }

        return true;
    }

    internal static List<Entity> CondenseArcs(List<Entity> entities)
    {
        List<Entity> returned = entities.Where(entity => !(entity is Arc)).ToList();

        List<IGrouping<int, Arc>> arcGroups = entities
            .OfType<Arc>()
            .GroupBy(arc => arc.GetHashCode()).ToList();

        List<Arc> arcs = new List<Arc>();
        foreach (var g in arcGroups)
        {
            List<Arc> group = g.ToList();
            Arc initArc = group[0];
            group.RemoveAt(0);

            if (group.Count == 0)
            {
                arcs.Add(initArc);
                continue;
            }

            int idx = 0;
            int failCount = 0;
            while (group.Count > 0)
            {
                if (failCount >= group.Count)
                {
                    arcs.Add(initArc);
                    idx = 0;
                    initArc = group[0];
                    group.RemoveAt(0);
                    if (group.Count == 0)
                    {
                        arcs.Add(initArc);
                        break;
                    }

                    failCount = 0;
                }

                Arc otherArc = group[idx];
                if (initArc.ConnectsTo(otherArc))
                {
                    Point center = initArc.Center;
                    double radius = initArc.Radius;

                    bool startAtSmallArcStart =
                        Math.Abs(otherArc.StartAngle + otherArc.CentralAngle - initArc.StartAngle) % 360 <
                        Entity.EntityTolerance;

                    double angleStart = startAtSmallArcStart ? otherArc.StartAngle : initArc.StartAngle;
                    double angleExtent = otherArc.CentralAngle + initArc.CentralAngle;

                    initArc = new Arc(center.X, center.Y, radius, angleStart, angleStart + angleExtent);
                    group.RemoveAt(idx);
                    failCount = 0;
                }
                else
                {
                    failCount++;
                }

                if (group.Count == 0)
                {
                    arcs.Add(initArc);
                    break;
                }

                idx = (idx + 1) % group.Count;
            }
        }

        // Convert arcs to circles if necessary
        foreach (Arc arc in arcs)
        {
            if (Math.Abs(arc.CentralAngle - 360) <= Entity.EntityTolerance)
            {
                returned.Add(new Circle(arc.Center.X, arc.Center.Y, arc.Radius));
            }
            else
            {
                returned.Add(arc);
            }
        }

        return returned;
    }
}