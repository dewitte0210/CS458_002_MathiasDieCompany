using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models { 
    public class Circle : Entity
    {
        //Center point of circle - x value
        public Point Center { get; set; }
        public  double radius { get; set; }
        public Circle( double centerX,  double centerY,  double radius)
        {
            entityType = PossibleEntityTypes.circle;
            Center = new Point(centerX, centerY);
            this.radius = radius;
            this.Length = (calcPerimeter(radius));
        }

        private  double calcPerimeter( double radius)
        {
            return 2 * Math.PI * radius;
        }
    }
}
