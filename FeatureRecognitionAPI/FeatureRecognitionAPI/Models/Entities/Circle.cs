using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models { 
    public class Circle : Entity
    {
        //Center point of circle - x value
        public decimal centerX {  get; set; }
        //Center point of circle - y value
        public decimal centerY { get; set; }
        public decimal radius { get; set; }
        public decimal perimeter { get; }
        public Circle(decimal centerX, decimal centerY, decimal radius)
        {
            entityType = PossibleEntityTypes.circle;
            this.centerX = centerX;
            this.centerY = centerY;
            this.radius = radius;
            this.perimeter = calcPerimeter(radius);
        }

        private decimal calcPerimeter(decimal radius)
        {
            return 2 * DecimalEx.Pi * radius;
        }
    }
}
