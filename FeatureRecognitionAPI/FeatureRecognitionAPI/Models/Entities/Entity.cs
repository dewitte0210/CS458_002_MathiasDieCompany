/*
 * Abstract class meant to be inherrited by every Entity child
 * The info from a PDF and DWG will be parsed into Entities
 *  - Line, Circle, Arc
 */
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
        protected enum PossibleEntityTypes
        {
            //MAP TO ObjectName inside CadObjectCollection from .Entities._entries
            line,
            circle,
            arc
        }

        protected void setLength(double length)
        {
            this.length = length;
        }
        public double getLength() { return this.length; }

        public Entity()
        {

        }

        public string GetEntityType()
        {
            return entityType.ToString();
        }
        public bool DoesIntersect(Entity other)
        {
            if (this is Circle || other is Circle) { return false; }

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

        internal bool PointsAreTouching(Point p1, Point p2)
        {
            double xDiff = Math.Abs(p1.X - p2.X); 
            double yDiff = Math.Abs(p1.Y - p2.Y);
            if (xDiff < 0.0000009 && yDiff < 0.0000009)
            {
                return true;
            }
            return false; 
        }
        internal bool IntersectLineWithArc(Line line, Arc arc)
        {
            //Check if the enpoints are touching first to avoid the intersect calculations
            Point lStart = new(line.StartX, line.StartY);
            Point lEnd = new(line.EndX, line.EndY);
            Point aStart = new(arc.startX, arc.startY);
            Point aEnd = new(arc.endX, arc.endY);
            bool touching = PointsAreTouching(lStart, aStart) || PointsAreTouching(lStart, aEnd) || PointsAreTouching(lEnd, aStart) || PointsAreTouching(lEnd, aEnd);
            if (touching) { return true; }

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
                if (slope > 1000000 || slope < -1000000)
                {
                    slope = 0;
                }
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

        internal bool IntersectLineWithLine(Line line1, Line line2)
        {
            // If the endpoints are touching we can avoid the intersect math 
            Point l1Start = new(line1.StartX, line1.StartY);
            Point l1End = new(line1.EndX, line1.EndY);
            Point l2Start = new(line2.StartX, line2.StartY);
            Point l2End = new(line2.EndX, line2.EndY);
            bool touching = PointsAreTouching(l1Start, l2Start) || PointsAreTouching(l1Start, l2End) || PointsAreTouching(l1End, l2Start) || PointsAreTouching(l1End, l2End);
            if (touching) { return true; }
            
            //  Get lines in the form Ax + By + C = 0
            double A1;
            double B1;
            double C1;
            double slope1 = 0;
            double intercept1 = 0;
            bool vertical1 = false;

            //  This is to check for a vertical line, since it would crash the program
            //  trying to divide by 0
            if (line1.EndX == line1.StartX)
            {
                A1 = 1;
                B1 = 0;
                C1 = -1 * line1.EndX;
                vertical1 = true;
            }
            else
            {
                slope1 = (line1.EndY - line1.StartY) / (line1.EndX - line1.StartX);
                intercept1 = line1.EndY - (slope1 * line1.EndX);
                // The slope of the line ends up being A in the general form
                A1 = slope1;
                C1 = intercept1;
                B1 = -1;
                //  A cannot be negative in the general form
                if (A1 < 0)
                {
                    A1 *= -1;
                    B1 *= -1;
                    C1 *= -1;
                }
            }

            double A2;
            double B2;
            double C2;
            double slope2 = 0;
            double intercept2 = 0;
            bool vertical2 = false;

            //  This is to check for a vertical line, since it would crash the program
            //  trying to divide by 0
            if (line2.EndX == line2.StartX)
            {
                A2 = 1;
                B2 = 0;
                C2 = -1 * line2.EndX;
                vertical2 = true;
            }
            else
            {
                slope2 = (line2.EndY - line2.StartY) / (line2.EndX - line2.StartX);
                intercept2 = line2.EndY - (slope2 * line2.EndX);
                // The slope of the line ends up being A in the general form
                A2 = slope2;
                C2 = intercept2;
                B2 = -1;
                //  A cannot be negative in the general form
                if (A2 < 0)
                {
                    A2 *= -1;
                    B2 *= -1;
                    C2 *= -1;
                }
            }

            //  Lines are parallel -> non zero
            if (slope1 == slope2 && slope1 != 0) { return false; }
            //  Lines are parallel -> vertical
            else if (vertical1 && vertical2) { return false; }
            //  Lines are parallel -> horizontal
            else if (slope1 == slope2 && (!vertical1 && !vertical2)) { return false; }

            //  Calc intersection between lines
            double intersectX;
            double intersectY;
            //  line1 is vertical
            if (B1 == 0)
            {
                intersectX = line1.EndX;
                intersectY = ((-1 * A2 * intersectX) - C2) / B2;
            }
            //  line2 is vertical
            else if (B2 == 0)
            {
                intersectX = line2.EndX;
                intersectY = ((-1 * A1 * intersectX) - C1) / B1;
            }
            //  line1 is horizontal
            else if (slope1 == 0)
            {
                intersectY = line1.EndY;
                intersectX = ((-1 * B2 * intersectY) - C2) / A2;
            }
            //  line2 is horizontal
            else if (slope2 == 0)
            {
                intersectY = line2.EndY;
                intersectX = ((-1 * B1 * intersectY) - C1) / A1;
            }
            else
            {
                intersectX = ((-1 * C2) - ((B2 * -1 * C1) / B1)) / (A2 + ((B2 * -1 * A1) / B1));
                intersectY = ((-1 * A1) / B1 * intersectX) - (C1 / B1);
            }

            //  Check if the intersection is in bounds of both line segments
            bool line1InBoundsX = intersectX >= Math.Min(line1.StartX, line1.EndX) &&
                    intersectX <= Math.Max(line1.StartX, line1.EndX);

            bool line1InBoundsY = intersectY >= Math.Min(line1.StartY, line1.EndY) &&
                    intersectY <= Math.Max(line1.StartY, line1.EndY);

            bool line2InBoundsX = intersectX >= Math.Min(line2.StartX, line2.EndX) &&
                    intersectX <= Math.Max(line2.StartX, line2.EndX);

            bool line2InBoundsY = intersectY >= Math.Min(line2.StartY, line2.EndY) &&
                    intersectY <= Math.Max(line2.StartY, line2.EndY);

            return line1InBoundsX && line1InBoundsY && line2InBoundsX && line2InBoundsY;
        }

        internal bool IntersectArcWithArc(Arc arc1, Arc arc2)
        {
            
            // If the endpoints are touching we can avoid the intersect math 
            Point a1Start = new(arc1.startX, arc1.startY);
            Point a1End = new(arc1.endX, arc1.endY);
            Point a2Start = new(arc2.startX, arc2.startY);
            Point a2End = new(arc2.endX, arc2.endY);
            bool touching = PointsAreTouching(a1Start, a2Start) || PointsAreTouching(a1Start, a2End) || PointsAreTouching(a1End, a2Start) || PointsAreTouching(a1End, a2End);
            if (touching) { return true; }
            
            // Treat both Arcs circles, get the line between their centers
            Line between = new Line(arc1.centerX, arc1.centerY, arc2.centerX, arc2.centerY);
             
            // First case, the circles do not intersect as they are too far appart
            // Second case, one circle is entirely inside the other but not intersecting.
            if (between.length > (arc1.radius + arc2.radius) || 
                between.length < (Math.Abs(arc1.radius - arc2.radius)) ||
                between.length == 0) { return false; }

            // The circles intersect. Do they intersect at the position of the arcs?
            
            // Find a and h.
             double a = (Math.Pow(arc1.radius,2) - Math.Pow(arc2.radius, 2) + Math.Pow(between.length, 2)) / 
                (2 * between.length);
             double h = Math.Sqrt(Math.Pow(arc1.radius, 2) - Math.Pow(a,2));
            
            // Find P2.
             double cx2 = arc1.centerX + a * (arc2.centerX - arc1.centerX) / between.length;
             double cy2 = arc1.centerY + a * (arc2.centerY - arc1.centerY) / between.length;

            // Get the points P3.
             double intersect1X = (cx2 + h * (arc2.centerY - arc1.centerY) / between.length);
             double intersect1Y = (cy2 - h * (arc2.centerX - arc1.centerX) / between.length);
             double intersect2X = (cx2 - h * (arc2.centerY - arc1.centerY) / between.length);
             double intersect2Y = (cy2 + h * (arc2.centerX - arc1.centerX) / between.length);

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
