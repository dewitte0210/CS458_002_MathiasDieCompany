using System;
using System.IO;
using System.Numerics;

namespace FeatureRecognitionAPI.Models
{
    public class Line : Entity
    {
        Line()
        {
            entityType = PossibleEntityTypes.line;
        }
    }
}
