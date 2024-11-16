using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;
using NHibernate.SqlCommand;

namespace FeatureRecognitionAPI.Models
{
    public class ExtendedLine : Line
    {
        //public Line Parent1 { get { return Parent1; } set { Parent1 = value; calcPoints(); } }
        public Line Parent1 { get; set; }
        public Line Parent2 { get; set; }

        public ExtendedLine() 
        {
        }
        //runs into issues if more than one perimeter feature is on a line
        //would show up as an ExtendedLine being a parent
        //this would also throw an error when trying to find a path because the parent would not be in EntityList
        public ExtendedLine(Line parent1, Line parent2) : base(parent1) // This just gives the line parent1's values by default
        {
            Parent1 = new Line(parent1);
            Parent2 = new Line(parent2);
            calcPoints();

            SlopeY = EndPoint.Y - StartPoint.Y;
            SlopeX = EndPoint.X - StartPoint.X;

            // Distance Calculation
            this.Length = (Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2) + Math.Pow(EndPoint.Y - StartPoint.Y, 2)));
        }

        //Function that calculates StartPoint and EndPoint based off parents
        public void calcPoints()
        {
            if (Parent1 != null && Parent2 != null)
            {
                Point pointToExtend;
                if (Parent1.findDistance(
                        Parent1.StartPoint,
                        Parent2.StartPoint)
                        < Parent1.findDistance(
                        Parent1.EndPoint,
                        Parent2.StartPoint))
                //This looks like a lot but all this is doing is finding the closest point on line1 to line2
                {
                    //At this point we know the point to be extended on line1 is the start point, meaning the end point can stay the same
                    //  Hence why tempLine end point is set to line1's
                    pointToExtend = Parent1.StartPoint;
                    StartPoint.X = Parent1.EndPoint.X;
                    StartPoint.Y = Parent1.EndPoint.Y;
                }
                else
                {
                    pointToExtend = Parent1.EndPoint;
                    StartPoint.X = Parent1.StartPoint.X;
                    StartPoint.Y = Parent1.StartPoint.Y;
                }
                if (Parent2.findDistance(
                    pointToExtend,
                    Parent2.StartPoint)
                    > Parent2.findDistance(
                    pointToExtend,
                    Parent2.EndPoint))
                //Similar to the one above but finds what point on line2 is farthest from line1's point to extend
                {
                    EndPoint.X = Parent2.StartPoint.X;
                    EndPoint.Y = Parent2.StartPoint.Y;
                }
                else
                {
                    EndPoint.X = Parent2.EndPoint.X;
                    EndPoint.Y = Parent2.EndPoint.Y;
                }
            }
        }
    }
}
