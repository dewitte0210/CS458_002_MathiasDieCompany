using NUnit;
using FeatureRecognitionAPI.Models;

namespace Testing_for_Project
{
    internal class EntityClassTests
    {
        // Tests for IsInArcRange
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

        //Tests for IntersectArcWithArc
        [Test]
        public void IntersectArcWithArc_HappyPath_ReturnTrue()
        {
            Arc arc1 = new Arc(0.0m, 0.0m, 10m, 270m, 90m);
            Arc arc2 = new Arc(20.0m, 0.0m, 10m, 90m, 280m);
            bool test = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(test, Is.True);
        }

        [Test]
        public void IntersectArcWithArc_NonIntersecting_ReturnFalse()
        {
            Arc arc1 = new Arc(0.0m, 0.0m, 10m, 270m, 90m);
            Arc arc2 = new Arc(25.0m, 0.0m, 10m, 90m, 280m);
            bool test = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(test, Is.False);
        }

        [Test]
        public void IntersectArcWithArc_DoubleIntersect_ReturnTrue()
        {

            Arc arc1 = new Arc(0.0m, 0.0m, 10m, 270m, 90m);
            Arc arc2 = new Arc(19.0m, 0.0m, 10m, 90m, 270m);
            bool test = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(test, Is.True);
        }
    }
}
