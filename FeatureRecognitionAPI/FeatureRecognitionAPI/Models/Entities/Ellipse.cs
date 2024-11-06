using FeatureRecognitionAPI.Models.Enums;
using NHibernate.Type;

namespace FeatureRecognitionAPI.Models
{
    public class Ellipse : Entity
    {
        public double centerX { get; set; }
        public double centerY { get; set; }
        public double majorAxisXValue { get; set; }
        public double majorAxisYValue { get; set; }
        public double extrusionDirectionX {  get; set; }
        public double extrusionDirectionY { get; set; }
        public double minorToMajorAxisRatio { get; set; }
        public double startParameter { get; set; }
        public double endParameter { get; set; }
        public bool isFullEllipse { get; set; }
        private Ellipse()
        {
            entityType = PossibleEntityTypes.ellipse;
        }
        public Ellipse(double centerX, double centerY, double majorAxisXValue, 
            double majorAxisYValue, double extrusionDirectionX, double extrusionDirectionY,
            double minorToMajorAxisRatio, double startParameter, double endParameter)
        {
            this.centerX = centerX;
            this.centerY = centerY;
            this.majorAxisXValue = majorAxisXValue;
            this.majorAxisYValue = majorAxisYValue;
            this.extrusionDirectionX = extrusionDirectionX;
            this.extrusionDirectionY = extrusionDirectionY;
            this.minorToMajorAxisRatio = minorToMajorAxisRatio;
            this.startParameter = startParameter;
            this.endParameter = endParameter;
            if (startParameter == 0 && endParameter == 2 * Math.PI)
            {
                this.isFullEllipse = true;
                this.setLength(fullPerimeterCalc());
            }
            //TODO Partial ellipse length :(
            else
            {
                this.isFullEllipse = false;
                this.setLength(partialPerimterCalc());
            }
        }

        private double fullPerimeterCalc()
        {
            //Major axis radius
            double majorAxis;
            if (centerX == majorAxisXValue)
            {
                majorAxis = majorAxisYValue - centerY;
            }
            else
            {
                majorAxis = majorAxisXValue - centerX;
            }
            double a = 1;
            double g = minorToMajorAxisRatio;
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
            return 0;
        }
    }
}
