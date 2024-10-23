﻿using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        public  double StartX { get; set; }
        public  double StartY { get; set; }
        public  double EndX { get; set; }
        public  double EndY { get; set; }


        public  double Length { get; } 
        private Line()
        {
            entityType = PossibleEntityTypes.line;
        }

        public Line( double startX,  double startY,  double endX,  double endY)
        {
            StartX = startX;
            StartY = startY;

            SlopeX = EndX - StartX;
            this.setLength(Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2)));
            }
            else return endPoint;
        }
    }
}
