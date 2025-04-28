using static FeatureRecognitionAPI.Models.Utility.Angles;

namespace FeatureRecognitionAPI.Models.Utility;

public static class EntityTools
{
    /// <summary>
    /// Determine if two lines are collinear. Two lines are collinear if they are parallel
    /// and lie on the same infinite line. NOT TESTED YET
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns> true if the two given lines are collinear </returns>
    public static bool IsCollinear(Line line1, Line line2)
    {
        if (!IsParallel(line1, line2)) return false;

        //turn the 2 lines into 3 vectors based on line1 start
        Point p1 = line1.GetDelta();
        Point p2 = new Line(line1.Start, line2.Start).GetDelta();
        Point p3 = new Line(line1.Start, line2.End).GetDelta();
        
        //if they are collinear these should both be 0
        double cross1 = double.Abs(CrossProduct(p1, p2));
        double cross2 = double.Abs(CrossProduct(p1, p3));
        
        return MdcMath.DoubleEquals(cross1, 0) && MdcMath.DoubleEquals(cross2, 0);
    }
    
    /// <summary>
    /// find the intersect point of two lines
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns> returns the intersection point of two lines, null if not possible </returns>
    
    private const int intersectTolerance = 4;
    
    /**
         * Function that finds at what point two lines intersect when they are treated as infinite
         * this is mostly for the perpendicular line check, which is commented out
         * 
         * @param line1 is the first line being checked
         * @param line2 is the second line being checked
         * @return the point that line1 and line2 intersects. The points intersect field will be false if they are parallel
         */
        public static Point? GetIntersectPoint(Entity? entity1, Entity? entity2)
        {
            if (entity1 == null || entity2 == null) return null;
            
            if (entity1 is Line line1 && entity2 is Line line2)
            {
                Point l1Delta = line1.GetDelta();
                Point l2Delta = line2.GetDelta();
        
                Point xDiff = new(-l1Delta.X, -l2Delta.X);
                Point yDiff = new(-l1Delta.Y, -l2Delta.Y);

                double diffCross = CrossProduct(xDiff, yDiff);
        
                //lines are parallel
                if (MdcMath.DoubleEquals(diffCross, 0)) return null;
        
                Point d = new(CrossProduct(line1.Start, line1.End), CrossProduct(line2.Start, line2.End));
                double x = CrossProduct(d, xDiff) / diffCross;
                double y = CrossProduct(d, yDiff) / diffCross;
        
                return new Point(x, y);
            }

            if (entity1 is Line line && entity2 is Arc arc)
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
                if (line.End.X == line.Start.X)
                {
                    a = 1;
                    b = 0;
                    c = -1 * line.End.X;
                }
                else
                {
                    double xDif = line.End.X - line.Start.X;
                    if (xDif == 0)
                    {
                        slope = 0;
                    }
                    else
                    {
                        slope = (line.End.Y - line.Start.Y) / xDif;
                    }

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
            }

            if (entity1 is Line line3 && entity2 is Ellipse ellipse)
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
                line3 = new Line(-1 * ((line3.Start.X * Math.Cos(rotation)) - (line3.Start.Y * Math.Sin(rotation))), -1 * ((line3.Start.Y * Math.Cos(rotation)) + (line3.Start.X * Math.Sin(rotation))), -1 * ((line3.End.X * Math.Cos(rotation)) - (line3.End.Y * Math.Sin(rotation))), -1 * ((line3.End.Y * Math.Cos(rotation)) + (line3.End.X * Math.Sin(rotation))));
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
            if ((new Point(line3.Start.X, 0).Equals(new Point(line3.End.X, 0))))
            {
                Al = 1;
                Bl = 0;
                Cl = -1 * (line3.End.X - ellipse.Center.X);
                isVertical = true;
            }
            else
            {
                slopel = (line3.End.Y - line3.Start.Y) / (line3.End.X - line3.Start.X);
                interceptl = (line3.End.Y - ellipse.Center.Y) - (slopel * (line3.End.X - ellipse.Center.X));
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
                        && Math.Min(Math.Round(line3.Start.X, intersectTolerance), Math.Round(line3.End.X, intersectTolerance)) <= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Min(Math.Round(line3.Start.Y, intersectTolerance), Math.Round(line3.End.Y, intersectTolerance)) <= (SolnCoords[i].Y + ellipse.Center.Y)
                        && Math.Max(Math.Round(line3.Start.X, intersectTolerance), Math.Round(line3.End.X, intersectTolerance)) >= (SolnCoords[i].X + ellipse.Center.X)
                        && Math.Max(Math.Round(line3.Start.Y, intersectTolerance), Math.Round(line3.End.Y, intersectTolerance)) >= (SolnCoords[i].Y + ellipse.Center.Y)) { return new Point(SolnCoords[i].X + ellipse.Center.X, SolnCoords[i].Y + ellipse.Center.Y); }
                }
            }
            return null;
            }

            return null;
        }
    
    
    internal static List<double> QuadraticFormula(double a, double b, double c)
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
    
    /// <summary>
    /// Extends two lines to their intersect point. Changes the value of the lines passed through.
    /// Only tested for non-touching line segments for chamfered line extension,
    /// change if you need advanced behavior
    /// </summary>
    /// <param name="line1"> nullable line A </param>
    /// <param name="line2"> nullable line B </param>
    /// <returns> returns whether the lines were successfully extended or not </returns>
    public static bool ExtendTwoLines(Line? line1, Line? line2)
    {
        if (line1 == null || line2 == null) return false;
        
        // todo: if collinear merge into one line
        if (IsCollinear(line1, line2)) return false;
        
        //if parallel do not extend 
        if (IsParallel(line1, line2)) return false;

        Point? intPoint = GetIntersectPoint(line1, line2);
        if (intPoint == null) return false;
        
        // todo: update line adjacency lists
        double line1StartDistance = Point.Distance(line1.Start, intPoint);
        double line1EndDistance = Point.Distance(line1.End, intPoint);
        //if start is closer to intersect set that as new start
        if (line1StartDistance < line1EndDistance)
        {
            line1.Start = intPoint;
        }
        else
        {
            line1.End = intPoint;
        }
        
        double line2StartDistance = Point.Distance(line2.Start, intPoint);
        double line2EndDistance = Point.Distance(line2.End, intPoint);
        if (line2StartDistance < line2EndDistance)
        {
            line2.Start = intPoint;
        }
        else
        {
            line2.End = intPoint;
        }

        // update Length because for some reason it isn't a function
        //line1.Length = Point.Distance(line1.Start, line1.End);
        //line2.Length = Point.Distance(line2.Start, line2.End);

        return true;
    }
}