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

        public void setPoint(double x, double y) {
            X = x;
            Y = y; 
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }
            if (this.X == ((Point)obj).X && this.Y == ((Point)obj).Y)
            {
                //dont need to check the intersect value
                return true;
            }
            return false;
        }
    }
}