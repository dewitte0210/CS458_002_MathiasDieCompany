using static FeatureRecognitionAPI.Models.Utility.MdcMath;

namespace FeatureRecognitionAPI.Models.Entities;

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
        

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

    public static double Distance(Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }
}