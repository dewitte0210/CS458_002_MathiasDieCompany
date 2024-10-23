namespace FeatureRecognitionAPI.Models
{
    public class Point
    {
        public double X; public double Y; public bool intersect;

        public Point()
        {
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void setPoint(double x, double y) { X = x; Y = y; }

    }
}