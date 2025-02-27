using Convert = System.Convert;
using Math = System.Math;

namespace FeatureRecognitionAPI.Models;

/**
 * Class that represents a Arc object that extends Entity
 * Inherits entityType and Length fields
 */
public class Arc : Entity
{
    public Point Center { get; set; } //Central point
    public Point Start { get; set; } //Start point
    public Point End { get; set; } //End point
    public double Radius { get; set; } //Radius value
    public double StartAngle { get; set; } //angle from central point to start point
    public double EndAngle { get; set; } //angle from central point to end point
    public double CentralAngle { get; } //angle of the arc

    /**
     * Creates an arc and calculates the starting and ending coordinates as well
     * as the length of the arc. Makes it so Arc has no default constructor
     *
     * @param CenterX the x value of the center of the arc
     * @param CenterY the y value of the center of the arc
     * @param radius the radius value of the arc
     * @param startAngle the angle the arc starts at
     * @param endAngle the angle the arc ends at
     */
    public Arc(double centerX, double centerY, double radius, double startAngle, double endAngle)
    {
        Center = new Point(centerX, centerY);
        this.Radius = radius;
        this.StartAngle = startAngle;
        this.EndAngle = endAngle;
        Start = new(calcXCoord(centerX, radius, startAngle), calcYCoord(centerY, radius, startAngle));
        End = new Point(calcXCoord(centerX, radius, endAngle), calcYCoord(centerY, radius, endAngle));
        this.CentralAngle = calcCentralAngle(startAngle, endAngle);
        this.Length = (calcLength(radius, CentralAngle));
    }

    /**
     * Function for calculating radians for cos and sin calculations.
     *
     * @param degrees is the amount of degrees being converted
     * @return the radian value of degrees
     */
    private double degreesToRadians(double degrees)
    {
        return (degrees * Math.PI / 180);
    }

    /**
     * Function to calculate the x coordinate given the center point, Radius
     * and an angle.
     */
    private double calcXCoord(double x, double radius, double angle)
    {
        return (radius * Math.Cos(degreesToRadians(angle)) + x);
    }

    /**
     * Function to calculate the y coordinate given the center point, Radius
     * and an angle.
     */
    private double calcYCoord(double y, double radius, double angle)
    {
        return (radius * Math.Sin(degreesToRadians(angle)) + y);
    }

        /**
         * Function to calculate the central angle
         * 
         * @param startAngle the start angle of the arc being calculated
         * @param endAngle the end angle of the arc being calculated
         * @return the calculated the length of the arc
         */
        internal double calcCentralAngle(double startAngle, double endAngle)
        {
            // rotate start and end angles to start at 0
            double difference = 360 - startAngle;
            double adjustedStart = 0;
            double adjustedEnd = endAngle + difference;
            if (adjustedEnd >= 360) { adjustedEnd -= 360; }

            double centerAngle = (adjustedEnd - adjustedStart) / 2;
            if (centerAngle >= 360) { centerAngle -= 360; }
            return centerAngle;
        }

    /**
     * Function to calculate the length of the arc for perimeter length checks
     *
     * @param radius the radius value of the arc being calculated
     * @param centralAngle the central angle for the arc being calculated
     * @return the calculated length (partial circumference) of the arc
     */
    private double calcLength(double radius, double centralAngle)
    {
        return (2 * Math.PI * radius * (centralAngle / 360));
    }

