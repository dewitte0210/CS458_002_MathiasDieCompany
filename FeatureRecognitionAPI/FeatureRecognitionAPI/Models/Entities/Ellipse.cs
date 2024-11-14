using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models
{
    /**
     * Class that represents a Ellipse object that extends Entity
     * Inherits entityType and Length fields
     */
    public class Ellipse : Entity
    {
        public Point Center { get; set; }
        public double MajorAxisXValue { get; set; }
        public double MajorAxisYValue { get; set; }
        public Point MajorAxisEndPoint { get; set; }
        public double ExtrusionDirectionX {  get; set; }
        public double ExtrusionDirectionY { get; set; }
        public double MinorToMajorAxisRatio { get; set; }
        public double StartParameter { get; set; }
        public double EndParameter { get; set; }
        public bool IsFullEllipse { get; set; }
        private Ellipse() { }
        public Ellipse(double centerX, double centerY, double majorAxisXValue, 
            double majorAxisYValue, double extrusionDirectionX, double extrusionDirectionY,
            double minorToMajorAxisRatio, double startParameter, double endParameter)
        {
            Center = new Point(centerX, centerY);
            MajorAxisEndPoint = new Point(majorAxisXValue, majorAxisYValue);
            this.ExtrusionDirectionX = extrusionDirectionX;
            this.ExtrusionDirectionY = extrusionDirectionY;
            this.MinorToMajorAxisRatio = minorToMajorAxisRatio;
            this.StartParameter = startParameter;
            this.EndParameter = endParameter;
            if (startParameter == 0 && endParameter == 2 * Math.PI)
            {
                this.IsFullEllipse = true;
                Length = fullPerimeterCalc();
            }
            //TODO Partial ellipse length :(
            else
            {
                this.IsFullEllipse = false;
                Length = partialPerimterCalc();
            }
        }

        private double fullPerimeterCalc()
        {
            //Major axis Radius
            double majorAxis;
            if (Center.X == MajorAxisXValue)
            {
                majorAxis = MajorAxisYValue - Center.Y;
            }
            else
            {
                majorAxis = MajorAxisXValue - Center.X;
            }
            double a = 1;
            double g = MinorToMajorAxisRatio;
            double total = (Math.Pow(a, 2) - Math.Pow(g, 2)) / 2;
            for (int i = 0; i < 5; i++)
            {
                double temp = a;
                a = (a + g) / 2;
                g = Math.Sqrt(temp * g);
                total += Math.Pow(2, i) * (Math.Pow(a, 2) - Math.Pow(g, 2));
            }
            return 4 * majorAxis * Math.PI / (2 * a) * (1 - total);
        }

        //TODO might not be possible
        private double partialPerimterCalc()
        {
            return fullPerimeterCalc() / ((EndParameter - StartParameter) / 2 * Math.PI);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Ellipse)
            {

            }
            return false;
        }
    }
}
