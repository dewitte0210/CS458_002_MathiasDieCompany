using FeatureRecognitionAPI.Models.Utility;
using static FeatureRecognitionAPI.Models.Utility.MdcMath;

namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// Class that represents an Ellipse object that extends Entity
/// Inherits entityType and Length fields
/// </summary>
public class Ellipse : Entity
{
    public Point Center { get; set; }
    public Point MajorAxisEndPoint { get; set; }
    public Point MajorAxisVectorFromCenter { get; set; }
    public double MajorAxis { get; set; }
    public double MinorAxis { get; set; }
    public double MinorToMajorAxisRatio { get; set; }
    public double StartParameter { get; set; }
    public double EndParameter { get; set; }
    public double Rotation { get; set; }
    public bool IsFullEllipse { get; set; }
        
    public Ellipse(double centerX, double centerY, double majorAxisXValue,
        double majorAxisYValue, double minorToMajorAxisRatio,
        double startParameter, double endParameter)
    {
        Center = new Point(centerX, centerY);
        MajorAxisEndPoint = new Point(majorAxisXValue + centerX, majorAxisYValue + centerY);
        MajorAxisVectorFromCenter = new Point(majorAxisXValue, majorAxisYValue);
        MajorAxis = Point.Distance(MajorAxisEndPoint, Center);
        MinorAxis = MajorAxis * minorToMajorAxisRatio;
        MinorToMajorAxisRatio = minorToMajorAxisRatio;
        StartParameter = startParameter;
        EndParameter = endParameter;
        Rotation = Math.Atan2(MajorAxisEndPoint.Y - Center.Y, MajorAxisEndPoint.X - Center.X);
        Start = PointOnEllipseGivenAngleInRadians(MajorAxis, MinorAxis, StartParameter);
        End = PointOnEllipseGivenAngleInRadians(MajorAxis, MinorAxis, EndParameter);
        if (Rotation > 0)
        {
            Start.X -= Center.X;
            Start.Y -= Center.Y;
            End.X -= Center.X;
            End.Y -= Center.Y;

            //Rotate around the origin
            double temp = Start.X;
            Start.X = -1 * ((Start.X * Math.Cos(Rotation)) - (Start.Y * Math.Sin(Rotation)));
            Start.Y = -1 * ((Start.Y * Math.Cos(Rotation)) + (temp * Math.Sin(Rotation)));
            temp = End.X;
            End.X = -1 * ((End.X * Math.Cos(Rotation)) - (End.Y * Math.Sin(Rotation)));
            End.Y = -1 * ((End.Y * Math.Cos(Rotation)) + (temp * Math.Sin(Rotation)));

            //Translate back
            Start.X += Center.X;
            Start.Y += Center.Y;
            End.X += Center.X;
            End.Y += Center.Y;
        }
        if (DoubleEquals(startParameter, 0) && DoubleEquals(endParameter, 2 * Math.PI))
        {
            IsFullEllipse = true;
        }
        else
        {
            IsFullEllipse = false;
        }
    }

