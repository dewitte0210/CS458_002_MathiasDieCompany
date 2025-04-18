﻿using FeatureRecognitionAPI.Models.Utility;

namespace FeatureRecognitionAPI.Models
{
    /**
     * Class that represents a Ellipse object that extends Entity
     * Inherits entityType and Length fields
     */
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
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public bool IsFullEllipse { get; set; }
        private Ellipse() { }
        public Ellipse(double centerX, double centerY, double majorAxisXValue,
            double majorAxisYValue, double minorToMajorAxisRatio,
            double startParameter, double endParameter)
        {
            Center = new Point(centerX, centerY);
            MajorAxisEndPoint = new Point(majorAxisXValue + centerX, majorAxisYValue + centerY);
            MajorAxisVectorFromCenter = new Point(majorAxisXValue, majorAxisYValue);
            MajorAxis = Point.Distance(MajorAxisEndPoint, Center);
            MinorAxis = MajorAxis * minorToMajorAxisRatio;
            this.MinorToMajorAxisRatio = minorToMajorAxisRatio;
            this.StartParameter = startParameter;
            this.EndParameter = endParameter;
            Rotation = Math.Atan2(MajorAxisEndPoint.Y - Center.Y, MajorAxisEndPoint.X - Center.X);
            StartPoint = PointOnEllipseGivenAngleInRadians(MajorAxis, MinorAxis, StartParameter);
            EndPoint = PointOnEllipseGivenAngleInRadians(MajorAxis, MinorAxis, EndParameter);
            if (Rotation > 0)
            {
                StartPoint.X = StartPoint.X - Center.X;
                StartPoint.Y = StartPoint.Y - Center.Y;
                EndPoint.X = EndPoint.X - Center.X;
                EndPoint.Y = EndPoint.Y - Center.Y;

                //Rotate around the origin
                double temp = StartPoint.X;
                StartPoint.X = -1 * ((StartPoint.X * Math.Cos(Rotation)) - (StartPoint.Y * Math.Sin(Rotation)));
                StartPoint.Y = -1 * ((StartPoint.Y * Math.Cos(Rotation)) + (temp * Math.Sin(Rotation)));
                temp = EndPoint.X;
                EndPoint.X = -1 * ((EndPoint.X * Math.Cos(Rotation)) - (EndPoint.Y * Math.Sin(Rotation)));
                EndPoint.Y = -1 * ((EndPoint.Y * Math.Cos(Rotation)) + (temp * Math.Sin(Rotation)));

                //Translate back
                StartPoint.X = StartPoint.X + Center.X;
                StartPoint.Y = StartPoint.Y + Center.Y;
                EndPoint.X = EndPoint.X + Center.X;
                EndPoint.Y = EndPoint.Y + Center.Y;
            }
            if (startParameter == 0 && endParameter == 2 * Math.PI)
            {
                this.IsFullEllipse = true;
                Length = fullPerimeterCalc();
            }
            else
            {
                this.IsFullEllipse = false;
                Length = partialPerimterCalc();
            }
        }

        private double fullPerimeterCalc()
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

