namespace FeatureRecognitionAPI.Models
{
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
                double xDiff = Math.Abs(point.X - this.X);
                double yDiff = Math.Abs(point.Y - this.Y);
                if (xDiff < 0.0009 && yDiff < 0.0009)
                {
                    return true;
                }
            }
            return false;
        }
        
        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}