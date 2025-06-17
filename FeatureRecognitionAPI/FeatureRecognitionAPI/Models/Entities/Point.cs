using static FeatureRecognitionAPI.Models.Utility.MdcMath;

namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// A class used for points on the cartesian plane.
/// </summary>
public class Point
{
    public double X { get; set; } // X coordinate.
    public double Y { get; set; } // Y coordinate.


    public Point()
    {
    }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public Point(Point point)
    {
        this.X = point.X;
        this.Y = point.Y;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Point point)
        {
            return DEQ(X, point.X) && DEQ(Y, point.Y);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    /// <summary>
    /// Calculates the distance between two points.
    /// </summary>
    /// <returns> The distance calculated. </returns>
    public static double Distance(Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }
}