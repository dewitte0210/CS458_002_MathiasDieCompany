//TODO: verify that getMinorAngle always calculates interior angle and rename
//TODO: decide what to do with polygon class

/// Author: Andrew Schmidt
/// <summary>
/// This file is used for calculating the angle between lines and on what side they lay
/// </summary>
namespace FeatureRecognitionAPI.Models.Utility
{
    /// <summary>
    /// Angles contains classes and functions for calculating angles
    /// Not to be used directly, use Angle class, Side and Orientation enums, etc
    /// </summary>
    class Angles
    {
        /// <summary>
        /// The side an angle lies in relation to the rest of the shape
        /// </summary>
        public enum Side
        {
            INTERIOR,
            EXTERIOR,
            UNKNOWN
        }

        /// <summary>
        /// The general orientation of a polygon
        /// This is the direction its lines are drawn
        /// Ideally all lines of a polygon should have a consistent direction to them
        /// </summary>
        public enum Orientation
        {
            COUNTERCLOCKWISE,
            CLOCKWISE,
            UNKNOWN
        }

        public class Degrees(double angle)
        {
            public double angle = angle;

            public Degrees GetOppositeAngle()
            {
                return new Degrees(360 - angle);
            }

            public void SetToOpposite()
            {
                angle = 360 - angle;
            }

            public Radians ToRadians()
            {
                return new Radians(Angles.ToRadians(angle));
            }
            
        }

        public class Radians(double angle)
        {
            public double angle = angle;

            public Radians GetOppositeAngle()
            {
                return new Radians((2 * Math.PI) - angle);
            }

            public void SetToOpposite()
            {
                angle = (2 * Math.PI) - angle;
            }

            public Degrees ToDegrees()
            {
                return new Degrees(Angles.ToDegrees(angle));
            }

        }

        /// <summary>
        /// The angle and side that angle is on between two lines
        /// </summary>
        public class Angle
        {
            private readonly Degrees angle;
            private readonly Side side;

            public Angle(Degrees angle, Side side)
            {
                this.angle = angle;
                this.side = side;
            }

            public Angle(Radians angle, Side side)
            {
                this.angle = angle.ToDegrees();
                this.side = side;
            }

            public Degrees GetDegrees()
            {
                return angle;
            }

            public Radians GetRadians()
            {
                return angle.ToRadians();
            }

            public Side GetSide()
            {
                return side;
            }
        }


