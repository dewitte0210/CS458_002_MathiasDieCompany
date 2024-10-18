/*
 * Abstract class meant to be inherrited by every Entity child
 * The info from a PDF and DWG will be parsed into Entities
 *  - Line, Circle, Arc
 */
using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using DecimalMath;
using Line = FeatureRecognitionAPI.Models.Line;
using Arc = FeatureRecognitionAPI.Models.Arc;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Testing_for_Project")]

namespace FeatureRecognitionAPI.Models
{

    public abstract class Entity {
        protected PossibleEntityTypes entityType;
        double length;

        public void setLength(double length)
        {
            this.length = length;
        }
        public double getLength() { return this.length; }

        public Entity()
        {

        }

        public PossibleEntityTypes GetEntityType()
        {
            return entityType;
        }
        public bool DoesIntersect(Entity other)
        {
            if (this.entityType == PossibleEntityTypes.circle) { return false; }

            if (this.entityType == PossibleEntityTypes.arc)
            {
                if (other.entityType == PossibleEntityTypes.line)
                {
                    return IntersectLineWithArc((Line)other,(Arc)this);
                }
                else
                {
                    return IntersectArcWithArc((Arc)this, (Arc)other);
                }
            }
            else
            {
                if (other.entityType == PossibleEntityTypes.line)
                {
                    return IntersectLineWithLine((Line)this, (Line)other);
                }
                else
                {
                    return IntersectLineWithArc((Line)this, (Arc)other);
                }
            }
        }
        
        internal bool IntersectLineWithArc(Line line, Arc arc)
        {
            //  Get line in the slope-intercept form, then transform it to the
            //  general form: Ax + By + C = 0

            //  A, B, and C variables in the general form
             double a;
             double b;
             double c;
            //  Slope and intercept of the line, used in quadratic calc
             double slope = 0;
             double intercept = 0;

            //  This is to check for a vertical line, since it would crash the program
            //  trying to divide by 0
            if (line.EndX == line.StartX)
            {
                a = 1;
                b = 0;
                c = -1 * line.EndX;
            }
            else
            {
                slope = (line.EndY - line.StartY) / (line.EndX - line.StartX);
                intercept = line.EndY - (slope * line.EndX);
                // The slope of the line ends up being A in the general form
                a = slope;
                c = intercept;
                b = -1;
                //  A cannot be negative in the general form
                if(a < 0)
                {
                    a *= -1;
                    b *= -1;
                    c *= -1;
                }
            }

            //  Checks if the line passes through or touches the circle the arc represents
             double numerator = Math.Abs(a * arc.centerX + b * arc.centerY + c);
             double distance = numerator / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            if (arc.radius >= distance)
            {
                //  Will hold the solution values of the quadratic equation
                List<double> solns = new();

                //  Special case for vertical line
                if (line.EndX == line.StartX)
                {
                    decimal[] tempSolns = DecimalEx.SolveQuadratic(1, (decimal)(-2 * arc.centerY), (decimal)(Math.Pow(arc.centerY, 2) + Math.Pow((line.EndX - arc.centerX), 2) - Math.Pow(arc.radius, 2)));

                    foreach(decimal number in tempSolns)
                    {
                        solns.Add((double)number);
                    }
                    //  Checks if each solution is on the arc, if one is on it return true
                    for (int i = 0; i < solns.Count(); i++)
                    {
                        //  Solution y value
                         double y = solns[i];
                        //  Solution x value
                         double x = line.EndX;
                        if (IsInArcRange(arc.centerX, arc.centerY, x, y, arc.startAngle, arc.endAngle)) { return true; }
                    }
                }
                else
                {
                    decimal[] tempSolns = DecimalEx.SolveQuadratic((decimal)(Math.Pow(slope, 2) + 1), (decimal)(-2.0 * arc.centerX) + (decimal)(2 * (intercept * slope)) - (decimal)(2 * (arc.centerY * slope)), (decimal)Math.Pow(arc.centerX, 2) + (decimal)Math.Pow(intercept, 2) - (decimal)(2 * (intercept * arc.centerY)) + (decimal)Math.Pow(arc.centerY, 2) - (decimal)Math.Pow(arc.radius, 2));
                    foreach(decimal number in tempSolns)
                    {
                        solns.Add((double)number);
                    }
                    
                    //  Checks if each solution is on the arc, if one is on it return true
                    for (int i = 0; i < solns.Count; i++)
                    {
                        //  Solution x value
                         double x = solns[i];
                        //  Solution y value
                         double y = slope * solns[i] + intercept;
                        if (IsInArcRange(arc.centerX, arc.centerY, x, y, arc.startAngle, arc.endAngle)) { return true; }
                    }
                }
            }
            return false;
        }

        public Point getIntersectPoint(Line line1, Line line2)
        {
            Point intersectPoint = new Point();
            double A1 = line1.EndY - line1.StartY;
            double B1 = line1.EndX - line1.StartX;
            double C1 = A1 * line1.StartX + B1 * line1.StartY;

            double A2 = line2.EndY - line2.StartY;
            double B2 = line2.EndX - line2.StartX;
            double C2 = A2 * line2.StartX + B2 * line2.StartY;

            double delta = A1 * B2 - A2 * B1;

            // Lines are parralell and thus cannot intersect
            intersectPoint.intersect = (delta == 0);

            // Intersection point
            intersectPoint.setPoint((B1 * C2 - B2 * C1) / delta, (A1 * C2 - A2 * C1) / delta);
            return intersectPoint;
        }

