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
        #region ExtendingLines
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
            Assert.IsTrue(finalTestLine.StartPoint.X == 7);
            Assert.IsTrue(finalTestLine.EndPoint.X == 7);
            Assert.IsTrue(finalTestLine.StartPoint.Y == 4);
            Assert.IsTrue(finalTestLine.EndPoint.Y == 10);
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
            Assert.IsTrue(finalTestLine.StartPoint.X == 4);
            Assert.IsTrue(finalTestLine.EndPoint.X == 10);
            Assert.IsTrue(finalTestLine.StartPoint.Y == 7);
            Assert.IsTrue(finalTestLine.EndPoint.Y == 7);
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
            Assert.IsTrue(finalTestLine.StartPoint.X == 0);
            Assert.IsTrue(finalTestLine.EndPoint.X == 9);
            Assert.IsTrue(finalTestLine.StartPoint.Y == 0);
            Assert.IsTrue(finalTestLine.EndPoint.Y == 9);
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

        [Test]
        public void InvalidEntities()
        {
            Arc arc1 = new Arc(1, 2, Math.Sqrt(8), 315, 45);
            Circle circle1 = new(0, 0, 2);
            List<Entity> testEntities = [arc1, circle1];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 2);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Arc && testFeature.ExtendedEntityList[1] is Circle);
            Arc finalTestLine1 = (Arc)testFeature.ExtendedEntityList[0];
            Circle finalTestLine2 = (Circle)testFeature.ExtendedEntityList[1];
            Assert.IsTrue(finalTestLine1 == arc1);
            Assert.IsTrue(finalTestLine2 == circle1);
        }
        #endregion

        #region SortIntoBaseEntityList
        [Test]
        public void averageCaseEntities()
        {
            //Set path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-001.dxf";
            DXFFile exampleOne = new DXFFile(path);

            exampleOne.detectAllFeatures();

            List<Feature> featureList = exampleOne.getFeatureList();
            foreach (Feature feature in featureList)
            {
                bool equalLists = false;
                if (feature.EntityList.Count == feature.baseEntityList.Count)
                {
                    for (int i = 0; i < feature.EntityList.Count; i++)
                    {
                        if (feature.baseEntityList.Contains(feature.EntityList[i]))
                        {
                            equalLists = true;
                        }
                    }
                }
            
                Assert.IsTrue(equalLists);
                Feature testFeature = new Feature(feature.baseEntityList);
                testFeature.DetectFeatures();
                Assert.IsTrue(feature.Equals(testFeature));
            }
        }
        #endregion
    }
}
