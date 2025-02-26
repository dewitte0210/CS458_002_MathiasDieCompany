//using System;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Linq;
//using System.Runtime.ExceptionServices;
//using System.Text;
//using System.Threading.Tasks;

//using FeatureRecognitionAPI.Models.Entities;

//TODO: overload cross and dot product instead of having different for lines and vectors
//TODO: remove classes like line that are already in project/library
//TODO: make angle class more usable
//TODO: convert to best practices
//TODO: verify that getMinorAngle always calculates interior angle and rename
//TODO: clean up comments
//TODO: document methods
//TODO: verify that polygon members stay up to date

// Author: Andrew Schmidt
namespace FeatureRecognitionAPI.Models.Utility
{
    class Angles
    {
        public enum Side
        {
            INTERIOR,
            EXTERIOR,
            UNKNOWN
        }

        public enum Orientation
        {
            COUNTERCLOCKWISE,
            CLOCKWISE,
            UNKNOWN
        }

        public class Angle
        {
            private double degrees;
            public Side side;

            public Angle(double newDegrees, Side newSide)
            {
                degrees = newDegrees;
                side = newSide;
            }

            public double getDegrees()
            {
                return degrees;
            }

            public double getRadians()
            {
                return degrees * (Math.PI / 180);
            }
        }

        //public class Point
        //{
        //    public double x;
        //    public double y;

        //    public Point(double newX, double newY)
        //    {
        //        x = newX;
        //        y = newY;
        //    }
        //}

        //public class ThreePoint
        //{
        //    public bool valid;

        //    public Point a;
        //    public Point b;
        //    public Point c;

        //    public ThreePoint(Point newA, Point newB, Point newC)
        //    {
        //        a = newA;
        //        b = newB;
        //        c = newC;
        //    }
        //}

        //public class Line
        //{
        //    public Point start;
        //    public Point end;

        //    public Line(Point newStart, Point newEnd)
        //    {
        //        start = newStart;
        //        end = newEnd;
        //    }

        //    public Point getRelative()
        //    {
        //        return new Point(end.x - start.x, end.y - start.y);
        //    }

        //    public double getLength()
        //    {
        //        return Math.Sqrt(Math.Pow(getRelative().x, 2) + Math.Pow(getRelative().y, 2));
        //    }
        //}

        //confirm that this math is correct

        //cannot guarantee this way will work with lines
        //may need to translate the two lines to two points??
        public static double CrossProduct(Point a, Point b)
        {
            //Console.WriteLine(a.getRelative().x + " " + a.getRelative().y + " " + b.getRelative().x + " " + b.getRelative().y);
            //Console.WriteLine(a.getRelative().x * b.getRelative().y - a.getRelative().y * b.getRelative().x);
            //return a.getRelative().x * b.getRelative().y - a.getRelative().y * b.getRelative().x;
            return a.X * b.Y - a.Y * b.X;
        }

        //get the cross product of two lines
        //uses the delta of the two given lines
        public static double CrossProduct(Line a, Line b)
        {
            Point aDelta = a.GetDelta();
            Point bDelta = b.GetDelta();
            return aDelta.X * bDelta.Y - aDelta.Y * bDelta.X;
        }

        public static double DotProduct(Point a, Point b)
        {
            //return a.getRelative().x * b.getRelative().x + a.getRelative().y * b.getRelative().y;
            //return a.start.x * b.start.x + a.start.y * b.start.y;
            return a.X * b.X + a.Y * b.Y;
        }

        public static double DotProduct(Line a, Line b)
        {
            Point aDelta = a.GetDelta();
            Point bDelta = b.GetDelta();
            return aDelta.X * bDelta.X + aDelta.Y * bDelta.Y;

            //return a.getRelative().x * b.getRelative().x + a.getRelative().y * b.getRelative().y;
            //return a.start.x * b.start.x + a.start.y * b.start.y;
        }

        //public static ThreePoint getThreePoint(Line a, Line b)
        //{
        //    ThreePoint tp = new ThreePoint(new Point(0, 0), new Point(0, 0), new Point(0, 0));
        //    tp.valid = false;

        //    tp.a = a.start;
        //    tp.c = b.end;

        //    //shared start and end
        //    if (a.end == b.start)
        //    {
        //        tp.b = a.end;
        //        tp.valid = true;
        //    }
        //    //lines intersect at some point
        //    else if (true)
        //    {
        //        Console.WriteLine("lines do not touch, need to handle this case");
        //    }
        //    //otherwise lines are parallel

        //    return tp;
        //}

        public static Angle GetMinorAngle(Line a, Line b)
        {
            double cross = CrossProduct(a, b);
            double dot = DotProduct(a, b);

            double cos_theta = dot / (a.Length * b.Length);
            cos_theta = Math.Max(-1, Math.Min(1, cos_theta));

            double angle = 0;
            angle = Math.Acos(cos_theta) * (180 / Math.PI);

            if (cross < 0)
            {
                angle = 360 - angle;
            }

            //need to verify that 180 minus angle gives right thing
            return new Angle(Math.Abs(180 - angle), Side.UNKNOWN);
        }

        public Side GetAngleDirection(Line a, Line b)
        {
            double cross = CrossProduct(a, b);

            if (cross > 0)
            {
                return Side.INTERIOR;
            }
            else if (cross < 0)
            {
                return Side.EXTERIOR;
            }
            else
            {
                return Side.UNKNOWN;
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

                area = Math.Abs(GetShoelaceArea());
                orientation = GetOrientation();
            }
            private double GetShoelaceArea()
            {
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

            private Orientation GetOrientation()
            {
                double area = GetShoelaceArea();

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
    }
}

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