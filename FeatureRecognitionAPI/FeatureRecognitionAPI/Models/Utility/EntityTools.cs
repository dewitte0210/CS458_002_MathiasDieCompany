using static FeatureRecognitionAPI.Models.Utility.Angles;

namespace FeatureRecognitionAPI.Models.Utility;

public static class EntityTools
{
    /// <summary>
    /// find the intersect point of two lines
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns> returns the intersection point of two lines, null if not possible </returns>
    public static Point? GetIntersection(Line line1, Line line2)
    {
        Point l1Delta = line1.GetDelta();
        Point l2Delta = line2.GetDelta();
        
        Point xDiff = new(-l1Delta.X, -l2Delta.X);
        Point yDiff = new(-l1Delta.Y, -l2Delta.Y);

        double diffCross = CrossProduct(xDiff, yDiff);
        
        //lines are parallel
        if (MdcMath.DoubleEquals(diffCross, 0)) return null;
        
        Point d = new(CrossProduct(line1.Start, line1.End), CrossProduct(line2.Start, line2.End));
        double x = CrossProduct(d, xDiff) / diffCross;
        double y = CrossProduct(d, yDiff) / diffCross;
        
        return new Point(x, y);
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
        // todo: update line adjacency lists
        //if parallel do not extend 
        if (IsParallel(line1, line2)) return false;

        Point intPoint = GetIntersection(line1, line2);
        if (intPoint == null) return false;
        
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

        // update Length because for some reason it isn't a function
        line1.Length = Point.Distance(line1.Start, line1.End);
        line2.Length = Point.Distance(line2.Start, line2.End);

        return true;
    }
}