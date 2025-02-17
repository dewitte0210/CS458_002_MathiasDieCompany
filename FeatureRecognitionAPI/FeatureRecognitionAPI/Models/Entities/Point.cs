namespace FeatureRecognitionAPI.Models
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool intersect; //boolean value that is true if this point is an intersect point between two lines

        //this is only true on points where the intersection point has to be calculated,
        //not on points where lines already touch

        public Point()
        {
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
            intersect = false;
        }

        public Point(Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
            this.intersect = point.intersect;
        }

        public void setPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Point)
            {
                double xDiff = Math.Abs((obj as Point).X - this.X);
                double yDiff = Math.Abs((obj as Point).Y - this.Y);
                if (xDiff < 0.0009 && yDiff < 0.0009)
                {
                    return true;
                }
            }
            return false;
        }
    }
}