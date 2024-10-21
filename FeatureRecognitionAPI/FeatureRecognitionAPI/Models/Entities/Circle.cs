using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models { 
    public class Circle : Entity
    {
        //Center point of circle - x value
        public  double centerX {  get; set; }
        //Center point of circle - y value
        public  double centerY { get; set; }
        public  double radius { get; set; }
        public Circle( double centerX,  double centerY,  double radius)
        {
            entityType = PossibleEntityTypes.circle;
            this.centerX = centerX;
            this.centerY = centerY;
            this.radius = radius;
            this.setLength(calcPerimeter(radius));
        }

        private  double calcPerimeter( double radius)
        {
            return 2 * Math.PI * radius;
        }
    }
}
