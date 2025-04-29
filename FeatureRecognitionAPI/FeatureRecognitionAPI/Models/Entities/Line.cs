using System.Runtime.Versioning;
using System.Web;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Utility;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        public double SlopeY { get; init; }
        public double SlopeX { get; init; }

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

        public bool isSameInfiniteLine(Entity other)
        {
            if (other is Line lineOther)
            {
                if (this.SlopeX > -0.00005 && this.SlopeX < 0.00005) // means this is a vertical line
                {
                    if (MdcMath.DoubleEquals(lineOther.SlopeX, 0)) // means other is a vertical line
                    {
                        // checks that the x values are within .00005 of each other
                        return (MdcMath.DoubleEquals(this.Start.X, lineOther.Start.X));
                    }
                    else
                    {
                        return false; // both have to be a vertical line
                    }
                }
                else if (MdcMath.DoubleEquals(lineOther.SlopeX, 0))
                {
                    return false; // means other is a vertical line but this is not
                }

                double ThisYintercept = this.Start.Y - ((this.SlopeY / this.SlopeX) * this.Start.X);
                double OtherYintercept = lineOther.Start.Y - ((lineOther.SlopeY / lineOther.SlopeX) * lineOther.Start.X);
                if (MdcMath.DoubleEquals(Math.Abs(this.SlopeY / this.SlopeX), 
                        Math.Abs(lineOther.SlopeY / lineOther.SlopeX)) 
                    && MdcMath.DoubleEquals(ThisYintercept, OtherYintercept))
                {
                    return true;
                }
            }

            return false;
        }

        public override double GetLength()
        {
            return Point.Distance(Start, End);
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