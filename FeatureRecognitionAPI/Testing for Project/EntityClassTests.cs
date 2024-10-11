using NUnit;
using FeatureRecognitionAPI.Models;
using DecimalMath;

namespace Testing_for_Project
{
    internal class EntityClassTests
    {
        #region IsInArcRange 
        [Test]
        public void IsInArcRange_HappyPath_ReturnTrue()
        {
            Line resultEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool result = resultEntity.IsInArcRange(0.0m,0.0m,1.0m,0.0m, 0m, 90m);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsInArcRange_XisNegativeOne_ReturnTrue()
        {
            Line resultEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool result = resultEntity.IsInArcRange(0.0m,0.0m,-1.0m,0.0m, 90m, 180m);
            Assert.That(result, Is.True);
        }
       
        [Test]
        public void IsInArcRange_NotInRange_ReturnFalse()
        {
            Line resultEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool result = resultEntity.IsInArcRange(0.0m,0.0m,0.0m,-1.0m, 0m, 90m);
            Assert.That(result, Is.False);
        }
        [Test] 
        public void IsInArcRange_StartGreaterThanEnd_ReturnTrue()
        {

            Line resultEntity = new(0.0m,0.0m,0.0m,0.0m);
            bool result = resultEntity.IsInArcRange(0.0m,0.0m,1.0m,0.0m, 270m, 0m);
            Assert.That(result, Is.True);
        }
        #endregion

        #region IntersectLineWithArc
        /**
         * results for IntersectLineWithArc method
         *  resulting for:
         *  - Sloped line with no intersection point
         *  - Sloped line with one intersection point
         *  - Sloped line with two intersection points
         *  - Horizontal line with no intersection point
         *  - Vertical line with no intersection point
         */
        [Test]
        public void IntersectLineWithArc_LineNoIntersection_ReturnFalse()
        {
            Line resultLine = new(-2m, -1m, 0m, 5m);
            Arc resultArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_LineOneIntersection_ReturnTrue()
        {
            Line resultLine = new(1m, 0m, 4m, 3m);
            Arc resultArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_LineTwoIntersections_ReturnFalse()
        {
            Line resultLine = new(3m, -1m, 4m, 7m);
            Arc resultArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_HorizontalLineNoIntersection_ReturnFalse()
        {
            Line resultLine = new(0m, 6m, 4m, 6m);
            Arc resultArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_HorizontalLineOneIntersection_ReturnTrue()
        {
            Line resultLine = new(3m, 2m, 5m, 2m);
            Arc resultArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_VerticalLineNoIntersection_ReturnFalse()
        {
            Line resultLine = new(4m, 0m, 4m, 6m);
            Arc resultArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_VerticalLineTwoIntersections_ReturnTrue()
        {
            Line resultLine = new(3m, -1m, 3m, 6m);
            Arc resultArc = new(1m, 2m, DecimalEx.Sqrt(8m), 315m, 45m);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }
        #endregion

        #region IntersectArcWithArc 
        [Test]
        public void IntersectArcWithArc_HappyPath_ReturnTrue()
        {
            Arc arc1 = new Arc(0.0m, 0.0m, 10m, 270m, 90m);
            Arc arc2 = new Arc(20.0m, 0.0m, 10m, 90m, 280m);
            bool result = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectArcWithArc_NonIntersecting_ReturnFalse()
        {
            Arc arc1 = new Arc(0.0m, 0.0m, 10m, 270m, 90m);
            Arc arc2 = new Arc(25.0m, 0.0m, 10m, 90m, 280m);
            bool result = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectArcWithArc_DoubleIntersect_ReturnTrue()
        {
            Arc arc1 = new Arc(0.0m, 0.0m, 10m, 270m, 90m);
            Arc arc2 = new Arc(19.0m, 0.0m, 10m, 90m, 270m);
            bool result = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(result, Is.True);
        }
        #endregion

        #region IntersectLineWithLine
        [Test]
        public void IntersectLineWithLine_Intersecting_ReturnTrue()
        {
            Line line1 = new(0.0m, 0.0m, 5.0m, 5.0m);
            Line line2 = new(0.0m, 5.0m, 5.0m, 0.0m);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithLine_NotIntersecting_ReturnFalse()
        {
            Line line1 = new(0.0m, 0.0m, 5.0m, 1.0m);
            Line line2 = new(5.0m, 5.0m, 5.0m, 4.0m);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithLine_ParallelLines_ReturnFalse()
        {
            Line line1 = new(0.0m, 0.0m, 5.0m, 0.0m);
            Line line2 = new(0.0m, 5.0m, 5.0m, 5.0m);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.False);
        }
        #endregion
    }
}
