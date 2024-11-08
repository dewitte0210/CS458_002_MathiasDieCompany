using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public double SlopeY { get; }
        public double SlopeX { get; }
        public bool ExtendedLine { get; set; }



        private Line() { }
        public Line(bool ExtendedLine)
        {
            this.ExtendedLine = ExtendedLine;
            StartPoint = new Point();
            EndPoint = new Point();
        }

        public Line( double startX,  double startY,  double endX,  double endY)
        {
            StartPoint = new Point(startX, startY);
            EndPoint = new Point(endX, endY);
            ExtendedLine = false;

            SlopeY = endY - startY;
            SlopeX = endX - startX;

            // Distance Calculation
            this.Length = (Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2)));
        }

        //constructor with extendedline parameter
        public Line(double startX, double startY, double endX, double endY, bool extendedLine)
        {
            StartPoint = new Point(startX, startY);
            EndPoint = new Point(endX, endY);
            ExtendedLine = extendedLine;

            SlopeY = endY - startY;
            SlopeX = endX - startX;

            // Distance Calculation
            this.Length = (Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2)));
        }

        public bool isParallel(Entity other)
        {
            if (other is Line)
            {
                Line lineOther = (Line)other;
                double ThisYintercept = this.StartPoint.Y - ((this.SlopeY / this.SlopeX) * this.StartPoint.X);
                double OtherYintercept = lineOther.StartPoint.Y - ((lineOther.SlopeY / lineOther.SlopeX) * lineOther.StartPoint.X);
                if (((this.SlopeY / this.SlopeX) == (lineOther.SlopeY / lineOther.SlopeX)) && (ThisYintercept == OtherYintercept))
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
            //If both lines have the same length, and the slopes are within a tight tollerance, they are equal
            if (obj is Line)
            {
                double slopeDifY = Math.Abs(SlopeY - ((Line)obj).SlopeY);
                double slopeDifX = Math.Abs(SlopeX - ((Line)obj).SlopeX);

                if (((Line)obj).Length == Length
                    && slopeDifY < 0.000009
                    && slopeDifX < 0.000009)
                {
                    return true;
                }
                else return false;
            }
            else return false;


        }
    }
}
