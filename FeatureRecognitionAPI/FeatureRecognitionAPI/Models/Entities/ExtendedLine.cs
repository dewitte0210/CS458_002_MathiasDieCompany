using FeatureRecognitionAPI.Models.Enums;
using System;
using System.IO;
using System.Numerics;
using DecimalMath;

namespace FeatureRecognitionAPI.Models
{
    public class ExtendedLine : Line
    {
        public Line Parent1 { get { return Parent1; } set { calcPoint(); } }
        public Line Parent2 { get { return Parent2; } set { calcPoint(); } }

        public ExtendedLine() { }
        public ExtendedLine(Line parent1, Line parent2)
        {
            this.Parent1 = parent1;
            this.Parent2 = parent2;
        }

        public void calcPoint()
        {

        }
    }
}
