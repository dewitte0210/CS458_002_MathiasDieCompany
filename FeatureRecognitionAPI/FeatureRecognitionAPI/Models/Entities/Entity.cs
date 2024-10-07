/*
 * Abstract class meant to be inherrited by every Entity child
 * The info from a PDF and DWG will be parsed into Entities
 *  - Line, Circle, Arc
 */
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Line = FeatureRecognitionAPI.Models.Line;
using Arc = FeatureRecognitionAPI.Models.Arc;

namespace FeatureRecognitionAPI.Models
{
    public abstract class Entity {
        protected PossibleEntityTypes entityType;
        double length;
        protected enum PossibleEntityTypes
        {
            line,
            circle,
            arc
        }

        public void setLength(double length)
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
            return false;
        }
        
        private bool IntersectLineWithArc(Line line, Arc arc)
        {
            //  Get line in the slope-intercept form, then transform it to the
            //  general form: Ax + By + C = 0

            //  A, B, and C variables in the general form
            decimal a;
            decimal b;
            decimal c;
            //  Variable used for calculation of intersect points
            bool negative = false;

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
                // The slope of the line ends up being A in the general form
                a = (line.EndY - line.StartY) / (line.EndX - line.StartX);
                c = line.EndY - (a * line.EndX);
                b = 1;
                //  A cannot be negative in the general form
                if(a < 0)
                {
                    negative = true;
                    a *= -1;
                    b *= -1;
                    c *= -1;
                }
            }

            //  Checks if the line passes through or touches the circle the arc represents
            decimal numerator = a * arc.centerX + b * arc.centerY + c;
            if (numerator < 0)
                numerator *= -1;
            decimal distance = numerator / DecimalEx.Sqrt(DecimalEx.Pow(a, 2) + DecimalEx.Pow(b, 2));

            if (arc.radius >= distance)
            {

            }
            return false;
        }

        private bool IntersectLineWithLine(Line line1, Line line2)
        {
            // Get lines in the form Ax + By = C
            decimal A1 = line1.EndY - line1.StartY;
            decimal B1 = line1.EndX - line1.StartX;
            decimal C1 = A1 * line1.StartX + B1 * line1.StartY;

            decimal A2 = line2.EndY - line2.StartY;
            decimal B2 = line2.EndX - line2.StartX;
            decimal C2 = A2 * line2.StartX + B2 * line2.EndX;

            decimal det = A1 * B2 - A2 * B1;
            
            // Lines are parralell and thus cannot intersect
            if (det == 0) { return false; }
            
            // Intersection point
            decimal xIntersect = (B2 * C1 - B1 * C2) / det;
            decimal yIntersect = (B2 * C1 - B1 * C2) / det;
           
            // Check if the intersect lies on each of our line segments.
            bool xBounds = (xIntersect > line1.StartX && xIntersect < line1.EndX
                && xIntersect > line2.StartX && xIntersect < line2.EndX);
            bool yBounds = (yIntersect > line1.StartY && yIntersect < line1.EndY
                && yIntersect > line2.StartY && yIntersect < line2.EndY);

            return xBounds && yBounds;
        }

        private bool IntersectArcWithArc(Arc arc1, Arc arc2)
        {
            // Treat both Arcs circles, get the line between their centers
            Line between = new Line(arc1.centerX, arc1.centerY, arc2.centerX, arc2.centerY);
             
            // First case, the circles do not intersect as they are too far appart
            // Second case, one circle is entirely inside the other but not intersecting.
            if (between.Length > (arc1.radius + arc2.radius) || 
                between.Length < (arc1.radius - arc2.radius)) { return false; }
            
            // The circles intersect. Do they intersect at the position of the arcs?

            return false;
        }
    }
}
