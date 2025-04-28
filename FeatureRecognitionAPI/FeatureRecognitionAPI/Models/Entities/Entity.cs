using DecimalMath;
using System.Runtime.CompilerServices;
using FeatureRecognitionAPI.Models.Utility;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Testing_for_Project")]

namespace FeatureRecognitionAPI.Models
{
    /// <summary>
    /// Abstract class that represents a generic entity in dxf or dwg files
    /// </summary>
    public abstract class Entity
    {
        // todo: make length a get function because it should never change
        // without underlying properties changing
        public double Length { get; init; }//length of the entity
        
        public Point Start { get; set; }
        public Point End { get; set; }
        [JsonIgnore] public List<Entity> AdjList { get; set; }
        public const double EntityTolerance = 0.00005;
        public bool KissCut { get; set; }

        //Precision for x and y intersect values to
        //account for inaccurate calculated values
        private const int intersectTolerance = 4;

        //Enables the use of a default constructor
        protected Entity()
        {
            AdjList = new List<Entity>();
            KissCut = false;
        }
        
        // todo: implement getLength()
        public abstract double GetLength();
        
        /// <summary>
        /// Function that checks if this entity intersects with another entity
        /// </summary>
        /// <param name="other"> the entity being checked against this </param>
        /// <returns> true if they intersect, otherwise false </returns>
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

