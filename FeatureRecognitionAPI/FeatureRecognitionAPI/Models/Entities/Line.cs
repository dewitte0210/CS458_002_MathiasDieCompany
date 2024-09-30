using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        public decimal StartX { get; set; }
        public decimal StartY { get; set; }
        public decimal EndX { get; set; }
        public decimal EndY { get; set; }

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

            // Distance Calculation
            Length = DecimalEx.Sqrt(DecimalEx.Pow(endX - startX, 2) + DecimalEx.Pow(endY - startY, 2));
        }
    }
}
