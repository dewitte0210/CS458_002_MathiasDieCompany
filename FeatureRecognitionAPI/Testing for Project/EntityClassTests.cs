using NUnit;
using FeatureRecognitionAPI.Models;

namespace Testing_for_Project
{
    internal class EntityClassTests
    {

        [Test]
        public void IsInArcRange_HappyPath_ReturnTrue()
        {
            Line testEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool test = testEntity.IsInArcRange(0.0m, 0.0m, 0.0m, 0.0m, 0m, 0m);
        }
    
    }
}