        /**
         * This function breaks down the perimeter of the partial ellipse into a series of small lines that
         * follow the actual perimeter of the partial ellipse. It is an accurate estimate of the perimeter
         * since an exact formula does not exist.
         */
        private double partialPerimterCalc()
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
                Line lineOnPerim = new Line(p1.X, p1.Y, p2.X, p2.Y);
                sum += lineOnPerim.Length;
            }
            return sum;
        }

        /**
         * Calculates the coordinate on an ellipse given the angle in radians.
         * 
         * @Param a - Major axis value
         * @PAram b - Minor axis value
         * @Param angle - angle of coordinate desired
         */
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

        public Line vectorFromCenter(double angle)
        {
            double a = Point.Distance(MajorAxisEndPoint, Center);
            Point endPoint = PointOnEllipseGivenAngleInRadians(a, MinorToMajorAxisRatio * a, angle);
            return new Line(Center.X, Center.Y, endPoint.X + Center.X, endPoint.Y + Center.Y);
        }

        /**
         * Checks if a given point on the ellipse is in range of the parameter boundaries
         */
        internal bool isInEllipseRange(Point point)
        {
            double y = point.Y - Center.Y;
            double x = point.X - Center.X;
            double pointAngle;
            if (x == 0)
            {
                pointAngle = y > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
            }
            else if (y == 0)
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
            if (ellipseX == 0)
            {
                ellipseRotation = ellipseY > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
            }
            else if (ellipseY == 0)
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
        //TODO: finish this
        public override bool Compare(object? obj)
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

        public override double MinX()
        {
            if (Rotation == 0 || Rotation == Math.PI)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == 0 && !(Math.PI >= StartParameter && Math.PI <= EndParameter)) || (Rotation == Math.PI && !(0 >= StartParameter && 0 <= EndParameter)))
                    {
                        return Math.Min(StartPoint.X, EndPoint.X);
                    }
                }
                return Center.X - MajorAxis;
            }
            else if (Rotation == Math.PI / 2 || Rotation == 3 * Math.PI / 2)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == Math.PI / 2 && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter)) || (Rotation == 3 * Math.PI / 2 && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter)))
                    {
                        return Math.Min(StartPoint.X, EndPoint.X);
                    }
                }
                return Center.X - MinorAxis;
            }
            List<Point> values = MaxAndMinX();
            double min = 0;
            int index = 0;
            for (int i = 0; i < values.Count; i++)
            {
                index = i;
                if (i == 0) { min = values[i].X; }
                else if (values[i].X < min) { min = values[i].X; }
            }
            if (!isInEllipseRange(values[index])) { return Math.Min(StartPoint.X, EndPoint.X); }
            return min;
        }

        public override double MinY()
        {
            if (Rotation == 0 || Rotation == Math.PI)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == 0 && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter)) || (Rotation == Math.PI && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter)))
                    {
                        return Math.Min(StartPoint.Y, EndPoint.Y);
                    }
                }
                return Center.Y - MinorAxis;
            }
            else if (Rotation == Math.PI / 2 || Rotation == 3 * Math.PI / 2)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == Math.PI / 2 && !(Math.PI >= StartParameter && Math.PI <= EndParameter)) || (Rotation == 3 * Math.PI / 2 && !(0 >= StartParameter && 0 <= EndParameter)))
                    {
                        return Math.Min(StartPoint.Y, EndPoint.Y);
                    }
                }
                return Center.Y - MajorAxis;
            }
            List<Point> values = MaxAndMinY();
            double min = 0;
            int index = 0;
            for (int i = 0; i < values.Count; i++)
            {
                index = i;
                if (i == 0) { min = values[i].Y; }
                else if (values[i].Y < min) { min = values[i].Y; }
            }
            if (!isInEllipseRange(values[index])) { return Math.Min(StartPoint.Y, EndPoint.Y); }
            return min;
        }

        public override double MaxX()
        {
            if (Rotation == 0 || Rotation == Math.PI)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == 0 && !(0 >= StartParameter && 0 <= EndParameter)) || (Rotation == Math.PI && !(Math.PI >= StartParameter && Math.PI <= EndParameter)))
                    {
                        return Math.Max(StartPoint.X, EndPoint.X);
                    }
                }
                return Center.X + MajorAxis;
            }
            else if (Rotation == Math.PI / 2 || Rotation == 3 * Math.PI / 2)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == Math.PI / 2 && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter)) || (Rotation == 3 * Math.PI / 2 && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter)))
                    {
                        return Math.Max(StartPoint.X, EndPoint.X);
                    }
                }
                return Center.X + MinorAxis;
            }
            List<Point> values = MaxAndMinX();
            double max = 0;
            int index = 0;
            for (int i = 0; i < values.Count; i++)
            {
                index = i;
                if (i == 0) { max = values[i].X; }
                else if (values[i].X > max) { max = values[i].X; }
            }
            if (!isInEllipseRange(values[index])) { return Math.Max(StartPoint.X, EndPoint.X); }
            return max;
        }

        public override double MaxY()
        {
            if (Rotation == 0 || Rotation == Math.PI)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == 0 && !(Math.PI / 2 >= StartParameter && Math.PI / 2 <= EndParameter)) || (Rotation == Math.PI && !(3 * Math.PI / 2 >= StartParameter && 3 * Math.PI / 2 <= EndParameter)))
                    {
                        return Math.Max(StartPoint.Y, EndPoint.Y);
                    }
                }
                return Center.Y + MinorAxis;
            }
            else if (Rotation == Math.PI / 2 || Rotation == 3 * Math.PI / 2)
            {
                if (!IsFullEllipse)
                {
                    if ((Rotation == Math.PI / 2 && !(0 >= StartParameter && 0 <= EndParameter)) || (Rotation == 3 * Math.PI / 2 && !(Math.PI >= StartParameter && Math.PI <= EndParameter)))
                    {
                        return Math.Max(StartPoint.Y, EndPoint.Y);
                    }
                }
                return Center.Y + MajorAxis;
            }
            List<Point> values = MaxAndMinY();
            double max = 0;
            int index = 0;
            for (int i = 0; i < values.Count; i++)
            {
                index = i;
                if (i == 0) { max = values[i].Y; }
                else if (values[i].Y > max) { max = values[i].Y; }
            }
            if (!isInEllipseRange(values[index])) { return Math.Max(StartPoint.Y, EndPoint.Y); }
            return max;
        }

        private List<Point> MaxAndMinY()
        {
            double A = 0, B = 0, C = 0, D = 0, E = 0, alpha = 0;
            CalculateEllipseConstants(ref A, ref B, ref C, ref D, ref E, ref alpha);
            double denom = Math.Sin(Rotation) * Math.Cos(Rotation) * (Math.Pow(MinorAxis, 2) - Math.Pow(MajorAxis, 2));
            double slope = -1 * ((Math.Pow(MajorAxis, 2) * Math.Pow(Math.Sin(Rotation), 2)) + (Math.Pow(MinorAxis, 2) * Math.Pow(Math.Cos(Rotation), 2))) / denom;
            double gamma = ((Center.X * ((Math.Pow(MajorAxis, 2) * Math.Pow(Math.Sin(Rotation), 2)) + (Math.Pow(MinorAxis, 2) * Math.Pow(Math.Cos(Rotation), 2)))) / denom) + Center.Y;
            List<double> xValues = CalcXCoordOfBoundCoords(A, B, C, D, E, alpha, slope, gamma);
            List<Point> yValues = new List<Point>();
            foreach (double result in xValues)
            {
                yValues.Add(new Point(result, slope * result + gamma));
            }
            return yValues;
        }

        private List<Point> MaxAndMinX()
        {
            double A = 0, B = 0, C = 0, D = 0, E = 0, alpha = 0;
            CalculateEllipseConstants(ref A, ref B, ref C, ref D, ref E, ref alpha);
            double denom = (Math.Pow(MinorAxis, 2) * Math.Pow(Math.Sin(Rotation), 2)) + (Math.Pow(MajorAxis, 2) * Math.Pow(Math.Cos(Rotation), 2));
            double slope = ((Math.Pow(MajorAxis, 2) - Math.Pow(MinorAxis, 2)) * Math.Sin(Rotation) * Math.Cos(Rotation)) / denom;
            double beta = (((Math.Pow(MinorAxis, 2) * Center.X * Math.Sin(Rotation) * Math.Cos(Rotation)) - (Math.Pow(MajorAxis, 2) * Center.X * Math.Sin(Rotation) * Math.Cos(Rotation))) / denom) + Center.Y;
            List<double> xValues = CalcXCoordOfBoundCoords(A, B, C, D, E, alpha, slope, beta);
            List<Point> yValues = new List<Point>();
            foreach (double result in xValues)
            {
                yValues.Add(new Point(result, slope * result + beta));
            }
            return yValues;
        }

        private void CalculateEllipseConstants(ref double A, ref double B, ref double C, ref double D, ref double E, ref double alpha)
        {
            A = (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MinorAxis, 2));
            B = (Center.Y * Math.Sin(2 * Rotation) * (Math.Pow(MinorAxis, -2) - Math.Pow(MajorAxis, -2))) - (2 * Center.X * ((Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MinorAxis, 2))));
            C = (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MinorAxis, 2));
            D = (Center.X * Math.Sin(2 * Rotation) * (Math.Pow(MinorAxis, -2) - Math.Pow(MajorAxis, -2))) - (2 * Center.Y * ((Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MinorAxis, 2))));
            E = Math.Sin(2 * Rotation) * (Math.Pow(MajorAxis, -2) - Math.Pow(MinorAxis, -2));
            alpha = (Math.Pow(Center.X, 2) * ((Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MinorAxis, 2)))) + (Center.X * Center.Y * Math.Sin(2 * Rotation) * (Math.Pow(MajorAxis, -2) - Math.Pow(MinorAxis, -2))) + (Math.Pow(Center.Y, 2) * ((Math.Pow(Math.Sin(Rotation), 2) / Math.Pow(MajorAxis, 2)) + (Math.Pow(Math.Cos(Rotation), 2) / Math.Pow(MinorAxis, 2)))) - 1;
        }

        private List<double> CalcXCoordOfBoundCoords(double A, double B, double C, double D, double E, double alpha, double slope, double intercept)
        {
            double squaredCoef = A + (slope * ((C * slope) + E));
            double linearCoef = intercept * ((2 * C * slope) + E) + B + (D * slope);
            double delta = intercept * ((C * intercept) + D) + alpha;
            return QuadraticFormula(squaredCoef, linearCoef, delta);
        }

        public override Ellipse Transform(Matrix3 transform)
        {
            throw new NotImplementedException("Ellipses within insert blocks are not yet supported.");
        }
    }
}
