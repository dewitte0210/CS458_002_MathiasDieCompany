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

        public override bool isParallel(Entity other)
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

        public override bool isPerpendicular(Entity other)
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

    }
}
