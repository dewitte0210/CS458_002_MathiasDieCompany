using DecimalMath;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Testing_for_Project")]

namespace FeatureRecognitionAPI.Models
{
    /**
     * Abstract class that represents a generic entity in dxf or dwg files 
     */
    public abstract class Entity
    {
        public double Length { get; set; }//length of the entity
        public List<Entity> adjList { get; set; }
        public const double EntityTolerance = 0.00005;

        //Enables the use of a default constructor
        public Entity()
        {
            adjList = new List<Entity>();
            
        }

        private const int intersectTolerance = 4;//Precision for x and y intersect values to
                                                 //account for inaccurate calculated values

        /**
         * Function that checks if this entity intersects with another entity
         * 
         * @param other is the entity being checked against this
         * @return true if they intersect, otherwise false
         */
        public bool DoesIntersect(Entity other)
        {
            if (this is Circle || other is Circle) { return false; }

            if (this is Arc)
            {
                if (other is Line)
                {
                    return IntersectLineWithArc((Line)other, (Arc)this);
                }
                else
                {
                    return IntersectArcWithArc((Arc)this, (Arc)other);
                }
            }
            else if (this is Ellipse)
            {
                if (other is Line)
                {
                    return IntersectLineWithEllipse((Line)other, (Ellipse)this);
                }
            }
            else if (this is Line)
            {
                if (other is Line)
                {
                    return IntersectLineWithLine((Line)this, (Line)other);
                }
                else if (other is Ellipse)
                {
                    return IntersectLineWithEllipse((Line)this, (Ellipse)other);
                }
                else
                {
                    return IntersectLineWithArc((Line)this, (Arc)other);
                }
            }
            return false;
        }

