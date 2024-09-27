using System;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
namespace FeatureRecognitionAPI.Models
{

    public class Arc : Entity
    {
        public double centerX { get; set; }
        public double centerY { get; set; }
        public double startX { get; set; }
        public double startY { get; set; }
        public double endX { get; set; }
        public double endY { get; set; }
        public double radius { get; set; }
        public double startAngle { get; set; }
        public double endAngle { get; set; }
        public Arc(double centerX, double centerY, double radius, double startAngle, double endAngle)
        {
            entityType = PossibleEntityTypes.arc;
            this.centerX = centerX;
            this.centerY = centerY;
            this.radius = radius;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.startX = calcXCoord(centerX, radius, startAngle);
            this.startY = calcYCoord(centerY, radius, startAngle);
            this.endX = calcXCoord(centerX, radius, endAngle);
            this.endY = calcYCoord(centerY, radius, endAngle);
        }

        private double degreesToRadians(double degrees)
        {
            return (degrees * Math.PI / 180);
        }

        private double calcXCoord(double x, double radius, double angle)
        {
            return (radius * Math.Cos(degreesToRadians(angle)) + x);
        }

        private double calcYCoord(double y, double radius, double angle)
        {
            return (radius * Math.Sin(degreesToRadians(angle)) + y);
        }
    }
}