    /**
     * Overides .Equals function for the Arc object
     *
     * @param obj object being compared to this
     * @return true if the same arc, false if not
     */
    public override bool Equals(object? obj)
    {
        if (obj is Arc)
        {
            //IDE Mapped everything to work with tolerance in one tab push :O
            if (Math.Abs(((Arc)obj).Length - this.Length) < EntityTolerance
                && Math.Abs(((Arc)obj).Radius - this.Radius) < EntityTolerance
                && Math.Abs(((Arc)obj).StartAngle - this.StartAngle) < EntityTolerance
                && Math.Abs(((Arc)obj).EndAngle - this.EndAngle) < EntityTolerance
                && ((Arc)obj).Start.Equals(this.Start) && ((Arc)obj).End.Equals(this.End))
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    public override bool Compare(object? obj)
    {
        if (obj is Arc)
        {
            if (Math.Abs(((Arc)obj).Length - this.Length) < EntityTolerance
                && Math.Abs(((Arc)obj).Radius - this.Radius) < EntityTolerance
                && Math.Abs(((Arc)obj).StartAngle - this.StartAngle) < EntityTolerance
                && Math.Abs(((Arc)obj).EndAngle - this.EndAngle) < EntityTolerance)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    /**
     * Function that determines if a point is in between the start and end angles
     * (already checked to be on the line if the arc is treated as a circle)
     * Mostly used in the intersect functions
     *
     * @param point is the point being checked
     */
    internal bool IsInArcRange(Point point)
    {
        double y = point.Y - Center.Y;
        double x = point.X - Center.X;
        double degrees;

        // Figure out the angle the point is in. Special cases apply at x=0 and y=0
        if (x == 0)
        {
            degrees = y > 0 ? 90 : 270;
        }
        else if (y == 0)
        {
            degrees = x > 0 ? 0 : 180;
        }
        else
        {
            double tan = Math.Atan2(y, x);
            degrees = tan * (180 / Math.PI);
            //Q2 and Q3
            if (x < 0)
            {
                // y < 0? Q3 else Q2
                degrees = y < 0 ? degrees += 360 : Math.Abs(degrees + 180);
            }
            //Q4
            else if (x > 0 && y < 0)
            {
                degrees = 360 + degrees;
            }
        }

        // rotate start and end angles to start at 0
        double difference = 360 - StartAngle;
        double adjustedStart = 0;
        double adjustedEnd = EndAngle + difference;
        double adjustedDegrees = degrees + difference;

        if (adjustedEnd >= 360)
        {
            adjustedEnd -= 360;
        }

        if (adjustedDegrees >= 360)
        {
            adjustedDegrees -= 360;
        }

        return adjustedDegrees >= adjustedStart && adjustedDegrees <= adjustedEnd;
    }

    internal Line VectorFromCenter(double angle)
    {
        return new Line(Center.X, Center.Y, 2 * Math.Cos(angle) + Center.X, 2 * Math.Sin(angle) + Center.Y);
    }

    public int GetHashCode()
    {
        //hash is built using ellipse center and arc radius
        //so two arcs with the same center and arc radius will have the same hash
        return Convert.ToInt32(
            Center.X * 10010111 +
            Center.Y * 10000379 +
            Radius * 10006721);
        //Note: this hash may not be robust enough
    }

    public override double MinX()
    {
        Rect bounds = GetBounds();
        return bounds.x;
    }
    
    public override double MinY()
    {
        Rect bounds = GetBounds();
        return bounds.y;
    }
    
    public override double MaxX()
    {
    
        Rect bounds = GetBounds();
        return bounds.x + bounds.width;
}
    
    public override double MaxY()
    {
        Rect bounds = GetBounds();
        return bounds.y + bounds.height;
    }

    public readonly struct Rect
    {
        public Rect(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public double x { get; init; }
        public double y { get; init; }
        public double width { get; init; }
        public double height { get; init; }
    }

    //This function and its helper methods are adapted from the Java.awt library.
    // Note: comments which appeared in the original source code will be labelled as //**
    public Rect GetBounds()
    {
        double x1, y1, x2, y2; //these numbers are coordinates relative to the unit circle
        x1 = y1 = 1.0;
        x2 = y2 = -1.0;
        double angle = 0.0;
        for (int i = 0; i < 6; i++)
        {
            if (i < 4)
            {
                //** 0-3 are the four quadrants
                angle += 90.0;
                if (!containsAngle(angle))
                {
                    continue;
                }
            }
            else if (i == 4)
            {
                //** 4 is start angle
                angle = StartAngle;
            }
            else
            {
                //** 5 is end angle
                angle += CentralAngle;
            }

            double rads = angle * (Math.PI / 180);
            double xe = Math.Cos(rads);
            double ye = Math.Sin(rads);
            x1 = Math.Min(x1, xe);
            y1 = Math.Min(y1, ye);
            x2 = Math.Max(x2, xe);
            y2 = Math.Max(y2, ye);
        }

        double width = (x2 - x1) *  Radius;
        double height = (y2 - y1) * Radius;

        double x  = Center.X  + x1  * Radius;
        double y = Center.Y + y1 * Radius;
        return new Rect(x, y, width, height);
    }

    public bool containsAngle(double angle)
    {
        double angExt = CentralAngle;
        bool backwards = (angExt < 0.0);
        if (backwards)
        {
            angExt = -angExt;
        }

        if (angExt >= 360.0)
        {
            return true;
        }

        angle = NormalizeDegrees(angle) - NormalizeDegrees(StartAngle);
        if (backwards)
        {
            angle = -angle;
        }

        if (angle < 0.0)
        {
            angle += 360.0;
        }


        return (angle >= 0.0) && (angle < angExt);
    }


    /*
     * ** Normalizes the specified angle into the range -180 to 180.
     */
    static double NormalizeDegrees(double angle)
    {
        if (angle > 180.0)
        {
            if (angle <= (180.0 + 360.0))
            {
                angle -= 360.0;
            }
            else
            {
                angle = Math.IEEERemainder(angle, 360.0);
                //** IEEEremainder can return -180 here for some input values...
                if (angle == -180.0)
                {
                    angle = 180.0;
                }
            }
        }
        else if (angle <= -180.0)
        {
            if (angle > (-180.0 - 360.0))
            {
                angle += 360.0;
            }
            else
            {
                angle = Math.IEEERemainder(angle, 360.0);
                //** IEEEremainder can return -180 here for some input values...
                if (angle == -180.0)
                {
                    angle = 180.0;
                }
            }
        }

        return angle;
    }

    /**
     * Return true if this and other should be combined into one larger arc.
     */
    public bool ConnectsTo(Arc other)
    {
        return !Equals(other) &&
               GetHashCode() == other.GetHashCode() &&
               (
                   Start.Equals(other.Start) ||
                   Start.Equals(other.End) ||
                   End.Equals(other.Start) ||
                   End.Equals(other.End)
               );
    }
}