        internal bool IntersectLineWithLine(Line line1, Line line2)
        {
//make into intercetion method
//return point variable
            // Get lines in the form Ax + By = C
             double A1 = line1.EndY - line1.StartY;
             double B1 = line1.EndX - line1.StartX;
             double C1 = A1 * line1.StartX + B1 * line1.StartY;

             double A2 = line2.EndY - line2.StartY;
             double B2 = line2.EndX - line2.StartX;
             double C2 = A2 * line2.StartX + B2 * line2.StartY;

             double delta = A1 * B2 - A2 * B1;
            
            // Lines are parralell and thus cannot intersect
            if (delta == 0) { return false; }
            
            // Intersection point

            Point intersectPoint = getIntersectPoint(line1, line2);
            if (!intersectPoint.intersect)
            {
                //Delta is 0 and cannot intersect
                return false;
            }
            double xIntersect = intersectPoint.X;
            double yIntersect = intersectPoint.Y;

            // Check if the intersect lies on each of our line segments.
            bool xBounds = (xIntersect > Math.Min(line1.StartX, line1.EndX) && xIntersect < Math.Max(line1.StartX, line1.EndX)) &&
                (xIntersect > Math.Min(line2.StartX, line2.EndX) && xIntersect < Math.Max(line2.StartX, line2.EndX));
            bool yBounds = (yIntersect > Math.Min(line1.StartY, line1.EndY) && yIntersect < Math.Max(line1.StartY, line1.EndY)) &&
                (yIntersect > Math.Min(line2.StartY, line2.EndY) && yIntersect < Math.Max(line2.StartY, line2.EndY));

            return xBounds && yBounds;
        }

        internal bool IntersectArcWithArc(Arc arc1, Arc arc2)
        {
            // Treat both Arcs circles, get the line between their centers
            Line between = new Line(arc1.centerX, arc1.centerY, arc2.centerX, arc2.centerY);
             
            // First case, the circles do not intersect as they are too far appart
            // Second case, one circle is entirely inside the other but not intersecting.
            if (between.Length > (arc1.radius + arc2.radius) || 
                between.Length < (Math.Abs(arc1.radius - arc2.radius)) ||
                between.Length == 0) { return false; }

            // The circles intersect. Do they intersect at the position of the arcs?
            
            // Find a and h.
             double a = (Math.Pow(arc1.radius,2) - Math.Pow(arc2.radius, 2) + Math.Pow(between.Length, 2)) / 
                (2 * between.Length);
             double h = Math.Sqrt(Math.Pow(arc1.radius, 2) - Math.Pow(a,2));
            
            // Find P2.
             double cx2 = arc1.centerX + a * (arc2.centerX - arc1.centerX) / between.Length;
             double cy2 = arc1.centerY + a * (arc2.centerY - arc1.centerY) / between.Length;

            // Get the points P3.
             double intersect1X = (cx2 + h * (arc2.centerY - arc1.centerY) / between.Length);
             double intersect1Y = (cy2 - h * (arc2.centerX - arc1.centerX) / between.Length);
             double intersect2X = (cx2 - h * (arc2.centerY - arc1.centerY) / between.Length);
             double intersect2Y = (cy2 + h * (arc2.centerX - arc1.centerX) / between.Length);

            bool intersect1IsValid = IsInArcRange(arc1.centerX, arc1.centerY, intersect1X, intersect1Y, arc1.startAngle, arc1.endAngle) &&
                   IsInArcRange(arc2.centerX, arc2.centerY, intersect1X, intersect1Y, arc2.startAngle, arc2.endAngle);
            bool intersect2IsValid = IsInArcRange(arc1.centerX, arc1.centerY, intersect2X, intersect2Y, arc1.startAngle, arc1.endAngle) &&
                   IsInArcRange(arc2.centerX, arc2.centerY, intersect2X, intersect2Y, arc2.startAngle, arc2.endAngle);
            
            return intersect1IsValid || intersect2IsValid; 
;
        }

        internal bool IsInArcRange( double circleX,  double circleY,  double pointX,  double pointY,
             double startAngle,  double endAngle)
        {
             double y = pointY - circleY;
             double x = pointX - circleX;
             double degrees;
            
            // Figure out the angle the point is in. Special cases apply at x=0 and y=0
            if(x == 0)
            {
                degrees = y > 0 ? 90 : 270;    
            }
            else if(y == 0)
            {
                degrees = x > 0 ? 0 : 180;
            }
            else
            {
                 double tan = Math.Atan2(y,x);
                degrees = tan * (180 / Math.PI);
                //Q2 and Q3
                if(x < 0)
                {
                    // y < 0? Q3 else Q2
                    degrees =  y < 0 ? degrees += 360 : Math.Abs(degrees + 180);
                }
                //Q4
                else if (x > 0 && y < 0)
                {
                    degrees = 360 + degrees;
                }
            }

            // rotate start and end angles to start at 0
             double difference = 360 - startAngle;
             double adjustedStart = 0;
             double adjustedEnd = endAngle + difference;
             double adjustedDegrees = degrees + difference;
            
            if(adjustedEnd >= 360) { adjustedEnd -= 360; }
            if(adjustedDegrees >= 360) { adjustedDegrees -= 360; }

            return adjustedDegrees >= adjustedStart && adjustedDegrees <= adjustedEnd; 
        }
    }
}
