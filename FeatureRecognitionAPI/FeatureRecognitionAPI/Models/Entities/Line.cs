using System;
using System.IO;
using System.Numerics;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }

        public double Length { get; } 
        private Line()
        {
            entityType = PossibleEntityTypes.line;
        }

        public Line(double startX, double startY, double endX, double endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            
            // Distance Calculation
            Length = Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2));
        }
    }
}
