using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Utility;
using Convert = System.Convert;
using Math = System.Math;

namespace FeatureRecognitionAPI.Models;

/// <summary>
/// Class that represents a Arc object that extends Entity
/// Inherits entityType and Length fields 
/// </summary>
public class Arc : Entity
{
    public Point Center { get; set; } //Central point
    public double Radius { get; set; } //Radius value
    public double StartAngle { get; set; } //angle from central point to start point
    public double EndAngle { get; set; } //angle from central point to end point
    public double CentralAngle { get; } //angle of the arc

    /// <summary>
    /// Creates an arc and calculates the starting and ending coordinates as well
    /// as the length of the arc. Makes it so Arc has no default constructor
    /// </summary>
    /// <param name="centerX"> the x value of the center of the arc </param>
    /// <param name="centerY"> the y value of the center of the arc </param>
    /// <param name="radius"> the radius value of the arc </param>
    /// <param name="startAngle"> the angle the arc starts at </param>
    /// <param name="endAngle"> the angle the arc ends at </param>
    public Arc(double centerX, double centerY, double radius, double startAngle, double endAngle)
    {
        Center = new Point(centerX, centerY);
        Radius = radius;
        StartAngle = startAngle;
        EndAngle = endAngle;
        Start = new(CalcXCoord(centerX, radius, startAngle), CalcYCoord(centerY, radius, startAngle));
        End = new Point(CalcXCoord(centerX, radius, endAngle), CalcYCoord(centerY, radius, endAngle));
        CentralAngle = CalcCentralAngle(startAngle, endAngle);
        Length = (CalcLength(radius, CentralAngle));
    }

    public override double GetLength()
    {
        return CalcLength(Radius, CentralAngle);
    }
    
    /// <summary>
    /// Function to calculate the x coordinate given the center point, Radius
    /// and an angle. 
    /// </summary>
    private static double CalcXCoord(double x, double radius, double angle)
    {
        return (radius * Math.Cos(Angles.DegToRadians(angle)) + x);
    }

    /// <summary>
    /// Function to calculate the y coordinate given the center point, Radius
    /// and an angle.
    /// </summary>
    private static double CalcYCoord(double y, double radius, double angle)
    {
        return (radius * Math.Sin(Angles.DegToRadians(angle)) + y);
    }

    /// <summary>
    /// Function to calculate the central angle 
    /// </summary>
    /// <param name="startAngle"> the start angle of the arc being calculated </param>
    /// <param name="endAngle"> the end angle of the arc being calculated </param>
    /// <returns> the calculated the length of the arc </returns>
    internal static double CalcCentralAngle(double startAngle, double endAngle)
    {
        //The subtraction result would be negative, need to add 360 to get correct value
        if (endAngle < startAngle)
            return endAngle - startAngle + 360;
        return endAngle - startAngle;
    }

    /// <summary>
    /// Function to calculate the length of the arc for perimeter length checks 
    /// </summary>
    /// <param name="radius"> the radius value of the arc being calculated </param>
    /// <param name="centralAngle"> the central angle for the arc being calculated </param>
    /// <returns> the calculated length (partial circumference) of the arc </returns>
    private static double CalcLength(double radius, double centralAngle)
    {
        return (2 * Math.PI * radius * (centralAngle / 360));
    }

    /// <summary>
    /// Overrides .Equals function for the Arc object
    /// </summary>
    /// <param name="obj"> object being compared to this </param>
    /// <returns> true if the same arc, false if not </returns>
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

    /// <summary>
    /// Function that determines if a point is in between the start and end angles
    /// (already checked to be on the line if the arc is treated as a circle)
    /// Mostly used in the intersect functions
    /// </summary>
    /// <param name="point"> the point being checked </param>
    /// <returns></returns>
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
            degrees = Angles.RadToDegrees(tan);
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
        return new Line(Center.X, Center.Y, Radius * Math.Cos(angle) + Center.X, Radius * Math.Sin(angle) + Center.Y);
    }

    internal double AngleInMiddle()
    {
        // rotate start and end angles to start at 0
        double difference = 360 - StartAngle;
        double adjustedStart = 0;
        double adjustedEnd = EndAngle + difference;
        if (adjustedEnd >= 360) { adjustedEnd -= 360; }
        double middleAngle = ((adjustedEnd - adjustedStart) / 2) + StartAngle;
        if (middleAngle >= 360) { middleAngle -= 360; }
        return middleAngle;
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
    
    /// <summary>
    /// This function and its helper methods are adapted from the Java.awt library.
    /// Note: comments which appeared in the original source code will be labelled as //**
    /// </summary>
    public Rect GetBounds()
    {
        //these numbers are coordinates relative to the unit circle
        double x1, y1, x2, y2;
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
    
    /// <summary>
    /// ** Normalizes the specified angle into the range -180 to 180.
    /// </summary>
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

    /// <summary>
    /// Return true if this and other should be combined into one larger arc.
    /// </summary>
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

    public override Arc Transform(Matrix3 transform)
    {
            Point newCenter = transform * Center;
            Point newStart = transform * Start;
            
            double rotation = Angles.RadToDegrees(Math.Acos(transform.GetUnderlyingMatrix().m00));
            
            double newRadius = Point.Distance(newStart, newCenter);
            double newAngleStart = StartAngle - rotation;
            double newAngleEnd = EndAngle - rotation;

            return new Arc(newCenter.X, newCenter.Y, newRadius, newAngleStart, newAngleEnd);
    }
}