        /**
         * Helper function that is called from DoesIntersect
         * Specifically checks if two lines intersect
         * 
         * @param line1 is the firt Line being checked
         * @param line2 is the second Line being checked
         * @return true if they intersect, otherwise false
         */
        internal bool IntersectLineWithLine(Line line1, Line line2)
        {
            // If the endpoints are touching we can avoid the intersect math
            bool touching = line1.StartPoint.Equals(line2.StartPoint) || line1.StartPoint.Equals(line2.EndPoint) || line1.EndPoint.Equals(line2.StartPoint) || line1.EndPoint.Equals(line2.EndPoint);
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
            if ((new Point(line1.StartPoint.X, 0).Equals(new Point(line1.EndPoint.X, 0))))
            {
                A1 = 1;
                B1 = 0;
                C1 = -1 * line1.EndPoint.X;
                vertical1 = true;
            }
            else
            {
                slope1 = (line1.EndPoint.Y - line1.StartPoint.Y) / (line1.EndPoint.X - line1.StartPoint.X);
                intercept1 = line1.EndPoint.Y - (slope1 * line1.EndPoint.X);
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
            if ((new Point(line2.StartPoint.X, 0).Equals(new Point(line2.EndPoint.X, 0))))
            {
                A2 = 1;
                B2 = 0;
                C2 = -1 * line2.EndPoint.X;
                vertical2 = true;
            }
            else
            {
                slope2 = (line2.EndPoint.Y - line2.StartPoint.Y) / (line2.EndPoint.X - line2.StartPoint.X);
                intercept2 = line2.EndPoint.Y - (slope2 * line2.EndPoint.X);
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
                intersectX = line1.EndPoint.X;
                intersectY = ((-1 * A2 * intersectX) - C2) / B2;
            }
            //  line2 is vertical
            else if (B2 == 0)
            {
                intersectX = line2.EndPoint.X;
                intersectY = ((-1 * A1 * intersectX) - C1) / B1;
            }
            //  line1 is horizontal
            else if (slope1 == 0)
            {
                intersectY = line1.EndPoint.Y;
                intersectX = ((-1 * B2 * intersectY) - C2) / A2;
            }
            //  line2 is horizontal
            else if (slope2 == 0)
            {
                intersectY = line2.EndPoint.Y;
                intersectX = ((-1 * B1 * intersectY) - C1) / A1;
            }
            else
            {
                intersectX = ((-1 * C2) - ((B2 * -1 * C1) / B1)) / (A2 + ((B2 * -1 * A1) / B1));
                intersectY = ((-1 * A1) / B1 * intersectX) - (C1 / B1);
            }

            //  Check if the intersection is in bounds of both line segments
            bool line1InBoundsX = Math.Round(intersectX, intersectTolerance) >= Math.Min(Math.Round(line1.StartPoint.X, intersectTolerance), Math.Round(line1.EndPoint.X, intersectTolerance)) &&
                    Math.Round(intersectX, intersectTolerance) <= Math.Max(Math.Round(line1.StartPoint.X, intersectTolerance), Math.Round(line1.EndPoint.X, intersectTolerance));

            bool line1InBoundsY = Math.Round(intersectY, intersectTolerance) >= Math.Min(Math.Round(line1.StartPoint.Y, intersectTolerance), Math.Round(line1.EndPoint.Y, intersectTolerance)) &&
                    Math.Round(intersectY, intersectTolerance) <= Math.Max(Math.Round(line1.StartPoint.Y, intersectTolerance), Math.Round(line1.EndPoint.Y, intersectTolerance));

            bool line2InBoundsX = Math.Round(intersectX, intersectTolerance) >= Math.Min(Math.Round(line2.StartPoint.X, intersectTolerance), Math.Round(line2.EndPoint.X, intersectTolerance)) &&
                    Math.Round(intersectX, intersectTolerance) <= Math.Max(Math.Round(line2.StartPoint.X, intersectTolerance), Math.Round(line2.EndPoint.X, intersectTolerance));

            bool line2InBoundsY = Math.Round(intersectY, intersectTolerance) >= Math.Min(Math.Round(line2.StartPoint.Y, intersectTolerance), Math.Round(line2.EndPoint.Y, intersectTolerance)) &&
                    Math.Round(intersectY, intersectTolerance) <= Math.Max(Math.Round(line2.StartPoint.Y, intersectTolerance), Math.Round(line2.EndPoint.Y, intersectTolerance));

            return line1InBoundsX && line1InBoundsY && line2InBoundsX && line2InBoundsY;
        }

        /**
         * Helper function that is called from DoesIntersect
         * Specifically checks if a line and arc intersects
         * 
         * @param line is the Line being checked
         * @param arc is the arc being checked
         * @return true if they intersect, otherwise false
         */
        internal bool IntersectLineWithArc(Line line, Arc arc)
        {
            //Check if the endpoints are touching first to avoid the intersect calculations
            Point aStart = new(arc.Start.X, arc.Start.Y);
            Point aEnd = new(arc.End.X, arc.End.Y);
            bool touching = line.StartPoint.Equals(aStart) || line.StartPoint.Equals(aEnd) || line.EndPoint.Equals(aStart) || line.EndPoint.Equals(aEnd);
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
            if (line.EndPoint.X == line.StartPoint.X)
            {
                a = 1;
                b = 0;
                c = -1 * line.EndPoint.X;
            }
            else
            {
                slope = (line.EndPoint.Y - line.StartPoint.Y) / (line.EndPoint.X - line.StartPoint.X);
                if (slope > 1000000 || slope < -1000000)
                {
                    slope = 0;
                }
                intercept = line.EndPoint.Y - (slope * line.EndPoint.X);
                // The slope of the line ends up being A in the general form
                a = slope;
                c = intercept;
                b = -1;
                //  A cannot be negative in the general form
                if (a < 0)
                {
                    a *= -1;
                    b *= -1;
                    c *= -1;
                }
            }

            //  Checks if the line passes through or touches the circle the arc represents
            double numerator = Math.Abs(a * arc.Center.X + b * arc.Center.Y + c);
            double distance = numerator / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            /*if (arc.Radius >= distance)
            {*/
                //  Will hold the solution values of the quadratic equation
                List<double> solns = new();

                //  Special case for vertical line
                if (line.EndPoint.X == line.StartPoint.X)
                {
                    decimal[] tempSolns = DecimalEx.SolveQuadratic(1, (decimal)(-2 * arc.Center.Y), (decimal)(Math.Pow(arc.Center.Y, 2) + Math.Pow((line.EndPoint.X - arc.Center.X), 2) - Math.Pow(arc.Radius, 2)));

                    foreach (decimal number in tempSolns)
                    {
                        solns.Add((double)number);
                    }
                    //  Checks if each solution is on the arc, if one is on it return true
                    for (int i = 0; i < solns.Count(); i++)
                    {
                        //  Solution y value
                        double y = solns[i];
                        //  Solution x value
                        double x = line.EndPoint.X;
                        if (arc.IsInArcRange(new Point(x, y)) && Math.Min(line.StartPoint.X, line.EndPoint.X) <= x && Math.Min(line.StartPoint.Y, line.EndPoint.Y) <= y && Math.Max(line.StartPoint.X, line.EndPoint.X) >= x && Math.Max(line.StartPoint.Y, line.EndPoint.Y) >= y) { return true; }
                    }
                }
                else
                {
                    decimal[] tempSolns = DecimalEx.SolveQuadratic((decimal)(Math.Pow(slope, 2) + 1), (decimal)(-2.0 * arc.Center.X) + (decimal)(2 * (intercept * slope)) - (decimal)(2 * (arc.Center.Y * slope)), (decimal)Math.Pow(arc.Center.X, 2) + (decimal)Math.Pow(intercept, 2) - (decimal)(2 * (intercept * arc.Center.Y)) + (decimal)Math.Pow(arc.Center.Y, 2) - (decimal)Math.Pow(arc.Radius, 2));
                    foreach (decimal number in tempSolns)
                    {
                        solns.Add((double)number);
                    }

                    //  Checks if each solution is on the arc, if one is on it return true
                    for (int i = 0; i < solns.Count; i++)
                    {
                        //Solution x value
                        double x = solns[i];
                        //Solution y value
                        double y = ((-1 * a) * solns[i] - c) / b;

                        if (arc.IsInArcRange(new Point(x, y)) && Math.Min(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) <= x && Math.Min(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) <= y && Math.Max(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) >= x && Math.Max(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) >= y) { return true; };
                    }
                }
            //}
            return false;
        }

        /**
         * Helper function that is called from DoesIntersect
         * Specifically checks if two arcs intersects
         * 
         * @param arc1 is the first Arc being checked
         * @param arc2 is the second Arc being checked
         * @return true if they intersect, otherwise false
         */
        internal bool IntersectArcWithArc(Arc arc1, Arc arc2)
        {

            // If the endpoints are touching we can avoid the intersect math 
            Point a1Start = new(arc1.Start.X, arc1.Start.Y);
            Point a1End = new(arc1.End.X, arc1.End.Y);
            Point a2Start = new(arc2.Start.X, arc2.Start.Y);
            Point a2End = new(arc2.End.X, arc2.End.Y);
            bool touching = a1Start.Equals(a2Start) || a1Start.Equals(a2End) || a1End.Equals(a2Start) || a1End.Equals(a2End);
            if (touching) { return true; }

            // Treat both Arcs circles, get the line between their centers
            Line between = new Line(arc1.Center.X, arc1.Center.Y, arc2.Center.X, arc2.Center.Y);

            // First case, the circles do not intersect as they are too far appart
            // Second case, one circle is entirely inside the other but not intersecting.
            if (between.Length > (arc1.Radius + arc2.Radius) ||
                between.Length < (Math.Abs(arc1.Radius - arc2.Radius)) ||
                between.Length == 0) { return false; }

            // The circles intersect. Do they intersect at the position of the arcs?

            // Find a and h.
            double a = (Math.Pow(arc1.Radius, 2) - Math.Pow(arc2.Radius, 2) + Math.Pow(between.Length, 2)) / (2 * between.Length);
            double h = Math.Sqrt(Math.Pow(arc1.Radius, 2) - Math.Pow(a, 2));

            // Find P2.
            double cx2 = arc1.Center.X + a * (arc2.Center.X - arc1.Center.X) / between.Length;
            double cy2 = arc1.Center.Y + a * (arc2.Center.Y - arc1.Center.Y) / between.Length;

            // Get the points P3.
            double intersect1X = (cx2 + h * (arc2.Center.Y - arc1.Center.Y) / between.Length);
            double intersect1Y = (cy2 - h * (arc2.Center.X - arc1.Center.X) / between.Length);
            double intersect2X = (cx2 - h * (arc2.Center.Y - arc1.Center.Y) / between.Length);
            double intersect2Y = (cy2 + h * (arc2.Center.X - arc1.Center.X) / between.Length);

            bool intersect1IsValid = arc1.IsInArcRange(new Point(intersect1X, intersect1Y)) &&
                   arc2.IsInArcRange(new Point(intersect1X, intersect1Y));
            bool intersect2IsValid = arc1.IsInArcRange(new Point(intersect2X, intersect2Y)) &&
                   arc2.IsInArcRange(new Point(intersect2X, intersect2Y));

            return intersect1IsValid || intersect2IsValid;
            ;
        }

        internal bool IntersectLineWithEllipse(Line line, Ellipse ellipse)
        {
            //Need to rotate the line around the origin for rotated ellipses
            double x = ellipse.MajorAxisEndPoint.X - ellipse.Center.X;
            double y = ellipse.MajorAxisEndPoint.Y - ellipse.Center.Y;
            double rotation;
            if (x == 0)
            {
                rotation = y > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
            }
            else if (y == 0)
            {
                rotation = x > 0 ? 0 : Math.PI;
            }
            else
            {
                rotation = Math.Atan2(y, x);
                //Q2 and Q3
                if (x < 0)
                {
                    rotation += Math.PI;
                }
                //Q4
                else if (x > 0 && y < 0)
                {
                    rotation += 2 * Math.PI;
                }
            }
            if (rotation > 0)
            {
                line = new Line(-1 * ((line.StartPoint.X * Math.Cos(rotation)) - (line.StartPoint.Y * Math.Sin(rotation))), -1 * ((line.StartPoint.Y * Math.Cos(rotation)) + (line.StartPoint.X * Math.Sin(rotation))), -1 * ((line.EndPoint.X * Math.Cos(rotation)) - (line.EndPoint.Y * Math.Sin(rotation))), -1 * ((line.EndPoint.Y * Math.Cos(rotation)) + (line.EndPoint.X * Math.Sin(rotation))));
            }
            //  Get line in the form Ax + By + C = 0 and moved so that ellipse center is the origin
            double Al;
            double Bl;
            double Cl;
            double slopel = 0;
            double interceptl = 0;
            bool isVertical = false;
            //  This is to check for a vertical line, since it would crash the program
            //  trying to divide by 0
            if ((new Point(line.StartPoint.X, 0).Equals(new Point(line.EndPoint.X, 0))))
            {
                Al = 1;
                Bl = 0;
                Cl = -1 * (line.EndPoint.X - ellipse.Center.X);
                isVertical = true;
            }
            else
            {
                slopel = (line.EndPoint.Y - line.StartPoint.Y) / (line.EndPoint.X - line.StartPoint.X);
                interceptl = (line.EndPoint.Y - ellipse.Center.Y) - (slopel * (line.EndPoint.X - ellipse.Center.X));
                // The slope of the line ends up being A in the general form
                Al = slopel;
                Cl = interceptl;
                Bl = -1;
                //  A cannot be negative in the general form
                if (Al < 0)
                {
                    Al *= -1;
                    Bl *= -1;
                    Cl *= -1;
                }
            }

            double major = Math.Sqrt(Math.Pow(ellipse.MajorAxisEndPoint.X - ellipse.Center.X, 2) + Math.Pow(ellipse.MajorAxisEndPoint.Y - ellipse.Center.Y, 2));
            double minor = major * ellipse.MinorToMajorAxisRatio;
            //List of solutions from equations
            List<Point> SolnCoords = new List<Point>();
            //Vertical line case
            if (isVertical && Cl <= Math.Round(major, intersectTolerance))
            {
                SolnCoords.Add(new Point(-1 * Cl, minor * Math.Sqrt(1 - (Math.Pow(Cl, 2) / Math.Pow(major, 2)))));
                if (Cl < Math.Round(major, intersectTolerance))
                {
                    SolnCoords.Add(new Point(-1 * Cl, -1 * (minor * Math.Sqrt(1 - (Math.Pow(Cl, 2) / Math.Pow(major, 2))))));
                }
            }
            else
            {
                double a = Math.Pow(Al, 2) + ((Math.Pow(Bl, 2) * Math.Pow(minor, 2)) / Math.Pow(major, 2));
                double b = -2 * Al * Cl;
                double c = Math.Pow(Cl, 2) - (Math.Pow(Bl, 2) * Math.Pow(minor, 2));
                //List of x value solns
                List<double> xSolns = QuadraticFormula(a, b, c);
                bool firstSoln = false;
                for (int i = 0; i < xSolns.Count; i++)
                {
                    double yValue;
                    if (isVertical)
                    {
                        if (!firstSoln)
                        {
                            yValue = Math.Sqrt(Math.Pow(minor, 2) - ((Math.Pow(xSolns[i], 2) / Math.Pow(major, 2)) * Math.Pow(minor, 2)));
                            SolnCoords.Add(new Point(xSolns[i], yValue));
                            firstSoln = true;
                        }
                        else
                        {
                            yValue = -1 * (Math.Sqrt(Math.Pow(minor, 2) - ((Math.Pow(xSolns[i], 2) / Math.Pow(major, 2)) * Math.Pow(minor, 2))));
                            SolnCoords.Add(new Point(xSolns[i], yValue));
                        }
                    }
                    else
                    {
                        yValue = (((-1 * Al) * xSolns[i]) - Cl) / Bl;
                        SolnCoords.Add(new Point(xSolns[i], yValue));
                    }
                }
            }

            if (SolnCoords.Count > 0)
            {
                for (int i = 0; i < SolnCoords.Count; i++)
                {
                    if (ellipse.isInEllipseRange(new Point(SolnCoords[i].X + ellipse.Center.X, SolnCoords[i].Y + ellipse.Center.Y))
                        && Math.Min(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) <= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Min(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) <= (SolnCoords[i].Y + ellipse.Center.Y)
                        && Math.Max(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) >= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Max(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) >= (SolnCoords[i].Y + ellipse.Center.Y)) { return true; }
                }
            }
            return false;
        }

        /**
         * Function to check if any points of this entity is touching any points of another entity
         * 
         * @param e2 is the entity being checked against this
         * @return true if they have points touch, otherwise false
         */
        internal bool EntityPointsAreTouching(Entity e2)
        {
            if (this is Circle || e2 is Circle)
            {
                return false;
            }
            if (this is Line)
            {
                if (e2 is Line)
                {
                    if (((Line)this).StartPoint.Equals(((Line)e2).StartPoint))
                    {
                        return true;
                    }
                    if (((Line)this).StartPoint.Equals(((Line)e2).EndPoint))
                    {
                        return true;
                    }
                    if (((Line)this).EndPoint.Equals(((Line)e2).StartPoint))
                    {
                        return true;
                    }
                    if (((Line)this).EndPoint.Equals(((Line)e2).EndPoint))
                    {
                        return true;
                    }
                    return false;
                }
                else if (e2 is Arc)
                {
                    if (((Line)this).StartPoint.Equals(((Arc)e2).Start))
                    {
                        return true;
                    }
                    if (((Line)this).StartPoint.Equals(((Arc)e2).End))
                    {
                        return true;
                    }
                    if (((Line)this).EndPoint.Equals(((Arc)e2).Start))
                    {
                        return true;
                    }
                    if (((Line)this).EndPoint.Equals(((Arc)e2).End))
                    {
                        return true;
                    }
                    return false;
                }
            }
            else if (this is Arc)
            {
                if (e2 is Line)
                {
                    if (((Arc)this).Start.Equals(((Line)e2).StartPoint))
                    {
                        return true;
                    }
                    if (((Arc)this).Start.Equals(((Line)e2).EndPoint))
                    {
                        return true;
                    }
                    if (((Arc)this).End.Equals(((Line)e2).StartPoint))
                    {
                        return true;
                    }
                    if (((Arc)this).End.Equals(((Line)e2).EndPoint))
                    {
                        return true;
                    }
                    return false;
                }
                else if (e2 is Arc)
                {
                    if (((Arc)this).Start.Equals(((Arc)e2).Start))
                    {
                        return true;
                    }
                    if (((Arc)this).Start.Equals(((Arc)e2).End))
                    {
                        return true;
                    }
                    if (((Arc)this).End.Equals(((Arc)e2).Start))
                    {
                        return true;
                    }
                    if (((Arc)this).End.Equals(((Arc)e2).End))
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        /**
         * Function that finds at what point two lines intersect when they are treated as infinite
         * this is mostly for the perpendicular line check, which is commented out
         * 
         * @param line1 is the first line being checked
         * @param line2 is the second line being checked
         * @return the point that line1 and line2 intersects. The points intersect field will be false if they are parallel
         */
        public Point getIntersectPoint(Line line1, Line line2)
        {
            Point intersectPoint = new Point();
            double A1 = line1.EndPoint.Y - line1.StartPoint.Y;
            double B1 = line1.EndPoint.X - line1.StartPoint.X;
            double C1 = A1 * line1.StartPoint.X + B1 * line1.StartPoint.Y;

            double A2 = line2.EndPoint.Y - line2.StartPoint.Y;
            double B2 = line2.EndPoint.X - line2.StartPoint.X;
            double C2 = A2 * line2.StartPoint.X + B2 * line2.StartPoint.Y;

            double delta = A1 * B2 - A2 * B1;

            // Lines are parralell and thus cannot intersect
            intersectPoint.intersect = !(delta == 0);

            // Intersection point
            intersectPoint.setPoint(((B1 * C2 - B2 * C1) / delta), ((A1 * C2 - A2 * C1) / delta));
            return intersectPoint;
        }

        internal Point getIntersectPoint(Line line, Arc arc)
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
            if (line.EndPoint.X == line.StartPoint.X)
            {
                a = 1;
                b = 0;
                c = -1 * line.EndPoint.X;
            }
            else
            {
                slope = (line.EndPoint.Y - line.StartPoint.Y) / (line.EndPoint.X - line.StartPoint.X);
                if (slope > 1000000 || slope < -1000000)
                {
                    slope = 0;
                }
                intercept = line.EndPoint.Y - (slope * line.EndPoint.X);
                // The slope of the line ends up being A in the general form
                a = slope;
                c = intercept;
                b = -1;
                //  A cannot be negative in the general form
                if (a < 0)
                {
                    a *= -1;
                    b *= -1;
                    c *= -1;
                }
            }

            //  Checks if the line passes through or touches the circle the arc represents
            double numerator = Math.Abs(a * arc.Center.X + b * arc.Center.Y + c);
            double distance = numerator / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            if (arc.Radius >= distance)
            {
                //  Will hold the solution values of the quadratic equation
                List<double> solns = new();

                //  Special case for vertical line
                if (line.EndPoint.X == line.StartPoint.X)
                {
                    decimal[] tempSolns = DecimalEx.SolveQuadratic(1, (decimal)(-2 * arc.Center.Y), (decimal)(Math.Pow(arc.Center.Y, 2) + Math.Pow((line.EndPoint.X - arc.Center.X), 2) - Math.Pow(arc.Radius, 2)));

                    foreach (decimal number in tempSolns)
                    {
                        solns.Add((double)number);
                    }
                    //  Checks if each solution is on the arc, if one is on it return true
                    for (int i = 0; i < solns.Count(); i++)
                    {
                        //  Solution y value
                        double y = solns[i];
                        //  Solution x value
                        double x = line.EndPoint.X;
                        if (arc.IsInArcRange(new Point(x, y)) && Math.Min(line.StartPoint.X, line.EndPoint.X) <= x && Math.Min(line.StartPoint.Y, line.EndPoint.Y) <= y && Math.Max(line.StartPoint.X, line.EndPoint.X) >= x && Math.Max(line.StartPoint.Y, line.EndPoint.Y) >= y) { return new Point(x, y); }
                    }
                }
                else
                {
                    decimal[] tempSolns = DecimalEx.SolveQuadratic((decimal)(Math.Pow(slope, 2) + 1), (decimal)(-2.0 * arc.Center.X) + (decimal)(2 * (intercept * slope)) - (decimal)(2 * (arc.Center.Y * slope)), (decimal)Math.Pow(arc.Center.X, 2) + (decimal)Math.Pow(intercept, 2) - (decimal)(2 * (intercept * arc.Center.Y)) + (decimal)Math.Pow(arc.Center.Y, 2) - (decimal)Math.Pow(arc.Radius, 2));
                    foreach (decimal number in tempSolns)
                    {
                        solns.Add((double)number);
                    }

                    //  Checks if each solution is on the arc, if one is on it return true
                    for (int i = 0; i < solns.Count; i++)
                    {
                        //Solution x value
                        double x = solns[i];
                        //Solution y value
                        double y = slope * solns[i] + intercept;

                        if (arc.IsInArcRange(new Point(x, y)) && Math.Min(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) <= x && Math.Min(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) <= y && Math.Max(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) >= x && Math.Max(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) >= y) { return new Point(x, y); };
                    }
                }
            }
            return null;
        }

        internal Point getIntersectPoint(Line line, Ellipse ellipse)
        {
            //Need to rotate the line around the origin for rotated ellipses
            double x = ellipse.MajorAxisEndPoint.X - ellipse.Center.X;
            double y = ellipse.MajorAxisEndPoint.Y - ellipse.Center.Y;
            double rotation;
            if (x == 0)
            {
                rotation = y > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
            }
            else if (y == 0)
            {
                rotation = x > 0 ? 0 : Math.PI;
            }
            else
            {
                rotation = Math.Atan2(y, x);
                //Q2 and Q3
                if (x < 0)
                {
                    rotation += Math.PI;
                }
                //Q4
                else if (x > 0 && y < 0)
                {
                    rotation += 2 * Math.PI;
                }
            }
            if (rotation > 0)
            {
                line = new Line(-1 * ((line.StartPoint.X * Math.Cos(rotation)) - (line.StartPoint.Y * Math.Sin(rotation))), -1 * ((line.StartPoint.Y * Math.Cos(rotation)) + (line.StartPoint.X * Math.Sin(rotation))), -1 * ((line.EndPoint.X * Math.Cos(rotation)) - (line.EndPoint.Y * Math.Sin(rotation))), -1 * ((line.EndPoint.Y * Math.Cos(rotation)) + (line.EndPoint.X * Math.Sin(rotation))));
            }
            //  Get line in the form Ax + By + C = 0 and moved so that ellipse center is the origin
            double Al;
            double Bl;
            double Cl;
            double slopel = 0;
            double interceptl = 0;
            bool isVertical = false;
            //  This is to check for a vertical line, since it would crash the program
            //  trying to divide by 0
            if ((new Point(line.StartPoint.X, 0).Equals(new Point(line.EndPoint.X, 0))))
            {
                Al = 1;
                Bl = 0;
                Cl = -1 * (line.EndPoint.X - ellipse.Center.X);
                isVertical = true;
            }
            else
            {
                slopel = (line.EndPoint.Y - line.StartPoint.Y) / (line.EndPoint.X - line.StartPoint.X);
                interceptl = (line.EndPoint.Y - ellipse.Center.Y) - (slopel * (line.EndPoint.X - ellipse.Center.X));
                // The slope of the line ends up being A in the general form
                Al = slopel;
                Cl = interceptl;
                Bl = -1;
                //  A cannot be negative in the general form
                if (Al < 0)
                {
                    Al *= -1;
                    Bl *= -1;
                    Cl *= -1;
                }
            }

            double major = Math.Sqrt(Math.Pow(ellipse.MajorAxisEndPoint.X - ellipse.Center.X, 2) + Math.Pow(ellipse.MajorAxisEndPoint.Y - ellipse.Center.Y, 2));
            double minor = major * ellipse.MinorToMajorAxisRatio;
            //List of solutions from equations
            List<Point> SolnCoords = new List<Point>();
            //Vertical line case
            if (isVertical && Cl <= Math.Round(major, intersectTolerance))
            {
                SolnCoords.Add(new Point(-1 * Cl, minor * Math.Sqrt(1 - (Math.Pow(Cl, 2) / Math.Pow(major, 2)))));
                if (Cl < Math.Round(major, intersectTolerance))
                {
                    SolnCoords.Add(new Point(-1 * Cl, -1 * (minor * Math.Sqrt(1 - (Math.Pow(Cl, 2) / Math.Pow(major, 2))))));
                }
            }
            else
            {
                double a = Math.Pow(Al, 2) + ((Math.Pow(Bl, 2) * Math.Pow(minor, 2)) / Math.Pow(major, 2));
                double b = -2 * Al * Cl;
                double c = Math.Pow(Cl, 2) - (Math.Pow(Bl, 2) * Math.Pow(minor, 2));
                //List of x value solns
                List<double> xSolns = QuadraticFormula(a, b, c);
                bool firstSoln = false;
                for (int i = 0; i < xSolns.Count; i++)
                {
                    double yValue;
                    if (isVertical)
                    {
                        if (!firstSoln)
                        {
                            yValue = Math.Sqrt(Math.Pow(minor, 2) - ((Math.Pow(xSolns[i], 2) / Math.Pow(major, 2)) * Math.Pow(minor, 2)));
                            SolnCoords.Add(new Point(xSolns[i], yValue));
                            firstSoln = true;
                        }
                        else
                        {
                            yValue = -1 * (Math.Sqrt(Math.Pow(minor, 2) - ((Math.Pow(xSolns[i], 2) / Math.Pow(major, 2)) * Math.Pow(minor, 2))));
                            SolnCoords.Add(new Point(xSolns[i], yValue));
                        }
                    }
                    else
                    {
                        yValue = (((-1 * Al) * xSolns[i]) - Cl) / Bl;
                        SolnCoords.Add(new Point(xSolns[i], yValue));
                    }
                }
            }

            if (SolnCoords.Count > 0)
            {
                for (int i = 0; i < SolnCoords.Count; i++)
                {
                    if (ellipse.isInEllipseRange(new Point(SolnCoords[i].X + ellipse.Center.X, SolnCoords[i].Y + ellipse.Center.Y))
                        && Math.Min(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) <= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Min(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) <= (SolnCoords[i].Y + ellipse.Center.Y)
                        && Math.Max(Math.Round(line.StartPoint.X, intersectTolerance), Math.Round(line.EndPoint.X, intersectTolerance)) >= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Max(Math.Round(line.StartPoint.Y, intersectTolerance), Math.Round(line.EndPoint.Y, intersectTolerance)) >= (SolnCoords[i].Y + ellipse.Center.Y)) { return new Point(SolnCoords[i].X + ellipse.Center.X, SolnCoords[i].Y + ellipse.Center.Y); }
                }
            }
            return null;
        }

        /**
         * Solves the quadratic formula
         * 
         * @Return - List of solutions
         */
        internal List<double> QuadraticFormula(double a, double b, double c)
        {
            List<double> solns = new List<double>();
            if (a == 0) { return solns; }
            double insideSqrt = Math.Pow(b, 2) - (4 * a * c);
            //Two real solutions
            if (insideSqrt > 0)
            {
                solns.Add(((-1 * b) + Math.Sqrt(insideSqrt)) / (2 * a));
                solns.Add(((-1 * b) - Math.Sqrt(insideSqrt)) / (2 * a));
            }
            //One real solution
            else if (insideSqrt == 0)
            {
                solns.Add((-1 * b) / (2 * a));
            }
            return solns;

        }
        public abstract override bool Equals(object? obj);

        //Return true when entities compared have similar traits, length is the same (but start and end point, or mid point can vary)
        public abstract bool Compare(object? obj);

        public abstract double MinX();
        public abstract double MinY();
        public abstract double MaxX();
        public abstract double MaxY();
    }
}
