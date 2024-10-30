using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureRecognitionAPI.Models;

namespace Testing_for_Project
{
    internal class ExtendLineTests
    {
        [Test]
        public void ExtendVerticalLines() {
            Line line1 = new(7,4,7,6);
            Line line2 = new(7, 8, 7, 10);
            List<Entity> testEntities = [line1, line2];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];
            Assert.IsTrue(finalTestLine.StartX == 7);
            Assert.IsTrue(finalTestLine.EndX == 7);
            Assert.IsTrue(finalTestLine.StartY == 4);
            Assert.IsTrue(finalTestLine.EndY == 10);
        }

        [Test]
        public void ExtendHorizontalLines()
        {
            Line line1 = new(4, 7, 6, 7);
            Line line2 = new(8, 7, 10, 7);
            List<Entity> testEntities = [line1, line2];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];
            Assert.IsTrue(finalTestLine.StartX == 4);
            Assert.IsTrue(finalTestLine.EndX == 10);
            Assert.IsTrue(finalTestLine.StartY == 7);
            Assert.IsTrue(finalTestLine.EndY == 7);
        }

        [Test]
        public void ExtendSlopedLines()
        {
            Line line1 = new(0, 0, 4, 4);
            Line line2 = new(6, 6, 9, 9);
            List<Entity> testEntities = [line1, line2];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];
            Assert.IsTrue(finalTestLine.StartX == 0);
            Assert.IsTrue(finalTestLine.EndX == 9);
            Assert.IsTrue(finalTestLine.StartY == 0);
            Assert.IsTrue(finalTestLine.EndY == 9);
        }

        [Test]
        public void DontExtendLines()
        {
            Line line1 = new(0, 3, 4, 4);
            Line line2 = new(1, 6, 10, 9);
            List<Entity> testEntities = [line1, line2];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 2);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line && testFeature.ExtendedEntityList[1] is Line);
            Line finalTestLine1 = (Line)testFeature.ExtendedEntityList[0];
            Line finalTestLine2 = (Line)testFeature.ExtendedEntityList[1];
            Assert.IsTrue(finalTestLine1 == line1);
            Assert.IsTrue(finalTestLine2 == line2);
        }
    }
}
