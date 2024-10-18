using FeatureRecognitionAPI.Models.Enums;
using Point = FeatureRecognitionAPI.Models.Point;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public double SlopeY { get; }
        public double SlopeX { get; }

        public  double Length { get; } 
        private Line()
        {
            entityType = PossibleEntityTypes.line;
        }

        public Line( double startX,  double startY,  double endX,  double endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            
            SlopeY = EndY- StartY;
            SlopeX = EndX - StartX;

            // Distance Calculation
            Length = Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2));
        }

        public bool isParallel(Entity other)
        {
            if (other is Line)
            {
                Line lineOther = (Line)other;
                double ThisYintercept = this.StartY - ((this.SlopeY / this.SlopeX) * this.StartX);
                double OtherYintercept = lineOther.StartY - ((lineOther.SlopeY / lineOther.SlopeX) * lineOther.StartX);
                if (((this.SlopeY/this.SlopeX) == (lineOther.SlopeY/lineOther.SlopeX)) && (ThisYintercept == OtherYintercept))
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
                if ((this.SlopeY / this.SlopeX) == (-1* (lineOther.SlopeX / lineOther.SlopeY)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool extend(Entity other)
        {
            if (other is Line && !this.DoesIntersect(other))
            {
                Line otherLine = (Line)other;
                if (isPerpendicular(other))
                {
                    Point intersectPoint = this.getIntersectPoint(this, (Line)other);
                    Point PointToExtendThis = findPointToExtend(this, intersectPoint);
                    Point PointToExtendOther = findPointToExtend(this, intersectPoint);
                    if (PointToExtendThis != null)
                    {
                        if (PointToExtendThis.X == this.StartX && PointToExtendThis.Y == this.StartY) {
                            //extend start point
                            this.StartX = intersectPoint.X;
                            this.StartY = intersectPoint.Y;
                        }
                        else
                        {
                            //extend end point
                            this.EndX = intersectPoint.X;
                            this.EndY = intersectPoint.Y;
                        }
                    }
                    if (PointToExtendOther != null)
                    {
                        if (PointToExtendOther.X == otherLine.StartX && PointToExtendThis.Y == otherLine.StartY)
                        {
                            //extend start point
                            otherLine.StartX = intersectPoint.X;
                            otherLine.StartY = intersectPoint.Y;
                        }
                        else
                        {
                            //extend end point
                            otherLine.EndX = intersectPoint.X;
                            otherLine.EndY = intersectPoint.Y;
                        }
                    }
                    //find point of intersection through entity method
                    // distance formula to this point for start and end points
                    // extend what is closer on this and other line
                }
                else if (isParallel(other))
                {
                    Point pointToExtend;
                    Point targetPoint;
                    if (findDistance(new Point(this.StartX, this.StartY), new Point (otherLine.StartX, otherLine.StartY)) < findDistance(new Point(this.EndX, this.EndY),new Point(otherLine.StartX, otherLine.StartY))) {
                        pointToExtend = new Point(this.StartX, this.StartY);
                    }
                    else
                    {
                        pointToExtend = new Point(this.EndX, this.EndY);
                    }
                    if (findDistance(pointToExtend, new Point(otherLine.StartX, otherLine.StartY)) > findDistance(pointToExtend, new Point(otherLine.EndX, otherLine.EndY)))
                    {
                        targetPoint = new Point(otherLine.StartX,otherLine.StartY);
                    }
                    else
                    {
                        targetPoint = new Point(otherLine.EndX, otherLine.EndY);
                    }
                    pointToExtend = targetPoint;
                    //////DOES NOT WORKKKKKKKKKKKK
                }
                other = otherLine;
            }
            return false;
        }

        public double findDistance(Point point1, Point point2)
        {
            return Math.Sqrt((Math.Pow(point2.X-point1.X, 2)) + (Math.Pow(point2.Y-point1.Y, 2)));
        }

        public Point findPointToExtend(Line line, Point point)
        {
            Point startPoint = new Point(StartX, StartY);
            Point endPoint = new Point(EndX, EndY);
            if (findDistance(startPoint, point) < findDistance(endPoint, point))
            {
                return startPoint;
            }
            else return endPoint;
        }
    }
}