        public static double CrossProduct(Point a, Point b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Calculates the cross product of two lines using their deltas
        /// </summary>
        public static double CrossProduct(Line a, Line b)
        {
            Point aDelta = a.GetDelta();
            Point bDelta = b.GetDelta();
            return aDelta.X * bDelta.Y - aDelta.Y * bDelta.X;
        }

        public static double DotProduct(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }
        
        public static double ToRadians(double degrees)
        {
            return degrees *  Math.PI / 180;
        }

        /// <summary>
        /// Calculates the dot product of two lines using their deltas
        /// </summary>
        public static double DotProduct(Line a, Line b)
        {
            Point aDelta = a.GetDelta();
            Point bDelta = b.GetDelta();
            return aDelta.X * bDelta.X + aDelta.Y * bDelta.Y;
        }

        // may not handle all cases yet, to be tested
        public static Angle GetAngle(Line a, Line b, Side targetSide = Side.INTERIOR)
        {
            double cross = CrossProduct(a, b);
            double dot = DotProduct(a, b);
            Side side = Side.UNKNOWN;

            double cos_theta = dot / (a.Length * b.Length);
            cos_theta = Math.Max(-1, Math.Min(1, cos_theta));

            double angle = Math.Acos(cos_theta) * (180 / Math.PI);

            if (cross < 0)
            {
                angle = 360 - angle;
            }

            if (cross > 0)
            {
                side = Side.INTERIOR;
            }
            else if (cross < 0)
            {
                side = Side.EXTERIOR;
            }

            Degrees returnAngle = new Degrees(Math.Abs(180 - angle));
            if (side != Side.UNKNOWN && side != targetSide)
            {
                returnAngle.SetToOpposite();
                side = targetSide;
            }

            return new Angle(returnAngle, side);
        }

        /// <summary>
        /// Checks if a polygon is closed.
        /// The polygon must be entirely made of lines.
        /// </summary>
        /// <param name="lineList"></param>
        /// <returns></returns>
        public static bool PolygonIsClosed(List<Line> lineList)
        {
            //check if closed
            for (int i = 0; i < lineList.Count; i++)
            {
                if (lineList[0].EndPoint != lineList[(i + 1) % lineList.Count].StartPoint)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates the area of a polygon.
        /// If the area is positive the area is counterclockwise.
        /// Only works for polygons made entirely up of lines.
        /// Make sure to do absolute value if all you need is the area.
        /// </summary>
        /// <param name="lineList"></param>
        /// <returns></returns>
        public static double GetShoelaceArea(List<Line> lineList)
        {
            if (!PolygonIsClosed(lineList))
            {
                return 0;
            }

            double area = 0;
            for (int i = 0; i < lineList.Count; i++)
            {
                area += CrossProduct(lineList[i], lineList[(i + 1) % lineList.Count]);
            }
            if (area != 0)
            {
                area /= 2;
            }
            return area;
        }

        /// <summary>
        /// Uses shoelace area to determine the orientation of a polygon.
        /// The same conditions apply as GetShoelaceArea
        /// </summary>
        /// <param name="lineList"></param>
        /// <returns></returns>
        public static Orientation GetOrientation(List<Line> lineList)
        {
            double area = GetShoelaceArea(lineList);

            if (area > 0)
            {
                return Orientation.COUNTERCLOCKWISE;
            }
            else if (area < 0)
            {
                return Orientation.CLOCKWISE;
            }
            else
            {
                return Orientation.UNKNOWN;
            }
        }

        public class Polygon
        {
            List<Line> lineList;

            public double area;
            public Orientation orientation;

            public Polygon(List<Line> newLineList)
            {
                lineList = newLineList;

                area = Math.Abs(GetShoelaceArea(lineList));
                orientation = GetOrientation(lineList);
            }
        }
    }
}

    //    static void Main(string[] args)
    //    {
    //        //counterclockwise
    //        List<Line> ll = new List<Line>();
    //        ll.Add(new Line(new Point(0, 0), new Point(5, 0)));
    //        ll.Add(new Line(new Point(5, 0), new Point(3, 3)));
    //        ll.Add(new Line(new Point(3, 3), new Point(0, 0)));

    //        Polygon poly = new Polygon(ll);
    //        Angle ang = getMinorAngle(ll[0], ll[1]);
    //        Console.WriteLine(ang.getDegrees());

    //        //clockwise
    //        List<Line> llc = new List<Line>();
    //        llc.Add(new Line(new Point(0, 0), new Point(3, 3)));
    //        llc.Add(new Line(new Point(3, 3), new Point(5, 0)));
    //        llc.Add(new Line(new Point(5, 0), new Point(0, 0)));

    //        Console.WriteLine(getMinorAngle(llc[0], llc[1]).getDegrees());

    //        //180
    //        Line l1 = new Line(new Point(0, 0), new Point(0, 9));
    //        Line l2 = new Line(new Point(0, 9), new Point(0, 0));
    //        Console.WriteLine(getMinorAngle(l1, l2).getDegrees());

    //        //parallel
    //        Console.WriteLine(getMinorAngle(l1, l1).getDegrees());

    //        //seperated
    //        Line l3 = new Line(new Point(0, 0), new Point(0, 9));
    //        Line l4 = new Line(new Point(5, 5), new Point(15, 15));
    //        Console.WriteLine(getMinorAngle(l3, l4).getDegrees());

    //        Console.Write("end");
    //    }

//cases to test
// touching lines
//	good!
// reversed
//	negative angle
// 180 touching
//	good!
// parallel touching
//	good!
// seperated for each
//clockwise for each
//obtuse angles