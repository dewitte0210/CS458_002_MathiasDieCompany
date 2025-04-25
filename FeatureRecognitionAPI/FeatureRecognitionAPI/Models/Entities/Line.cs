using System.Runtime.Versioning;
using System.Web;
using FeatureRecognitionAPI.Models.Utility;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        private const double TOLERANCE = 0.00005;

        public double SlopeY { get; set; }
        public double SlopeX { get; set; }

        // Don't Delete. Called from ExtendedLine constructor
        protected Line()
        {
            Start = new Point();
            End = new Point();
        }

        public Line(Line line)
        {
            Start = new Point(line.Start);
            End = new Point(line.End);
            SlopeY = line.SlopeY;
            SlopeX = line.SlopeX;
            Length = line.Length;
        }

        public Line(double startX, double startY, double endX, double endY)
        {
            Start = new Point(startX, startY);
            End = new Point(endX, endY);
            SlopeY = End.Y - Start.Y;
            SlopeX = End.X - Start.X;

            this.Length = Point.Distance(Start, End);
        }


        public Line(Point startPoint, Point endPoint)
        {
            Start = new Point(startPoint);
            End = new Point(endPoint);
            
            SlopeY = End.Y - Start.Y;
            SlopeX = End.X - Start.X;
            
            this.Length = Point.Distance(Start, End);
        }

        //constructor with extendedline parameter
        public Line(double startX, double startY, double endX, double endY, bool extendedLine)
        {
            Start = new Point(startX, startY);
            End = new Point(endX, endY);

            SlopeY = End.Y - Start.Y;
            SlopeX = End.X - Start.X;

            Length = Point.Distance(Start, End);
        }

        // todo: implement getLength()
        // public override double GetLength()
        // {
        //     Point delta = GetDelta();
        //     return double.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        // }

        public Line swapStartEnd()
        {
            return new Line(End.X, End.Y, Start.X, Start.Y);
        }

        public bool hasPoint(Point point)
        {
            return (Start.Equals(point) || End.Equals(point));
        }

        [Obsolete("Line isParallel is deprecated, please use Angles isParallel in Utility.")]
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

        private static bool withinTolerance(double value, double target)
        {
            return ((value <= (target + TOLERANCE)) && (value >= (target - TOLERANCE)));
        }

        public bool isSameInfiniteLine(Entity other)
        {
            if (other is Line lineOther)
            {
                if (this.SlopeX > -0.00005 && this.SlopeX < 0.00005) // means this is a verticle line
                {
                    if (withinTolerance(lineOther.SlopeX, 0)) // means other is a verticle line
                    {
                        return (withinTolerance(this.Start.X,
                            lineOther.Start.X)); // checks that the x values are within .00005 of each other
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

                double ThisYintercept = this.Start.Y - ((this.SlopeY / this.SlopeX) * this.Start.X);
                double OtherYintercept = lineOther.Start.Y -
                                         ((lineOther.SlopeY / lineOther.SlopeX) * lineOther.Start.X);
                if (withinTolerance(Math.Abs(this.SlopeY / this.SlopeX),
                        Math.Abs(lineOther.SlopeY / lineOther.SlopeX)) &&
                    withinTolerance(ThisYintercept, OtherYintercept))
                {
                    return true;
                }
            }

            return false;
        }

        [Obsolete("Line isPerpendicular is deprecated, please use Angles isPerpendicular in Utility.")]
        public bool isPerpendicular(Entity other)
        {
            if (other is Line lineOther)
            {
                // Vertical slope edge cases
                if (Math.Round(this.Start.X, 4).Equals(Math.Round(this.End.X, 4)) && Math.Round(lineOther.Start.Y, 4).Equals(Math.Round(lineOther.End.Y, 4)))
                {
                    return true;
                }
                else if (Math.Round(this.Start.Y, 4).Equals(Math.Round(this.End.Y, 4)) && Math.Round(lineOther.Start.X, 4).Equals(Math.Round(lineOther.End.X, 4)))
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

        public override bool Equals(object? obj)
        {
            //If both lines have the same length , and the slopes are equal (within tight tolerance)
            if (obj is Line && Math.Abs(((Line)obj).Length - this.Length) < EntityTolerance)
            {
                double slopeDifY = Math.Abs(SlopeY - ((Line)obj).SlopeY);
                double slopeDifX = Math.Abs(SlopeX - ((Line)obj).SlopeX);

                if (Math.Abs(((Line)obj).Length - this.Length) < EntityTolerance
                    && slopeDifY < EntityTolerance
                    && slopeDifX < EntityTolerance
                    && this.hasPoint(((Line)obj).End)
                    && this.hasPoint(((Line)obj).Start))
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
            return Math.Min(Start.X, End.X);
        }

        public override double MinY()
        {
            return Math.Min(Start.Y, End.Y);
        }

        public override double MaxX()
        {
            return Math.Max(Start.X, End.X);
        }

        public override double MaxY()
        {
            return Math.Max(Start.Y, End.Y);
        }

        public Point GetDelta()
        {
            return new Point(End.X - Start.X, End.Y - Start.Y);
        }
        
        public override Line Transform(Matrix3 transform)
        {
            Point newStart = transform * Start; 
            Point newEnd =  transform * End;
            return new Line(newStart, newEnd);
        }
    }
}