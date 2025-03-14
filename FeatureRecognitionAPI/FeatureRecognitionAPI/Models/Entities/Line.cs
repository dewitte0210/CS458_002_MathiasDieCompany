using System.Web;
using FeatureRecognitionAPI.Models.Utility;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        private const double TOLERANCE = 0.00005;

        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public double SlopeY { get; set; }
        public double SlopeX { get; set; }

        // Don't Delete. Called from ExtendedLine constructor
        protected Line()
        {
            StartPoint = new Point();
            EndPoint = new Point();
        }

        public Line(Line line)
        {
            StartPoint = new Point(line.StartPoint);
            EndPoint = new Point(line.EndPoint);
            SlopeY = line.SlopeY;
            SlopeX = line.SlopeX;
            Length = line.Length;
        }

        public Line(double startX, double startY, double endX, double endY)
        {
            StartPoint = new Point(startX, startY);
            EndPoint = new Point(endX, endY);
            SlopeY = EndPoint.Y - StartPoint.Y;
            SlopeX = EndPoint.X - StartPoint.X;

            this.Length = Point.Distance(StartPoint, EndPoint);
        }


        public Line(Point startPoint, Point endPoint)
        {
            StartPoint = new Point(startPoint);
            EndPoint = new Point(endPoint);
            
            SlopeY = EndPoint.Y - StartPoint.Y;
            SlopeX = EndPoint.X - StartPoint.X;
            
            this.Length = Point.Distance(StartPoint, EndPoint);
        }

        //constructor with extendedline parameter
        public Line(double startX, double startY, double endX, double endY, bool extendedLine)
        {
            StartPoint = new Point(startX, startY);
            EndPoint = new Point(endX, endY);

            SlopeY = EndPoint.Y - StartPoint.Y;
            SlopeX = EndPoint.X - StartPoint.X;

            Length = (Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2) + Math.Pow(EndPoint.Y - StartPoint.Y, 2)));
        }

        public bool hasPoint(Point point)
        {
            return (StartPoint.Equals(point) || EndPoint.Equals(point));
        }

        public bool isParallel(Line line)
        {
            // Vertical line case
            if (Math.Abs(this.SlopeX) < Entity.EntityTolerance && Math.Abs(line.SlopeX) < Entity.EntityTolerance) { return true; }
            // Horizontal line case
            if (Math.Abs(this.SlopeY) < Entity.EntityTolerance && Math.Abs(line.SlopeY) < Entity.EntityTolerance) { return true; }
            // One line is vertical while the other is not
            if (Math.Abs(this.SlopeX) < Entity.EntityTolerance || Math.Abs(line.SlopeX) < Entity.EntityTolerance) { return false; }
            return Math.Round(this.SlopeY / this.SlopeX, 4).Equals(Math.Round(line.SlopeY / line.SlopeX, 4));
        }

        private bool withinTolerance(double value, double target)
        {
            return ((value <= (target + TOLERANCE)) && (value >= (target - TOLERANCE)));
        }

        public bool isSameInfinateLine(Entity other)
        {
            if (other is Line)
            {
                Line lineOther = (Line)other;
                if (this.SlopeX > -0.00005 && this.SlopeX < 0.00005) // means this is a verticle line
                {
                    if (withinTolerance(lineOther.SlopeX, 0)) // means other is a verticle line
                    {
                        return ((withinTolerance(this.StartPoint.X,
                            lineOther.StartPoint.X))); // checks that the x values are within .00005 of each other
                    }
                    else
                    {
                        return false; // both have to be a verticle line
                    }
                }
                else if (withinTolerance(lineOther.SlopeX, 0))
                {
                    return false; // means other is a verticle line but this is not
                }

                double ThisYintercept = this.StartPoint.Y - ((this.SlopeY / this.SlopeX) * this.StartPoint.X);
                double OtherYintercept = lineOther.StartPoint.Y -
                                         ((lineOther.SlopeY / lineOther.SlopeX) * lineOther.StartPoint.X);
                if (withinTolerance(Math.Abs(this.SlopeY / this.SlopeX),
                        Math.Abs(lineOther.SlopeY / lineOther.SlopeX)) &&
                    withinTolerance(ThisYintercept, OtherYintercept))
                {
                    return true;
                }
            }

            return false;
        }

        public bool isPerpendicular(Entity other)
        {
            if (other is Line)
            {
                Line lineOther = (Line)other;
                // Vertical slope edge cases
                if (Math.Round(this.StartPoint.X, 4).Equals(Math.Round(this.EndPoint.X)) && Math.Round(lineOther.StartPoint.Y, 4).Equals(Math.Round(lineOther.EndPoint.Y, 4)))
                {
                    return true;
                }
                else if (Math.Round(this.StartPoint.Y, 4).Equals(Math.Round(this.EndPoint.Y, 4)) && Math.Round(lineOther.StartPoint.X, 4).Equals(Math.Round(lineOther.EndPoint.X, 4)))
                {
                    return true;
                }
                if ((this.SlopeY / this.SlopeX) == (-1 * (lineOther.SlopeX / lineOther.SlopeY)))
                {
                    return true;
                }
            }

            return false;
        }

        public double findDistance(Point point1, Point point2)
        {
            return Math.Sqrt((Math.Pow(point2.X - point1.X, 2)) + (Math.Pow(point2.Y - point1.Y, 2)));
        }

        public Point findPointToExtend(Line line, Point point)
        {
            if (findDistance(line.StartPoint, point) < findDistance(line.EndPoint, point))
            {
                return line.StartPoint;
            }
            else return line.EndPoint;
        }


        public override bool Equals(object? obj)
        {
            //If both lines have the same length , and the slopes are equal (within tight tollerance)
            if (obj is Line && Math.Abs(((Line)obj).Length - this.Length) < EntityTolerance)
            {
                double slopeDifY = Math.Abs(SlopeY - ((Line)obj).SlopeY);
                double slopeDifX = Math.Abs(SlopeX - ((Line)obj).SlopeX);

                if (Math.Abs(((Line)obj).Length - this.Length) < EntityTolerance
                    && slopeDifY < EntityTolerance
                    && slopeDifX < EntityTolerance
                    && this.hasPoint(((Line)obj).EndPoint)
                    && this.hasPoint(((Line)obj).StartPoint))
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public override bool Compare(object? obj)
        {
            //If both lines have the same length , and the slopes are equal (within tight tollerance)
            if (obj is Line && Math.Abs(((Line)obj).Length - this.Length) < EntityTolerance)
            {
                double slopeDifY = Math.Abs(SlopeY - ((Line)obj).SlopeY);
                double slopeDifX = Math.Abs(SlopeX - ((Line)obj).SlopeX);

                if (Math.Abs(((Line)obj).Length - this.Length) < EntityTolerance
                    && slopeDifY < EntityTolerance
                    && slopeDifX < EntityTolerance)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public override double MinX()
        {
            return Math.Min(StartPoint.X, EndPoint.X);
        }

        public override double MinY()
        {
            return Math.Min(StartPoint.Y, EndPoint.Y);
        }

        public override double MaxX()
        {
            return Math.Max(StartPoint.X, EndPoint.X);
        }

        public override double MaxY()
        {
            return Math.Max(StartPoint.Y, EndPoint.Y);
        }

        public Point GetDelta()
        {
            return new Point(EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y);
        }
        
        public override Line Transform(Matrix3 transform)
        {
            Point newStart = transform * StartPoint; 
            Point newEnd =  transform * EndPoint;
            return new Line(newStart, newEnd);
        }
    }
}