        /// <summary>
        /// Helper function that is called from DoesIntersect
        /// Specifically checks if two lines intersect
        /// </summary>
        /// <param name="line1"> the first Line being checked </param>
        /// <param name="line2"> the second Line being checked </param>
        /// <returns> true if they intersect, otherwise false </returns>
        internal static bool IntersectLineWithLine(Line line1, Line line2)
        {
            // If the endpoints are touching we can avoid the intersect math
            if (line1.AreEndpointsTouching(line2)) { return true; }

            //  Get lines in the form Ax + By + C = 0
            double A1;
            double B1;
            double C1;
            double slope1 = 0;
            double intercept1 = 0;
            bool vertical1 = false;

            //  This is to check for a vertical line, since it would crash the program
            //  trying to divide by 0
            if ((new Point(line1.Start.X, 0).Equals(new Point(line1.End.X, 0))))
            {
                A1 = 1;
                B1 = 0;
                C1 = -1 * line1.End.X;
                vertical1 = true;
            }
            else
            {
                slope1 = (line1.End.Y - line1.Start.Y) / (line1.End.X - line1.Start.X);
                intercept1 = line1.End.Y - (slope1 * line1.End.X);
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
            if ((new Point(line2.Start.X, 0).Equals(new Point(line2.End.X, 0))))
            {
                A2 = 1;
                B2 = 0;
                C2 = -1 * line2.End.X;
                vertical2 = true;
            }
            else
            {
                slope2 = (line2.End.Y - line2.Start.Y) / (line2.End.X - line2.Start.X);
                intercept2 = line2.End.Y - (slope2 * line2.End.X);
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
            if (vertical1 && vertical2) { return false; }
            //  Lines are parallel -> horizontal
            if (slope1 == slope2 && (!vertical1 && !vertical2)) { return false; }

            //  Calc intersection between lines
            double intersectX;
            double intersectY;
            //  line1 is vertical
            if (B1 == 0)
            {
                intersectX = line1.End.X;
                intersectY = ((-1 * A2 * intersectX) - C2) / B2;
            }
            //  line2 is vertical
            else if (B2 == 0)
            {
                intersectX = line2.End.X;
                intersectY = ((-1 * A1 * intersectX) - C1) / B1;
            }
            //  line1 is horizontal
            else if (slope1 == 0)
            {
                intersectY = line1.End.Y;
                intersectX = ((-1 * B2 * intersectY) - C2) / A2;
            }
            //  line2 is horizontal
            else if (slope2 == 0)
            {
                intersectY = line2.End.Y;
                intersectX = ((-1 * B1 * intersectY) - C1) / A1;
            }
            else
            {
                intersectX = ((-1 * C2) - ((B2 * -1 * C1) / B1)) / (A2 + ((B2 * -1 * A1) / B1));
                intersectY = ((-1 * A1) / B1 * intersectX) - (C1 / B1);
            }

            //  Check if the intersection is in bounds of both line segments
            bool line1InBoundsX = Math.Round(intersectX, intersectTolerance) >= Math.Min(Math.Round(line1.Start.X, intersectTolerance), Math.Round(line1.End.X, intersectTolerance)) &&
                    Math.Round(intersectX, intersectTolerance) <= Math.Max(Math.Round(line1.Start.X, intersectTolerance), Math.Round(line1.End.X, intersectTolerance));

            bool line1InBoundsY = Math.Round(intersectY, intersectTolerance) >= Math.Min(Math.Round(line1.Start.Y, intersectTolerance), Math.Round(line1.End.Y, intersectTolerance)) &&
                    Math.Round(intersectY, intersectTolerance) <= Math.Max(Math.Round(line1.Start.Y, intersectTolerance), Math.Round(line1.End.Y, intersectTolerance));

            bool line2InBoundsX = Math.Round(intersectX, intersectTolerance) >= Math.Min(Math.Round(line2.Start.X, intersectTolerance), Math.Round(line2.End.X, intersectTolerance)) &&
                    Math.Round(intersectX, intersectTolerance) <= Math.Max(Math.Round(line2.Start.X, intersectTolerance), Math.Round(line2.End.X, intersectTolerance));

            bool line2InBoundsY = Math.Round(intersectY, intersectTolerance) >= Math.Min(Math.Round(line2.Start.Y, intersectTolerance), Math.Round(line2.End.Y, intersectTolerance)) &&
                    Math.Round(intersectY, intersectTolerance) <= Math.Max(Math.Round(line2.Start.Y, intersectTolerance), Math.Round(line2.End.Y, intersectTolerance));

            return line1InBoundsX && line1InBoundsY && line2InBoundsX && line2InBoundsY;
        }

        /// <summary>
        /// Helper function that is called from DoesIntersect
        /// Specifically checks if a line and arc intersects
        /// </summary>
        /// <param name="line"> the Line being checked </param>
        /// <param name="arc"> the arc being checked </param>
        /// <returns> true if they intersect, otherwise false </returns>
        internal static bool IntersectLineWithArc(Line line, Arc arc)
        {
            //Check if the endpoints are touching first to avoid the intersect calculations
            if (line.AreEndpointsTouching(arc))
            {
                return true;
            }

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
            if (line.End.X == line.Start.X)
            {
                a = 1;
                b = 0;
                c = -1 * line.End.X;
            }
            else
            {
                slope = (line.End.Y - line.Start.Y) / (line.End.X - line.Start.X);
                if (slope > 1000000 || slope < -1000000)
                {
                    slope = 0;
                }

                intercept = line.End.Y - (slope * line.End.X);
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
            //  Will hold the solution values of the quadratic equation
            List<double> solns = new();

            //  Special case for vertical line
            if (line.End.X == line.Start.X)
            {
                decimal[] tempSolns = DecimalEx.SolveQuadratic(1, (decimal)(-2 * arc.Center.Y),
                    (decimal)(Math.Pow(arc.Center.Y, 2) + Math.Pow((line.End.X - arc.Center.X), 2) -
                              Math.Pow(arc.Radius, 2)));

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
                    double x = line.End.X;
                    if (arc.IsInArcRange(new Point(x, y)) && Math.Min(line.Start.X, line.End.X) <= x &&
                        Math.Min(line.Start.Y, line.End.Y) <= y &&
                        Math.Max(line.Start.X, line.End.X) >= x &&
                        Math.Max(line.Start.Y, line.End.Y) >= y)
                    {
                        return true;
                    }
                }
            }
            else
            {
                decimal[] tempSolns = DecimalEx.SolveQuadratic((decimal)(Math.Pow(slope, 2) + 1),
                    (decimal)(-2.0 * arc.Center.X) + (decimal)(2 * (intercept * slope)) -
                    (decimal)(2 * (arc.Center.Y * slope)),
                    (decimal)Math.Pow(arc.Center.X, 2) + (decimal)Math.Pow(intercept, 2) -
                    (decimal)(2 * (intercept * arc.Center.Y)) + (decimal)Math.Pow(arc.Center.Y, 2) -
                    (decimal)Math.Pow(arc.Radius, 2));
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

                    if (arc.IsInArcRange(new Point(x, y)) &&
                        Math.Min(Math.Round(line.Start.X, intersectTolerance),
                            Math.Round(line.End.X, intersectTolerance)) <= x &&
                        Math.Min(Math.Round(line.Start.Y, intersectTolerance),
                            Math.Round(line.End.Y, intersectTolerance)) <= y &&
                        Math.Max(Math.Round(line.Start.X, intersectTolerance),
                            Math.Round(line.End.X, intersectTolerance)) >= x &&
                        Math.Max(Math.Round(line.Start.Y, intersectTolerance),
                            Math.Round(line.End.Y, intersectTolerance)) >= y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Helper function that is called from DoesIntersect
        /// Specifically checks if two arcs intersects
        /// </summary>
        /// <param name="arc1"> the first Arc being checked </param>
        /// <param name="arc2"> the second Arc being checked </param>
        /// <returns> true if they intersect, otherwise false </returns>
        internal static bool IntersectArcWithArc(Arc arc1, Arc arc2)
        {
            // If the endpoints are touching we can avoid the intersect math 
            if (arc1.AreEndpointsTouching(arc2)) { return true; }

            // Treat both Arcs circles, get the line between their centers
            Line between = new Line(arc1.Center.X, arc1.Center.Y, arc2.Center.X, arc2.Center.Y);

            // First case, the circles do not intersect as they are too far apart
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
        }

        internal static bool IntersectLineWithEllipse(Line line, Ellipse ellipse)
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
            }
            if (rotation > 0)
            {
                Point start = line.Start;
                Point end = line.End;

                //Translate the line to the origin
                start.X = start.X - ellipse.Center.X;
                start.Y = start.Y - ellipse.Center.Y;
                end.X = end.X - ellipse.Center.X;
                end.Y = end.Y - ellipse.Center.Y;

                //Rotate around the origin
                double temp = start.X;
                start.X = -1 * ((start.X * Math.Cos(rotation)) - (start.Y * Math.Sin(rotation)));
                start.Y = -1 * ((start.Y * Math.Cos(rotation)) + (temp * Math.Sin(rotation)));
                temp = end.X;
                end.X = -1 * ((end.X * Math.Cos(rotation)) - (end.Y * Math.Sin(rotation)));
                end.Y = -1 * ((end.Y * Math.Cos(rotation)) + (temp * Math.Sin(rotation)));

                //Translate back
                start.X = start.X + ellipse.Center.X;
                start.Y = start.Y + ellipse.Center.Y;
                end.X = end.X + ellipse.Center.X;
                end.Y = end.Y + ellipse.Center.Y;

                line = new Line(start.X, start.Y, end.X, end.Y);
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
            if ((new Point(line.Start.X, 0).Equals(new Point(line.End.X, 0))))
            {
                Al = 1;
                Bl = 0;
                Cl = -1 * (line.End.X - ellipse.Center.X);
                isVertical = true;
            }
            else
            {
                slopel = (line.End.Y - line.Start.Y) / (line.End.X - line.Start.X);
                interceptl = (line.End.Y - ellipse.Center.Y) - (slopel * (line.End.X - ellipse.Center.X));
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

            double major = Point.Distance(ellipse.MajorAxisEndPoint, ellipse.Center);
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
                List<double> xSolns = MdcMath.QuadraticFormula(a, b, c);
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
                        && Math.Min(Math.Round(line.Start.X, intersectTolerance), Math.Round(line.End.X, intersectTolerance)) <= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Min(Math.Round(line.Start.Y, intersectTolerance), Math.Round(line.End.Y, intersectTolerance)) <= (SolnCoords[i].Y + ellipse.Center.Y)
                        && Math.Max(Math.Round(line.Start.X, intersectTolerance), Math.Round(line.End.X, intersectTolerance)) >= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Max(Math.Round(line.Start.Y, intersectTolerance), Math.Round(line.End.Y, intersectTolerance)) >= (SolnCoords[i].Y + ellipse.Center.Y)) { return true; }
                }
            }
            return false;
        }

        // todo: remove redundant get touching functions and move to entityTools
        
        /// <summary>
        /// Function to check if any points of this entity is touching any points of another entity
        /// </summary>
        /// <param name="e2"> the entity being checked against this </param>
        /// <returns> true if they have points touch, otherwise false </returns>
        internal bool AreEndpointsTouching(Entity e2)
        {
            if (this is Circle || e2 is Circle)
            {
                return false;
            }

            return Start.Equals(e2.Start) ||
                   Start.Equals(e2.End) ||
                   End.Equals(e2.Start) ||
                   End.Equals(e2.End);
        }

        // todo: move to entityTools
        
        // todo: move to MDCMath
        
        public abstract override bool Equals(object? obj);

        /// <returns> Return true when entities compared have similar traits,
        /// length is the same (but start and end point, or mid point can vary) </returns>
        public abstract bool Compare(object? obj);

        public abstract double MinX();
        public abstract double MinY();
        public abstract double MaxX();
        public abstract double MaxY();

        public abstract Entity Transform(Matrix3 transform);
    }
}
