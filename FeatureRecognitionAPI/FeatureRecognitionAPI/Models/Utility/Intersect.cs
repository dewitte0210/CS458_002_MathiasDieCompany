using DecimalMath;
using FeatureRecognitionAPI.Models.Entities;
using static FeatureRecognitionAPI.Models.Utility.Angles;
using static FeatureRecognitionAPI.Models.Utility.MdcMath;

namespace FeatureRecognitionAPI.Models.Utility;

public static class Intersect
{
    /// <summary>
    /// Function to check if any points of this entity is touching any points of another entity
    /// </summary>
    /// <param name="e1"> first entity </param>
    /// <param name="e2"> second entity </param>
    /// <returns> true if they have points touch, otherwise false </returns>
    internal static bool AreEndpointsTouching(Entity? e1, Entity? e2)
    {
        if (e1 is null || e2 is null) return false;
        if (e1 is Circle || e2 is Circle) return false;

        return e1.Start.Equals(e2.Start) ||
               e1.Start.Equals(e2.End) ||
               e1.End.Equals(e2.Start) ||
               e1.End.Equals(e2.End);
    }

    /// <summary>
    /// used for intersect functions
    /// </summary>
    private const int IntersectTolerance = 4;

    public static bool DoesIntersect(Entity? e1, Entity? e2)
    {
        return GetIntersectPoint(e1, e2) != null;
    }

