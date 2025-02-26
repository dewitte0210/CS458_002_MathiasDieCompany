namespace FeatureRecognitionAPI.Models
{
    /**
     * Class that represents a Circle object that extends Entity
     * Inherits entityType and Length fields
     */
    public class Circle : Entity
    {
        public Point Center { get; set; }//Center point of circle
        public double Radius { get; set; }//Radius of circle

        /**
         * Constructor that takes an x, y and radius value
         * calls calcPerimeter
         * makes it so Circle has no default constructor
         * 
         * @param CenterX x coordinate of central point
         * @param CenterY y coordinate of central point
         * @param radius value of the radius of this circle
         */
        public Circle(double centerX, double centerY, double radius)
        {
            Center = new Point(centerX, centerY);
            this.Radius = radius;
            this.Length = (calcPerimeter(radius));
        }

        /**
         * Function that calculates the perimeter (circumference)
         * 
         * @param radius is the radius value that is used to calculate the perimeter
         */
        private double calcPerimeter(double radius)
        {
            return 2 * Math.PI * radius;
        }

        /**
         * Overides .Equals function for the Arc object
         * 
         * @param obj is the object that is being compared
         * @return true if equal, otherwise false
         */
        public override bool Equals(object? obj)
        {
            //If the object is a cricle, and the circles have even Radius, within tollerance then the circles are equal

            if (obj is Circle)
            {
                if (Math.Abs(((Circle)obj).Radius - this.Radius) < EntityTolerance)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public override bool Compare(object? obj)
        {
            //If the object is a cricle, and the circles have even Radius, within tollerance then the circles are equal

            if (obj is Circle)
            {
                if (Math.Abs(((Circle)obj).Radius - this.Radius) < EntityTolerance)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }
        
        public override double MinX()
        {
            return Center.X - Radius;
        }

        public override double MinY()
        {
            return Center.Y - Radius;
        }

        public override double MaxX()
        {
            return Center.X + Radius;
        }

        public override double MaxY()
        {
            return Center.Y + Radius;
        }

    }
}
