using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        public  double StartX { get; set; }
        public  double StartY { get; set; }
        public  double EndX { get; set; }
        public  double EndY { get; set; }
        public double SlopeY { get; }
        public double SlopeX { get; }
        public bool ExtendedLine { get; set; }
        private Line()
        {
            entityType = PossibleEntityTypes.line;
        }
        public Line(bool ExtendedLine)
        {
            this.ExtendedLine = ExtendedLine;
        }

        public Line( double startX,  double startY,  double endX,  double endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            ExtendedLine = false;

            SlopeY = EndY - StartY;
            SlopeX = EndX - StartX;

            // Distance Calculation
            this.setLength(Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2)));
        }

        //constructor with extendedline parameter
        public Line(double startX, double startY, double endX, double endY, bool extendedLine)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            ExtendedLine = extendedLine;

            SlopeY = EndY - StartY;
            SlopeX = EndX - StartX;

            // Distance Calculation
            this.setLength(Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2)));
        }

        public bool isParallel(Entity other)
        {
            if (other is Line)
            {
                Line lineOther = (Line)other;
                double ThisYintercept = this.StartY - ((this.SlopeY / this.SlopeX) * this.StartX);
                double OtherYintercept = lineOther.StartY - ((lineOther.SlopeY / lineOther.SlopeX) * lineOther.StartX);
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
