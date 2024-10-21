using System;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using DecimalMath;

namespace FeatureRecognitionAPI.Models
{

    public class Arc : Entity
    {
        //Center point of arc - x value
        public  double centerX { get; set; }
        //Center point of arc - y value
        public  double centerY { get; set; }
        //Starting point of arc - x value
        public  double startX { get; }
        //Starting point of arc - y value
        public  double startY { get; }
        //Ending point of arc - x value
        public  double endX { get; }
        //Ending point of arc - y value
        public  double endY { get; }
        public  double radius { get; set; }
        //Start angle for starting coordinate values
        public  double startAngle { get; set; }
        //End angle for ending coordinate values
        public  double endAngle { get; set; }
        //Actual angle of the arc
        public  double centralAngle { get; }

        /**
         * Creates an arc and calculates the starting and ending coordinates as well
         * as the length of the arc
         */
        public Arc( double centerX,  double centerY,  double radius,  double startAngle,  double endAngle)
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
            this.centralAngle = calcCentralAngle(startAngle, endAngle);
            this.setLength(calcLength(radius, centralAngle));
        }

        /**
         * Function for calculating radians for cos and sin calculations.
         */
        private  double degreesToRadians( double degrees)
        {
            return (degrees * Math.PI / 180);
        }

        /**
         * Function to calculate the x coordinate given the center point, radius
         * and an angle.
         */
        private  double calcXCoord( double x,  double radius,  double angle)
        {
            return (radius * Math.Cos(degreesToRadians(angle)) + x);
        }

        /**
         * Function to calculate the y coordinate given the center point, radius
         * and an angle.
         */
        private  double calcYCoord( double y,  double radius,  double angle)
        {
            return (radius * Math.Sin(degreesToRadians(angle)) + y);
        }

        /**
         * Function to calculate the central angle
         */
        private  double calcCentralAngle( double startAngle,  double endAngle) 
        {
            //The subtraction result would be negative, need to add 360 to get correct value
            if (endAngle < startAngle)
                return endAngle - startAngle + 360;
            return endAngle - startAngle;
        }

        /**
         * Fucntion to calculate the length of the arc for perimeter length checks
         */
        private  double calcLength( double radius,  double centralAngle)
        {
            return (2 * Math.PI * radius * (centralAngle / 360));
        }
    }
}