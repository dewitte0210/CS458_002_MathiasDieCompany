using FeatureRecognitionAPI.Models;

namespace Testing_for_Project
{
    internal class EntityClassTests
    {
        #region IsInArcRange 
        [Test]
        public void IsInArcRange_HappyPath_ReturnTrue()
        {
            Arc testArc = new Arc(0, 0, 5, 0, 90);
            Point testPoint = new Point(1, 0);
            bool result = testArc.IsInArcRange(testPoint);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsInArcRange_XisNegativeOne_ReturnTrue()
        {
            Arc testArc = new Arc(0, 0, 5, 90, 180);
            Point testPoint = new Point(-1, 0);
            bool result = testArc.IsInArcRange(testPoint);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsInArcRange_NotInRange_ReturnFalse()
        {
            Arc testArc = new Arc(0, 0, 5, 0, 90);
            Point testPoint = new Point(0, -1);
            bool result = testArc.IsInArcRange(testPoint);
            Assert.That(result, Is.False);
        }
        [Test]
        public void IsInArcRange_StartGreaterThanEnd_ReturnTrue()
        {
            Arc testArc = new Arc(0, 0, 5, 270, 0);
            Point testPoint = new Point(1, 0);
            bool result = testArc.IsInArcRange(testPoint);
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
            Line resultLine = new(-2, -1, 0, 5);
            Arc resultArc = new(1, 2, Math.Sqrt(8), 315, 45);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_LineOneIntersection_ReturnTrue()
        {
            Line resultLine = new(1, 0, 4, 3);
            Arc resultArc = new(1, 2, Math.Sqrt(8), 315, 45);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_LineTwoIntersections_ReturnFalse()
        {
            Line resultLine = new(3, -1, 4, 7);
            Arc resultArc = new(1, 2, Math.Sqrt(8), 315, 45);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_HorizontalLineNoIntersection_ReturnFalse()
        {
            Line resultLine = new(0, 6, 4, 6);
            Arc resultArc = new(1, 2, Math.Sqrt(8), 315, 45);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_HorizontalLineOneIntersection_ReturnTrue()
        {
            Line resultLine = new(3, 2, 5, 2);
            Arc resultArc = new(1, 2, Math.Sqrt(8), 315, 45);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithArc_VerticalLineNoIntersection_ReturnFalse()
        {
            Line resultLine = new(4, 0, 4, 6);
            Arc resultArc = new(1, 2, Math.Sqrt(8), 315, 45);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithArc_VerticalLineTwoIntersections_ReturnTrue()
        {
            Line resultLine = new(3, -1, 3, 6);
            Arc resultArc = new(1, 2, Math.Sqrt(8), 315, 45);
            bool result = resultLine.IntersectLineWithArc(resultLine, resultArc);
            Assert.That(result, Is.True);
        }
        #endregion

        #region IntersectArcWithArc 
        [Test]
        public void IntersectArcWithArc_HappyPath_ReturnTrue()
        {
            Arc arc1 = new Arc(0.0, 0.0, 10, 270, 90);
            Arc arc2 = new Arc(20.0, 0.0, 10, 90, 280);
            bool result = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectArcWithArc_NonIntersecting_ReturnFalse()
        {
            Arc arc1 = new Arc(0.0, 0.0, 10, 270, 90);
            Arc arc2 = new Arc(25.0, 0.0, 10, 90, 280);
            bool result = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectArcWithArc_DoubleIntersect_ReturnTrue()
        {
            Arc arc1 = new Arc(0.0, 0.0, 10, 270, 90);
            Arc arc2 = new Arc(19.0, 0.0, 10, 90, 270);
            bool result = arc1.IntersectArcWithArc(arc1, arc2);
            Assert.That(result, Is.True);
        }
        #endregion

        #region IntersectLineWithLine
        [Test]
        public void IntersectLineWithLine_Intersecting_ReturnTrue()
        {
            Line line1 = new(0.0, 0.0, 5.0, 5.0);
            Line line2 = new(0.0, 5.0, 5.0, 0.0);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithLine_IntersectingWithVerticalLine1_ReturnTrue()
        {
            Line line2 = new(0.0, 0.0, 5.0, 5.0);
            Line line1 = new(2.0, -1.0, 2.0, 6.0);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithLine_IntersectingWithVerticalLine2_ReturnTrue()
        {
            Line line1 = new(0.0, 0.0, 5.0, 5.0);
            Line line2 = new(2.0, -1.0, 2.0, 6.0);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithLine_IntersectingWithHorizontalLine1_ReturnTrue()
        {
            Line line2 = new(0.0, 0.0, 5.0, 5.0);
            Line line1 = new(-1.0, 3.0, 6.0, 3.0);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithLine_IntersectingWithHorizontalLine2_ReturnTrue()
        {
            Line line1 = new(0.0, 0.0, 5.0, 5.0);
            Line line2 = new(-1.0, 3.0, 6.0, 3.0);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IntersectLineWithLine_NotIntersecting_ReturnFalse()
        {
            Line line1 = new(0.0, 0.0, 5.0, 1.0);
            Line line2 = new(5.0, 5.0, 5.0, 4.0);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithLine_ParallelLines_ReturnFalse()
        {
            Line line1 = new(0.0, 0.0, 5.0, 0.0);
            Line line2 = new(0.0, 5.0, 5.0, 5.0);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IntersectLineWithLine_VerticalAndHorizontal_ReturnTrue()
        {
            Line line1 = new(2, 2, 12, 2);
            Line line2 = new(2, 2, 2, 8);
            bool result = line1.IntersectLineWithLine(line1, line2);
            Assert.That(result, Is.True);
        }
        #endregion
    }
}
