using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;
using ACadSharp.Entities;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
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
            Point point1 = new Point(startX, startY);
            Point point2 = new Point(endX, endY);
            double xDiff = point1.X - point2.X;
            double yDiff = point1.Y - point2.Y;
            if (xDiff > Entity.EntityTolerance)
            {
                StartPoint = point1;
                EndPoint = point2;
            }
            //if the difference falls between +/- entityTolerance it is equals
            else if (Entity.EntityTolerance > xDiff && xDiff > (0 - Entity.EntityTolerance))
            {
                if (yDiff > Entity.EntityTolerance)
                {
                    StartPoint = point1;
                    EndPoint = point2;
                }
                else
                {
                    StartPoint = point2;
                    EndPoint = point1;
                }

            }
            else
            {
                StartPoint = point2;
                EndPoint = point1;
            }
            SlopeY = EndPoint.Y - StartPoint.Y;
            SlopeX = EndPoint.X - StartPoint.X;

            // Distance Calculation
            this.Length = (Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2)));
        }

        //constructor with extendedline parameter
        public Line(double startX, double startY, double endX, double endY, bool extendedLine)
        {
            StartPoint = new Point(startX, startY);
            EndPoint = new Point(endX, endY);

            SlopeY = EndPoint.Y - StartPoint.Y;
            SlopeX = EndPoint.X - StartPoint.X;

            // Distance Calculation
            this.Length = (Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2) + Math.Pow(EndPoint.Y - StartPoint.Y, 2)));
        }

        public bool hasPoint(Point point)
        {
            return (StartPoint.Equals(point) || EndPoint.Equals(point));
        }

        public bool isParallel(Line line)
        {
            double xSlopeDiff = this.SlopeX - line.SlopeX;
            double ySlopeDiff = this.SlopeY - line.SlopeY;
            if(xSlopeDiff < Entity.EntityTolerance && ySlopeDiff < Entity.EntityTolerance)
            {
                return true;
            }
            return false;
        }
        public bool isSameInfinateLine(Entity other)
        {
            if (other is Line)
            {
                Line lineOther = (Line)other;
                if (this.SlopeX > -0.00005 && this.SlopeX < 0.00005) // means this is a verticle line
                {
                    if (lineOther.SlopeX > -0.00005 && lineOther.SlopeX < 0.00005) // means other is a verticle line
                    {
                        return ((this.StartPoint.X >  (lineOther.StartPoint.X - 0.00005)) && (this.StartPoint.X < (lineOther.StartPoint.X + 0.00005))); // checks that the x values are within .00005 of each other
                    }
                    else
                    {
                        return false; // both have to be a verticle line
                    }

                }
                else if (lineOther.SlopeX > -0.00005 && lineOther.SlopeX < 0.00005)
                {
                    return false; // means other is a verticle line but this is not
                }

                double ThisYintercept = this.StartPoint.Y - ((this.SlopeY / this.SlopeX) * this.StartPoint.X);
                double OtherYintercept = lineOther.StartPoint.Y - ((lineOther.SlopeY / lineOther.SlopeX) * lineOther.StartPoint.X);
                if ((Math.Abs((this.SlopeY / this.SlopeX)) == Math.Abs((lineOther.SlopeY / lineOther.SlopeX))) && (ThisYintercept == OtherYintercept))
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
            if (obj is Line && Math.Abs( ((Line)obj).Length - this.Length ) < EntityTolerance ) 
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
    }
}
