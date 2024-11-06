namespace FeatureRecognitionAPI.Models
{
    public class Point
    {
        public double X;
        public double Y;
        public bool intersect; //boolean value that is true if this point is an intersect point between two lines

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
    }
}