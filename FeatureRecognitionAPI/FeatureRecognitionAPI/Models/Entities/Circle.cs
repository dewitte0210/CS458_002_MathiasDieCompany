using System;
using System.IO;
using System.Numerics;

namespace FeatureRecognitionAPI.Models { 
    public class Circle : Entity
    {
        Circle()
        {
            entityType = PossibleEntityTypes.circle;
        }
    }
}
