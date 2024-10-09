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
            bool test = testEntity.IsInArcRange(0.0m,0.0m,1.0m,0.0m, 0m, 90m);
            Assert.That(test, Is.True);
        }

        [Test]
        public void IsInArcRange_XisNegativeOne_ReturnTrue()
        {
            Line testEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool test = testEntity.IsInArcRange(0.0m,0.0m,-1.0m,0.0m, 90m, 180m);
            Assert.That(test, Is.True);
        }
        [Test]
        public void IsInArcRange_NotInRange_ReturnFalse()
        {
            Line testEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool test = testEntity.IsInArcRange(0.0m,0.0m,0.0m,-1.0m, 0m, 90m);
            Assert.That(test, Is.False);
        }
        [Test] 
        public void IsInArcRange_StartGreaterThanEnd_ReturnTrue()
        {

            Line testEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool test = testEntity.IsInArcRange(0.0m,0.0m,1.0m,0.0m, 270m, 0m);
            Assert.That(test, Is.True);
        }
    }
}
