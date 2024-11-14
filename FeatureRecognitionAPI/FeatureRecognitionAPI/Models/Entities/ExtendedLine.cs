﻿using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;
using NHibernate.SqlCommand;

namespace FeatureRecognitionAPI.Models
{
    public class ExtendedLine : Line
    {
        public Line Parent1 { get { return Parent1; } set { Parent1 = value; calcPoints(); } }
        public Line Parent2 { get { return Parent2; } set { Parent2 = value; calcPoints(); } }

        public ExtendedLine() 
        {
        }
        public ExtendedLine(Line parent1, Line parent2)
        {
            Parent1 = new Line(parent1);
            Parent2 = new Line(parent2);
            calcPoints();
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