    /// <summary>
    /// Finds an intersect point between two entities.
    /// Check against null if you want to bool compare
    /// </summary>
    /// <param name="entity1"> An entity but really a Line </param>
    /// <param name="entity2"> Another entity </param>
    /// <returns> the intersect point or null if not possible </returns>
    public static Point? GetIntersectPoint(Entity? entity1, Entity? entity2)
    {
        if (entity1 == null || entity2 == null) return null;

        // todo: implement circle intersects
        if (entity1 is Circle || entity2 is Circle) return null;

        // todo: implement arc-ellipse and ellipse-ellipse intersect
        
        // If the endpoints are touching we can avoid the intersect math 
        if (Equals(entity1.Start, entity2.Start)) return entity1.Start;
        if (Equals(entity1.Start, entity2.End)) return entity1.Start;
        if (Equals(entity1.End, entity2.End)) return entity1.End;
        if (Equals(entity1.End, entity2.Start)) return entity1.End;

        if (entity1 is Line line1)
        {
            switch (entity2)
            {
                case Line line2:
                    return FindIntersectPointHelper(line1, line2);
                case Arc arc2:
                    return FindIntersectPointHelper(line1, arc2);
                case Ellipse ellipse2:
                    return FindIntersectPointHelper(line1, ellipse2);
            }
        }
        else if (entity1 is Arc arc1)
        {
            switch (entity2)
            {
                case Line line2:
                    return FindIntersectPointHelper(line2, arc1);
                case Arc arc2:
                    return FindIntersectPointHelper(arc1, arc2);
            }
        }
        else if (entity1 is Ellipse ellipse1)
        {
            switch (entity2)
            {
                case Line line2:
                    return FindIntersectPointHelper(line2, ellipse1);
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the intersect point between two lines.
    /// Treats the two lines as infinite. 
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns>
    /// returns the point where the two lines would intersect, otherwise null
    /// </returns>
    public static Point? GetInfiniteLineIntersect(Line line1, Line line2)
    {
        Point l1Delta = line1.GetDelta();
        Point l2Delta = line2.GetDelta();

        Point xDiff = new(-l1Delta.X, -l2Delta.X);
        Point yDiff = new(-l1Delta.Y, -l2Delta.Y);

        double diffCross = CrossProduct(xDiff, yDiff);

        //lines are parallel
        if (DEQ(diffCross, 0)) return null;

        Point d = new(CrossProduct(line1.Start, line1.End), CrossProduct(line2.Start, line2.End));
        double x = CrossProduct(d, xDiff) / diffCross;
        double y = CrossProduct(d, yDiff) / diffCross;

        return new Point(x, y);
    }
    
    // line with line
    private static Point? FindIntersectPointHelper(Line line1, Line line2)
    {
        Point l1Delta = line1.GetDelta();
        Point l2Delta = line2.GetDelta();

        Point xDiff = new(-l1Delta.X, -l2Delta.X);
        Point yDiff = new(-l1Delta.Y, -l2Delta.Y);

        // uses cross product to get around slope divide by 0 issues
        double diffCross = CrossProduct(xDiff, yDiff);

        // lines are parallel, don't say they intersect even if they are actually collinear
        if (DEQ(diffCross, 0)) return null;

        Point d = new(CrossProduct(line1.Start, line1.End), CrossProduct(line2.Start, line2.End));
        double x = CrossProduct(d, xDiff) / diffCross;
        double y = CrossProduct(d, yDiff) / diffCross;
        
        // if point lies on both lines it must be within both bounds
        bool within1 = IsBetween(x, line1.Start.X, line1.End.X) && IsBetween(y, line1.Start.Y, line1.End.Y);
        bool within2 = IsBetween(x, line2.Start.X, line2.End.X) && IsBetween(y, line2.Start.Y, line2.End.Y);

        if (within1 && within2) return new Point(x, y);
        return null;
    }

    // line with arc
    private static Point? FindIntersectPointHelper(Line line, Arc arc)
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
        if (DEQ(line.End.X, line.Start.X))
        {
            a = 1;
            b = 0;
            c = -1 * line.End.X;
        }
        else
        {
            double xDif = line.End.X - line.Start.X;
            
            if (DEQ(xDif, 0)) slope = 0;
            else slope = (line.End.Y - line.Start.Y) / xDif;

            if (slope is > 1000000 or < -1000000) slope = 0;

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
        double numerator = Math.Abs(a * arc.Center.X + b * arc.Center.Y + c);
        double distance = numerator / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        if (arc.Radius >= distance)
        {
            //  Will hold the solution values of the quadratic equation
            List<double> solutions = new();

            //  Special case for vertical line
            if (DEQ(line.End.X, line.Start.X))
            {
                double[] tempSolutions = QuadraticFormula(
                    1,
                    (-2 * arc.Center.Y),
                    (Math.Pow(arc.Center.Y, 2) + Math.Pow((line.End.X - arc.Center.X), 2) - Math.Pow(arc.Radius, 2))
                ).ToArray();

                foreach (double number in tempSolutions)
                {
                    solutions.Add(number);
                }

                //  Checks if each solution is on the arc, if one is on it return true
                foreach (double solution in solutions)
                {
                    //  Solution x value
                    double x = line.End.X;
                    if (arc.IsInArcRange(new Point(x, solution)) && Math.Min(line.Start.X, line.End.X) <= x &&
                        Math.Min(line.Start.Y, line.End.Y) <= solution && Math.Max(line.Start.X, line.End.X) >= x &&
                        Math.Max(line.Start.Y, line.End.Y) >= solution)
                    {
                        return new Point(x, solution);
                    }
                }
            }
            else
            {
                // todo what is going on here?
                decimal[] tempSolns = DecimalEx.SolveQuadratic((decimal)(Math.Pow(slope, 2) + 1),
                    (decimal)(-2.0 * arc.Center.X) + (decimal)(2 * (intercept * slope)) -
                    (decimal)(2 * (arc.Center.Y * slope)),
                    (decimal)Math.Pow(arc.Center.X, 2) + (decimal)Math.Pow(intercept, 2) -
                    (decimal)(2 * (intercept * arc.Center.Y)) + (decimal)Math.Pow(arc.Center.Y, 2) -
                    (decimal)Math.Pow(arc.Radius, 2));
                foreach (decimal number in tempSolns)
                {
                    solutions.Add((double)number);
                }

                //  Checks if each solution is on the arc, if one is on it return true
                foreach (double solution in solutions)
                {
                    //Solution x value
                    double x = solution;
                    //Solution y value
                    double y = slope * solution + intercept;

                    Point returnPoint = new(x, y);
                    
                    if (arc.IsInArcRange(returnPoint) &&
                        Math.Round(Math.Min(line.Start.X, line.End.X), IntersectTolerance) <= x &&
                        Math.Round(Math.Min(line.Start.Y, line.End.Y), IntersectTolerance) <= y &&
                        Math.Round(Math.Max(line.Start.X, line.End.X), IntersectTolerance) >= x &&
                        Math.Round(Math.Max(line.Start.Y, line.End.Y), IntersectTolerance) >= y)
                    {
                        return returnPoint;
                    }
                }
            }
        }

        return null;
    }

    // line with ellipse
    private static Point? FindIntersectPointHelper(Line line, Ellipse ellipse)
    {
        //Need to rotate the line around the origin for rotated ellipses
        double x = ellipse.MajorAxisEndPoint.X - ellipse.Center.X;
        double y = ellipse.MajorAxisEndPoint.Y - ellipse.Center.Y;
        double rotation;
        if (DEQ(x, 0))
        {
            rotation = y > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
        }
        else if (DEQ(y, 0))
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
            line = new Line(-1 * ((line.Start.X * Math.Cos(rotation)) - (line.Start.Y * Math.Sin(rotation))),
                -1 * ((line.Start.Y * Math.Cos(rotation)) + (line.Start.X * Math.Sin(rotation))),
                -1 * ((line.End.X * Math.Cos(rotation)) - (line.End.Y * Math.Sin(rotation))),
                -1 * ((line.End.Y * Math.Cos(rotation)) + (line.End.X * Math.Sin(rotation))));
        }

        //  Get line in the form Ax + By + C = 0 and moved so that ellipse center is the origin
        double lineA;
        double lineB;
        double lineC;
        bool isVertical = false;
        //  This is to check for a vertical line, since it would crash the program
        //  trying to divide by 0
        if ((new Point(line.Start.X, 0).Equals(new Point(line.End.X, 0))))
        {
            lineA = 1;
            lineB = 0;
            lineC = -1 * (line.End.X - ellipse.Center.X);
            isVertical = true;
        }
        else
        {
            double lineSlope = (line.End.Y - line.Start.Y) / (line.End.X - line.Start.X);
            double lineIntercept = (line.End.Y - ellipse.Center.Y) - (lineSlope * (line.End.X - ellipse.Center.X));
            // The slope of the line ends up being A in the general form
            lineA = lineSlope;
            lineC = lineIntercept;
            lineB = -1;
            //  A cannot be negative in the general form
            if (lineA < 0)
            {
                lineA *= -1;
                lineB *= -1;
                lineC *= -1;
            }
        }

        double major = Point.Distance(ellipse.MajorAxisEndPoint, ellipse.Center);
        double minor = major * ellipse.MinorToMajorAxisRatio;
        //List of solutions from equations
        List<Point> solutionCoords = new();
        //Vertical line case
        if (isVertical && lineC <= Math.Round(major, IntersectTolerance))
        {
            solutionCoords.Add(new Point(-1 * lineC, minor * Math.Sqrt(1 - (Math.Pow(lineC, 2) / Math.Pow(major, 2)))));
            if (lineC < Math.Round(major, IntersectTolerance))
            {
                solutionCoords.Add(new Point(-1 * lineC,
                    -1 * (minor * Math.Sqrt(1 - (Math.Pow(lineC, 2) / Math.Pow(major, 2))))));
            }
        }
        else
        {
            double a = Math.Pow(lineA, 2) + ((Math.Pow(lineB, 2) * Math.Pow(minor, 2)) / Math.Pow(major, 2));
            double b = -2 * lineA * lineC;
            double c = Math.Pow(lineC, 2) - (Math.Pow(lineB, 2) * Math.Pow(minor, 2));
            //List of x value solutions
            List<double> xSolutions = QuadraticFormula(a, b, c);
            bool firstSolution = false;
            foreach (double xSolution in xSolutions)
            {
                double yValue;
                if (isVertical)
                {
                    if (!firstSolution)
                    {
                        yValue = Math.Sqrt(Math.Pow(minor, 2) -
                                           ((Math.Pow(xSolution, 2) / Math.Pow(major, 2)) * Math.Pow(minor, 2)));
                        solutionCoords.Add(new Point(xSolution, yValue));
                        firstSolution = true;
                    }
                    else
                    {
                        yValue = -1 * (Math.Sqrt(Math.Pow(minor, 2) -
                                                 ((Math.Pow(xSolution, 2) / Math.Pow(major, 2)) * Math.Pow(minor, 2))));
                        solutionCoords.Add(new Point(xSolution, yValue));
                    }
                }
                else
                {
                    yValue = (((-1 * lineA) * xSolution) - lineC) / lineB;
                    solutionCoords.Add(new Point(xSolution, yValue));
                }
            }
        }

        if (solutionCoords.Count > 0)
        {
            foreach (Point solutionCoord in solutionCoords)
            {
                double compX = Math.Round(solutionCoord.X + ellipse.Center.X, IntersectTolerance);
                double compY = Math.Round(solutionCoord.Y + ellipse.Center.Y, IntersectTolerance);
                double roundedStartX = Math.Round(line.Start.X, IntersectTolerance);
                double roundedEndX = Math.Round(line.End.X, IntersectTolerance);
                double roundedStartY = Math.Round(line.Start.Y, IntersectTolerance);
                double roundedEndY = Math.Round(line.End.Y, IntersectTolerance);
                
                if (ellipse.IsInEllipseRange(new Point(compX, compY))
                    && Math.Min(roundedStartX, roundedEndX) <= compX
                    && Math.Max(roundedStartX, roundedEndX) >= compX
                    && Math.Min(roundedStartY, roundedEndY) <= compY
                    && Math.Max(roundedStartY, roundedEndY) >= compY)
                {
                    return new Point(compX, compY);
                }
            }
        }

        return null;
    }

    // arc with arc
    private static Point? FindIntersectPointHelper(Arc arc1, Arc arc2)
    {
        // Treat both Arcs circles, get the line between their centers
        Line between = new Line(arc1.Center.X, arc1.Center.Y, arc2.Center.X, arc2.Center.Y);

        // First case, the circles do not intersect as they are too far apart
        // Second case, one circle is entirely inside the other but not intersecting.
        if (between.GetLength() > (arc1.Radius + arc2.Radius) 
            || between.GetLength() < (Math.Abs(arc1.Radius - arc2.Radius)) 
            || DEQ(between.GetLength(), 0))
        {
            return null;
        }

        // The circles intersect. Do they intersect at the position of the arcs?

        // Find a and h.
        double a = (Math.Pow(arc1.Radius, 2) - Math.Pow(arc2.Radius, 2) + Math.Pow(between.GetLength(), 2)) /
                   (2 * between.GetLength());
        double h = Math.Sqrt(Math.Pow(arc1.Radius, 2) - Math.Pow(a, 2));

        // Find P2.
        double cx2 = arc1.Center.X + a * (arc2.Center.X - arc1.Center.X) / between.GetLength();
        double cy2 = arc1.Center.Y + a * (arc2.Center.Y - arc1.Center.Y) / between.GetLength();

        // Get the points P3.
        double intersect1X = (cx2 + h * (arc2.Center.Y - arc1.Center.Y) / between.GetLength());
        double intersect1Y = (cy2 - h * (arc2.Center.X - arc1.Center.X) / between.GetLength());
        double intersect2X = (cx2 - h * (arc2.Center.Y - arc1.Center.Y) / between.GetLength());
        double intersect2Y = (cy2 + h * (arc2.Center.X - arc1.Center.X) / between.GetLength());

        bool intersect1IsValid = arc1.IsInArcRange(new Point(intersect1X, intersect1Y)) &&
                                 arc2.IsInArcRange(new Point(intersect1X, intersect1Y));
        bool intersect2IsValid = arc1.IsInArcRange(new Point(intersect2X, intersect2Y)) &&
                                 arc2.IsInArcRange(new Point(intersect2X, intersect2Y));

        if (intersect1IsValid) return new Point(intersect1X, intersect1Y);
        if (intersect2IsValid) return new Point(intersect2X, intersect2Y);
        return null;
    }
}