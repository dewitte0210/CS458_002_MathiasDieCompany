using NUnit;
using FeatureRecognitionAPI.Models;
using DecimalMath;

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

        /**
         * Tests for IntersectLineWithArc method
         *  Testing for:
         *  - Sloped line with no intersection point
         *  - Sloped line with one intersection point
         *  - Sloped line with two intersection points
         *  - Horizontal line with no intersection point
         *  - Vertical line with no intersection point
         */
        [Test]
        public void IntersectLineWithArc_LineNoIntersection_ReturnFalse()
        {
            Line testLine = new(-2m, -1m, 0m, 5m);
            Arc testArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool test = testLine.IntersectLineWithArc(testLine, testArc);
            Assert.That(test, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_LineOneIntersection_ReturnTrue()
        {
            Line testLine = new(1m, 0m, 4m, 3m);
            Arc testArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool test = testLine.IntersectLineWithArc(testLine, testArc);
            Assert.That(test, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_LineTwoIntersections_ReturnFalse()
        {
            Line testLine = new(3m, -1m, 4m, 7m);
            Arc testArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool test = testLine.IntersectLineWithArc(testLine, testArc);
            Assert.That(test, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_HorizontalLineNoIntersection_ReturnFalse()
        {
            Line testLine = new(0m, 6m, 4m, 6m);
            Arc testArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool test = testLine.IntersectLineWithArc(testLine, testArc);
            Assert.That(test, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_HorizontalLineOneIntersection_ReturnTrue()
        {
            Line testLine = new(3m, 2m, 5m, 2m);
            Arc testArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool test = testLine.IntersectLineWithArc(testLine, testArc);
            Assert.That(test, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_VerticalLineNoIntersection_ReturnFalse()
        {
            Line testLine = new(4m, 0m, 4m, 6m);
            Arc testArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool test = testLine.IntersectLineWithArc(testLine, testArc);
            Assert.That(test, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_VerticalLineTwoIntersections_ReturnTrue()
        {
            Line testLine = new(3m, -1m, 3m, 6m);
            Arc testArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool test = testLine.IntersectLineWithArc(testLine, testArc);
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
