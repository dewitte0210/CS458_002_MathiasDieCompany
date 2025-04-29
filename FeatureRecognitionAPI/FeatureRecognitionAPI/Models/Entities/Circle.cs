using FeatureRecognitionAPI.Models.Utility;
using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// Class that represents a Circle object that extends Entity
/// Inherits entityType and Length fields
/// </summary>
public class Circle : Entity
{
    public Point Center { get; set; }//Center point of circle
    public double Radius { get; set; }//Radius of circle
        
    [JsonIgnore] public new Point Start
    {
        get { throw new NotSupportedException("Circle does not have a start point.");}
        set{ throw new NotSupportedException("Circle does not have a start point.");}
    }

    [JsonIgnore] public new Point End
    {
        get { throw new NotSupportedException("Circle does not have an end point."); }
        set { throw new NotSupportedException("Circle does not have an end point."); }
    }

    /// <summary>
    /// Constructor that takes an x, y and radius value
    /// calls calcPerimeter
    /// makes it so Circle has no default constructor
    /// </summary>
    /// <param name="centerX"> x coordinate of central point </param>
    /// <param name="centerY"> y coordinate of central point </param>
    /// <param name="radius"> value of the radius of this circle </param>
    public Circle(double centerX, double centerY, double radius)
    {
        Center = new Point(centerX, centerY);
        this.Radius = radius;
    }

    /// <summary>
    /// Function that calculates the perimeter (circumference)
    /// </summary>
    public override double GetLength()
    {
        return 2 * Math.PI * Radius;
    }

    /// <summary>
    /// Overrides .Equals function for the Arc object
    /// </summary>
    /// <param name="obj"> the object that is being compared </param>
    /// <returns> true if equal, otherwise false </returns>
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

    public override Circle Transform(Matrix3 transform)
    {
        Point newCenter = transform * Center;
        Point newRadiusPoint = transform * new Point(Center.X + Radius, Center.Y + Radius);
        double dist = Point.Distance(newCenter, newRadiusPoint);
        return new Circle(newCenter.X, newCenter.Y, dist);
    }
}