    private double FullPerimeterCalc()
    {
        //Major axis Radius
        double majorAxis = Point.Distance(MajorAxisEndPoint, Center);
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

    /// <summary>
    /// This function breaks down the perimeter of the partial ellipse into a series of small lines that
    /// follow the actual perimeter of the partial ellipse. It is an accurate estimate of the perimeter
    /// since an exact formula does not exist.
    /// </summary>
    private double PartialPerimeterCalc()
    {
        //Major axis value
        double a = Point.Distance(MajorAxisEndPoint, Center);
        //Minor axis value
        double b = MinorToMajorAxisRatio * a;
        //Return value for perimeter
        double sum = 0;
        //Num of lines that will trace the actual perimeter
        int numLines = (int)Math.Ceiling(360 * (3.4064 * (a - 2) + 3.0258 * (b - 1)));
        //Increment value between each angle
        double angleIncrement;
        //Adjust for rotated ellipses
        if (EndParameter < StartParameter)
        {
            double difference = (2 * Math.PI) - StartParameter;
            double adjustedStart = 0;
            double adjustedEnd = EndParameter + difference;
            angleIncrement = adjustedEnd - adjustedStart / numLines;
        }
        else
        {
            angleIncrement = (EndParameter - StartParameter) / numLines;
        }
        for (int i = 0; i < numLines; i++)
        {
            double ang1 = (i * angleIncrement) + StartParameter;
            Point p1 = PointOnEllipseGivenAngleInRadians(a, b, ang1);
            Point p2 = PointOnEllipseGivenAngleInRadians(a, b, ang1 + angleIncrement);
            Line lineOnPerimeter = new Line(p1.X, p1.Y, p2.X, p2.Y);
            sum += lineOnPerimeter.GetLength();
        }
        return sum;
    }
        
    /// <summary>
    /// Calculates the coordinate on an ellipse given the angle in radians.
    /// </summary>
    /// <param name="a"> Major axis value </param>
    /// <param name="b"> Minor axis value </param>
    /// <param name="angle"> angle of coordinate desired in Radians </param>
    internal Point PointOnEllipseGivenAngleInRadians(double a, double b, double angle)
    {
        double x1;
        double y1;
        //Special cases for pi/2 and 3pi/2
        switch ((angle % (2 * Math.PI)) / (2 * Math.PI))
        {
            case 0.25:
                x1 = 0;
                y1 = b;
                break;
            case 0.75:
                x1 = 0;
                y1 = -1 * b;
                break;
            default:
                x1 = (a * b) / (Math.Sqrt(Math.Pow(b, 2) + (Math.Pow(a, 2) * Math.Pow(Math.Tan(angle), 2))));
                //Tan limitation adjusted
                if (angle % (2 * Math.PI) < 3 * (Math.PI / 2) && angle % (2 * Math.PI) > (Math.PI / 2))
                {
                    x1 *= -1;
                }
                break;
        }
        //Special cases for 0, pi/2, pi, and 3pi/2
        switch ((angle % (2 * Math.PI)) / (2 * Math.PI))
        {
            case 0:
                y1 = 0;
                break;
            case 0.25:
                y1 = b;
                break;
            case 0.5:
                y1 = 0;
                break;
            case 0.75:
                y1 = -1 * b;
                break;
            default:
                y1 = (a * b) / Math.Sqrt(Math.Pow(a, 2) + (Math.Pow(b, 2) / Math.Pow(Math.Tan(angle), 2)));
                //Tan limitation adjusted
                if (angle % (2 * Math.PI) > Math.PI && angle % (2 * Math.PI) < (2 * Math.PI))
                {
                    y1 *= -1;
                }
                break;
        }
        Point sol = new Point(x1 + Center.X, y1 + Center.Y);
        return sol;
    }

    /**
     * Takes an angle and creates a line representing a vector pointing out from the center in the
     * direction of the angle.
     */
    public Line VectorFromCenter(double angle)
    {
        double a = Point.Distance(MajorAxisEndPoint, Center);
        Point endPoint = PointOnEllipseGivenAngleInRadians(a, MinorToMajorAxisRatio * a, angle);
        return new Line(Center.X, Center.Y, endPoint.X + Center.X, endPoint.Y + Center.Y);
    }

    /// <summary>
    /// Checks if a given point on the ellipse is in range of the parameter boundaries
    /// </summary>
    internal bool IsInEllipseRange(Point point)
    {
        double y = point.Y - Center.Y;
        double x = point.X - Center.X;
        double pointAngle;
        if (DoubleEquals(x, 0))
        {
            pointAngle = y > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
        }
        else if (DoubleEquals(y, 0))
        {
            pointAngle = x > 0 ? 0 : Math.PI;
        }
        else
        {
            pointAngle = Math.Atan2(y, x);
            //Q2 and Q3
            if (x < 0)
            {
                pointAngle += Math.PI;
            }
            //Q4
            else if (x > 0 && y < 0)
            {
                pointAngle += 2 * Math.PI;
            }
        }
        double ellipseY = MajorAxisEndPoint.Y - Center.Y;
        double ellipseX = MajorAxisEndPoint.X - Center.X;
        double ellipseRotation;
        if (DoubleEquals(ellipseX, 0))
        {
            ellipseRotation = ellipseY > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
        }
        else if (DoubleEquals(ellipseY, 0))
        {
            ellipseRotation = ellipseX > 0 ? 0 : Math.PI;
        }
        else
        {
            ellipseRotation = Math.Atan2(ellipseY, ellipseX);
        }
        //Adjusting for ellipse rotation
        pointAngle -= ellipseRotation;
        if (pointAngle < 0)
        {
            pointAngle += 2 * Math.PI;
        }
        return pointAngle >= Math.Round(StartParameter, 4) && pointAngle <= Math.Round(EndParameter, 4);
    }

    public override double GetLength()
    {
        return (IsFullEllipse)? FullPerimeterCalc() : PartialPerimeterCalc();
    }

    //TODO: finish this
    public override bool Equals(object? obj)
    {
        if (obj is Ellipse)
        {
            //intentional reference comparison
            if (this == obj)
            {
                return true;
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Center, MajorAxis, MinorAxis, Rotation);
    }

    public override double MinX()
    {
        //Base cases
        if (DoubleEquals(Rotation, 0) || DoubleEquals(Rotation, Math.PI))
        {
            //If the major axis is not in the partial ellipse, the end points have to be the min
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, 0) && !(Math.PI >= StartParameter && Math.PI <= EndParameter))
                     || (DoubleEquals(Rotation, Math.PI) && !(0 >= StartParameter && 0 <= EndParameter)))
                {
                    return Math.Min(Start.X, End.X);
                }
            }
            return Center.X - MajorAxis;
        }
        else if (DoubleEquals(Rotation, Math.PI / 2) || DoubleEquals(Rotation, 3 * Math.PI / 2))
        {
            //If the minor axis is not in the partial ellipse, the end points have to be the min
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, Math.PI / 2) && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter))
                     || (DoubleEquals(Rotation, 3 * Math.PI / 2) && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter)))
                {
                    return Math.Min(Start.X, End.X);
                }
            }
            return Center.X - MinorAxis;
        }
        List<Point> values = MaxAndMinX();
        //Min value
        double min = 0;
        //Index tracker
        int index = 0;
        for (int i = 0; i < values.Count; i++)
        {
            index = i;
            if (i == 0) min = values[i].X;
            else if (values[i].X < min) min = values[i].X;
        }
        //Checks if the calculated min is in range of the parameters
        if (!IsInEllipseRange(values[index])) { return Math.Min(Start.X, End.X); }
        return min;
    }

    public override double MinY()
    {
        //Base cases
        if (DoubleEquals(Rotation, 0) || DoubleEquals(Rotation, Math.PI))
        {
            //If the minor axis is not in the partial ellipse, the end points have to be the min
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, 0) && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter))
                    || (DoubleEquals(Rotation, Math.PI) && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter)))
                {
                    return Math.Min(Start.Y, End.Y);
                }
            }
            return Center.Y - MinorAxis;
        }
        //If the major axis is not in the partial ellipse, the end points have to be the min
        else if (DoubleEquals(Rotation, Math.PI / 2) || DoubleEquals(Rotation, 3 * Math.PI / 2))
        {
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, Math.PI / 2) && !(Math.PI >= StartParameter && Math.PI <= EndParameter)) 
                     || (DoubleEquals(Rotation, 3 * Math.PI / 2) && !(0 >= StartParameter && 0 <= EndParameter)))
                {
                    return Math.Min(Start.Y, End.Y);
                }
            }
            return Center.Y - MajorAxis;
        }
        List<Point> values = MaxAndMinY();
        //Min value
        double min = 0;
        //Index tracker
        int index = 0;
        for (int i = 0; i < values.Count; i++)
        {
            index = i;
            if (i == 0) min = values[i].Y;
            else if (values[i].Y < min) min = values[i].Y;
        }
        //Checks if the calculated min is in range of the parameters
        if (!IsInEllipseRange(values[index])) { return Math.Min(Start.Y, End.Y); }
        return min;
    }

    public override double MaxX()
    {
        //Base cases
        if (DoubleEquals(Rotation, 0) || DoubleEquals(Rotation, Math.PI))
        {
            //If the major axis is not in the partial ellipse, the end points have to be the max
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, 0) && !(0 >= StartParameter && 0 <= EndParameter)) 
                     || (DoubleEquals(Rotation, Math.PI) && !(Math.PI >= StartParameter && Math.PI <= EndParameter)))
                {
                    return Math.Max(Start.X, End.X);
                }
            }
            return Center.X + MajorAxis;
        }
        //If the minor axis is not in the partial ellipse, the end points have to be the max
        else if (DoubleEquals(Rotation, Math.PI / 2) || DoubleEquals(Rotation, 3 * Math.PI / 2))
        {
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, Math.PI / 2) && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter)) 
                     || (DoubleEquals(Rotation, 3 * Math.PI / 2) && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter)))
                {
                    return Math.Max(Start.X, End.X);
                }
            }
            return Center.X + MinorAxis;
        }
        List<Point> values = MaxAndMinX();
        //Max value
        double max = 0;
        //Index tracker
        int index = 0;
        for (int i = 0; i < values.Count; i++)
        {
            index = i;
            if (i == 0) max = values[i].X;
            else if (values[i].X > max) max = values[i].X;
        }
        //Checks if the calculated max is in range of the parameters
        if (!IsInEllipseRange(values[index])) { return Math.Max(Start.X, End.X); }
        return max;
    }

    public override double MaxY()
    {
        //Base cases
        if (DoubleEquals(Rotation, 0) || DoubleEquals(Rotation, Math.PI))
        {
            //If the minor axis is not in the partial ellipse, the end points have to be the max
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, 0) && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter)) 
                    || (DoubleEquals(Rotation, Math.PI) && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter)))
                {
                    return Math.Max(Start.Y, End.Y);
                }
            }
            return Center.Y + MinorAxis;
        }
        //If the major axis is not in the partial ellipse, the end points have to be the max
        else if (DoubleEquals(Rotation, Math.PI / 2) || DoubleEquals(Rotation, 3 * Math.PI / 2))
        {
            if (!IsFullEllipse)
            {
                if ((DoubleEquals(Rotation, Math.PI / 2) && !(0 >= StartParameter && 0 <= EndParameter)) 
                    || (DoubleEquals(Rotation, 3 * Math.PI / 2) && !(Math.PI >= StartParameter && Math.PI <= EndParameter)))
                {
                    return Math.Max(Start.Y, End.Y);
                }
            }
            return Center.Y + MajorAxis;
        }
        List<Point> values = MaxAndMinY();
        //Max value
        double max = 0;
        //Index tracker
        int index = 0;
        for (int i = 0; i < values.Count; i++)
        {
            index = i;
            if (i == 0) max = values[i].Y;
            else if (values[i].Y > max) max = values[i].Y;
        }
        //Checks if the calculated max is in range of the parameters
        if (!IsInEllipseRange(values[index])) { return Math.Max(Start.Y, End.Y); }
        return max;
    }

    #region Bounds
    /**
     * Calculates the y-axis bounds of the ellipse
     * @Return - The 2 points on the ellipse corresponding to the bounds
     */
    private List<Point> MaxAndMinY()
    {
        double a = 0, b = 0, c = 0, d = 0, e = 0, alpha = 0;
        CalculateEllipseConstants(ref a, ref b, ref c, ref d, ref e, ref alpha);
        double denominator = Math.Sin(Rotation) * Math.Cos(Rotation) * (Math.Pow(MinorAxis, 2) - Math.Pow(MajorAxis, 2));
        double slope = -1 * ((Math.Pow(MajorAxis, 2) * Math.Pow(Math.Sin(Rotation), 2)) + (Math.Pow(MinorAxis, 2) * Math.Pow(Math.Cos(Rotation), 2))) / denominator;
        double gamma = ((Center.X * ((Math.Pow(MajorAxis, 2) * Math.Pow(Math.Sin(Rotation), 2)) + (Math.Pow(MinorAxis, 2) * Math.Pow(Math.Cos(Rotation), 2)))) / denominator) + Center.Y;
        List<double> xValues = CalcXCoordOfBoundCoords(a, b, c, d, e, alpha, slope, gamma);
        List<Point> yValues = new List<Point>();
        foreach (double result in xValues)
        {
            yValues.Add(new Point(result, slope * result + gamma));
        }
        return yValues;
    }

    /**
     * Calculates the x-axis bounds of the ellipse
     * @Return - The 2 points on the ellipse corresponding to the bounds
     */
    private List<Point> MaxAndMinX()
    {
        double a = 0, b = 0, c = 0, d = 0, e = 0, alpha = 0;
        CalculateEllipseConstants(ref a, ref b, ref c, ref d, ref e, ref alpha);
        double denominator = (Math.Pow(MinorAxis, 2) * Math.Pow(Math.Sin(Rotation), 2)) + (Math.Pow(MajorAxis, 2) * Math.Pow(Math.Cos(Rotation), 2));
        double slope = ((Math.Pow(MajorAxis, 2) - Math.Pow(MinorAxis, 2)) * Math.Sin(Rotation) * Math.Cos(Rotation)) / denominator;
        double beta = (((Math.Pow(MinorAxis, 2) * Center.X * Math.Sin(Rotation) * Math.Cos(Rotation)) - (Math.Pow(MajorAxis, 2) * Center.X * Math.Sin(Rotation) * Math.Cos(Rotation))) / denominator) + Center.Y;
        List<double> xValues = CalcXCoordOfBoundCoords(a, b, c, d, e, alpha, slope, beta);
        List<Point> yValues = new List<Point>();
        foreach (double result in xValues)
        {
            yValues.Add(new Point(result, slope * result + beta));
        }
        return yValues;
    }

    /**
     * Calculates the constants in the general form of an ellipse (Ax^2 + Bx + Cy^2 + Dy + Exy + alpha)
     */
    private void CalculateEllipseConstants(ref double A, ref double B, ref double C, ref double D, ref double E, 
        ref double alpha)
    {
        A = (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MinorAxis, 2));
        B = (Center.Y * Math.Sin(2 * Rotation) * (Math.Pow(MinorAxis, -2) - Math.Pow(MajorAxis, -2))) - (2 * Center.X * ((Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MinorAxis, 2))));
        C = (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MinorAxis, 2));
        D = (Center.X * Math.Sin(2 * Rotation) * (Math.Pow(MinorAxis, -2) - Math.Pow(MajorAxis, -2))) - (2 * Center.Y * ((Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MinorAxis, 2))));
        E = Math.Sin(2 * Rotation) * (Math.Pow(MajorAxis, -2) - Math.Pow(MinorAxis, -2));
        alpha = (Math.Pow(Center.X, 2) * ((Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MinorAxis, 2)))) + (Center.X * Center.Y * Math.Sin(2 * Rotation) * (Math.Pow(MajorAxis, -2) - Math.Pow(MinorAxis, -2))) + (Math.Pow(Center.Y, 2) * ((Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MinorAxis, 2)))) - 1;
    }

    /**
     * Takes the bounding lines and plugs them into the ellipse equation for the quadratic formula
     * @Return - The x values of the bounding coords
     */
    private static List<double> CalcXCoordOfBoundCoords(double a, double b, double c, double d, double e, 
        double alpha, double slope, double intercept)
    {
        double squaredCoefficient = a + (slope * ((c * slope) + e));
        double linearCoefficient = intercept * ((2 * c * slope) + e) + b + (d * slope);
        double delta = intercept * ((c * intercept) + d) + alpha;
        return QuadraticFormula(squaredCoefficient, linearCoefficient, delta);
    }
    #endregion

    public override Ellipse Transform(Matrix3 transform)
    {
        throw new NotImplementedException("Ellipses within insert blocks are not yet supported.");
    }
}