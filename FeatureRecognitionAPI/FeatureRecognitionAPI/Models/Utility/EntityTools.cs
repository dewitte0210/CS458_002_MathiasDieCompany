using static FeatureRecognitionAPI.Models.Utility.Angles;

namespace FeatureRecognitionAPI.Models.Utility;

public class EntityTools
{
    public class ExtendTwoLinesResult(Line line1, Line line2, bool success)
    {
        public readonly Line Line1 = line1;
        public readonly Line Line2 = line2;
        public readonly bool Success = success;
    };

    /// <summary>
    /// Extends two lines to their intersect point. Changes the value of the lines passed through.
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns> returns whether the lines were extended or not </returns>
    public static bool ExtendTwoLines(Line? line1, Line? line2)
    {
        if (line1 == null || line2 == null) return false;
        
        //if parallel do not extend 
        if (IsParallel(line1, line2)) return false;

        Point intPoint = Entity.GetIntersectPoint(line1, line2);
        
        //Line extended1 = new(line1.StartPoint, line1.EndPoint);
        double line1StartDistance = Point.Distance(line1.StartPoint, intPoint);
        double line1EndDistance = Point.Distance(line1.EndPoint, intPoint);
        //if start is closer to intersect set that as new start
        if (line1StartDistance < line1EndDistance)
        {
            line1.StartPoint = intPoint;
        }
        else
        {
            line1.EndPoint = intPoint;
        }
        
        //Line extended2 = new(line2.StartPoint, line2.EndPoint);
        double line2StartDistance = Point.Distance(line2.StartPoint, intPoint);
        double line2EndDistance = Point.Distance(line2.EndPoint, intPoint);
        if (line2StartDistance < line2EndDistance)
        {
            line2.StartPoint = intPoint;
        }
        else
        {
            line2.EndPoint = intPoint;
        }

        return true;
    }
}