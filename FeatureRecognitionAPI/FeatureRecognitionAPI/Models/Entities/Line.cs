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
        public decimal EndY { get; set; }
        public double SlopeY { get; }
        public double SlopeX { get; }

        public decimal Length { get; } 
        private Line()
        {
            entityType = PossibleEntityTypes.line;
        }

        public Line(decimal startX, decimal startY, decimal endX, decimal endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            
            SlopeY = EndY- StartY;
            SlopeX = EndX - StartX;

            // Distance Calculation
            Length = DecimalEx.Sqrt(DecimalEx.Pow(endX - startX, 2) + DecimalEx.Pow(endY - startY, 2));
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
