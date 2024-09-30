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
        public decimal centerX { get; set; }
        //Center point of arc - y value
        public decimal centerY { get; set; }
        //Starting point of arc - x value
        public decimal startX { get; }
        //Starting point of arc - y value
        public decimal startY { get; }
        //Ending point of arc - x value
        public decimal endX { get; }
        //Ending point of arc - y value
        public decimal endY { get; }
        public decimal radius { get; set; }
        //Start angle for starting coordinate values
        public decimal startAngle { get; set; }
        //End angle for ending coordinate values
        public decimal endAngle { get; set; }
        //Actual angle of the arc
        public decimal centralAngle { get; }
        //Length of the arc
        public decimal length { get; }

        /**
         * Creates an arc and calculates the starting and ending coordinates as well
         * as the length of the arc
         */
        public Arc(decimal centerX, decimal centerY, decimal radius, decimal startAngle, decimal endAngle)
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
            this.length = calcLength(radius, centralAngle);
        }

        /**
         * Function for calculating radians for cos and sin calculations.
         */
        private decimal degreesToRadians(decimal degrees)
        {
            return (degrees * DecimalEx.Pi / 180);
        }

        /**
         * Function to calculate the x coordinate given the center point, radius
         * and an angle.
         */
        private decimal calcXCoord(decimal x, decimal radius, decimal angle)
        {
            return (radius * DecimalEx.Cos(degreesToRadians(angle)) + x);
        }

        /**
         * Function to calculate the y coordinate given the center point, radius
         * and an angle.
         */
        private decimal calcYCoord(decimal y, decimal radius, decimal angle)
        {
            return (radius * DecimalEx.Sin(degreesToRadians(angle)) + y);
        }

        /**
         * Function to calculate the central angle
         */
        private decimal calcCentralAngle(decimal startAngle, decimal endAngle) 
        {
            //The subtraction result would be negative, need to add 360 to get correct value
            if (endAngle < startAngle)
                return endAngle - startAngle + 360;
            return endAngle - startAngle;
        }

        /**
         * Fucntion to calculate the length of the arc for perimeter length checks
         */
        private decimal calcLength(decimal radius, decimal centralAngle)
        {
            return (2 * DecimalEx.Pi * radius * (centralAngle / 360));
        }
    }
}