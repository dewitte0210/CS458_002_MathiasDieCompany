using FeatureRecognitionAPI.Models;
using Convert = System.Convert;
using Math = System.Math;

namespace FeatureRecognitionAPI.Models;

using FeatureRecognitionAPI.Models.Enums;
using iText.Pdfua.Checkers.Utils;

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
    private double calcCentralAngle(double startAngle, double endAngle)
    {
        //The subtraction result would be negative, need to add 360 to get correct value
        if (endAngle < startAngle)
            return endAngle - startAngle + 360;
        return endAngle - startAngle;
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

    //TODO: these calculations aren't completely robust,
    //but they should be good enough for calculating the bounding box of the canvas on the front end
    public override double MinX()
    {
        return Math.Min(Math.Min(Start.X, Center.X), End.X);
    }

    public override double MinY()
    {
        return Math.Min(Math.Min(Start.Y, Center.Y), End.Y);
    }

    public override double MaxX()
    {
        return Math.Max(Math.Max(Start.X, Center.X), End.X);
    }

    public override double MaxY()
    {
        return Math.Max(Math.Max(Start.Y, Center.Y), End.Y);
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