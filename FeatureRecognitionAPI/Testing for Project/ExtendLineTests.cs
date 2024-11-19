using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSMath.Geometry;
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

            Assert.IsTrue(finalTestLine.hasPoint(new Point(7, 4)));
            Assert.IsTrue(finalTestLine.hasPoint(new Point(7, 10)));
        }

        [Test]
        public void ExtendThreeVerticalLines()
        {
            Line line1 = new(7, 4, 7, 6);
            Line line2 = new(7, 8, 7, 10);
            Line line3 = new(7, 12, 7, 14);

            List<Entity> testEntities = [line1, line2, line3];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];

            Assert.IsTrue(finalTestLine.hasPoint(new Point(7, 4)));
            Assert.IsTrue(finalTestLine.hasPoint(new Point (7, 14)));
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

            Assert.IsTrue(finalTestLine.hasPoint(new Point(4, 7)));
            Assert.IsTrue(finalTestLine.hasPoint(new Point(10, 7)));
        }

        [Test]
        public void ExtendThreeHorizontalLines()
        {
            Line line1 = new(4, 7, 6, 7);
            Line line2 = new(8, 7, 10, 7);
            Line line3 = new(12, 7, 14, 7);
            List<Entity> testEntities = [line1, line2, line3];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];

            Assert.IsTrue(finalTestLine.hasPoint(new Point(4, 7)));
            Assert.IsTrue(finalTestLine.hasPoint(new Point(14, 7)));
        }

        [Test]
        public void ExtendHorizontalLinesWithPerimeterFeatures()
        {
            Line line1 = new(4, 7, 6, 7);
            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);
            Line line5 = new(8, 7, 10, 7);
            List<Entity> testEntities = [line1, line2, line3, line4, line5];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 4);
            bool hasExtendedLine = false;
            foreach(Entity entity in testFeature.ExtendedEntityList)
            {
                if (entity is ExtendedLine) 
                {

                    Assert.IsTrue(((ExtendedLine)entity).hasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).hasPoint(new Point(10, 7)));
                    hasExtendedLine = true;
                }
            }
            Assert.IsTrue(hasExtendedLine);
            Assert.IsTrue(testFeature.EntityList.Contains(line2));
            Assert.IsTrue(testFeature.EntityList.Contains(line3));
            Assert.IsTrue(testFeature.EntityList.Contains(line4));
        }

        [Test]
        public void ExtendThreeHorizontalLinesWithPerimeterFeatures()
        {
            Line line1 = new(4, 7, 6, 7);

            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);

            Line line5 = new(8, 7, 10, 7);

            Line line6 = new(10, 7, 10, 3);
            Line line7 = new(10, 3, 12, 3);
            Line line8 = new(12, 3, 12, 7);

            Line line9 = new(12, 7, 14, 7);

            List<Entity> testEntities = [line1, line2, line3, line4, line5, line6, line7, line8, line9];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 7);
            bool hasExtendedLine = false;
            foreach (Entity entity in testFeature.ExtendedEntityList)
            {
                if (entity is ExtendedLine)
                {
                    Assert.IsFalse(hasExtendedLine);
                    Assert.IsTrue(((ExtendedLine)entity).hasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).hasPoint(new Point(14, 7)));
                    hasExtendedLine = true;
                }
            }
            Assert.IsTrue(hasExtendedLine);
            Assert.IsTrue(testFeature.EntityList.Contains(line2));
            Assert.IsTrue(testFeature.EntityList.Contains(line3));
            Assert.IsTrue(testFeature.EntityList.Contains(line4));

            Assert.IsTrue(testFeature.EntityList.Contains(line6));
            Assert.IsTrue(testFeature.EntityList.Contains(line7));
            Assert.IsTrue(testFeature.EntityList.Contains(line8));
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
            Assert.IsTrue(finalTestLine.hasPoint(new Point(0, 0)));
            Assert.IsTrue(finalTestLine.hasPoint(new Point(9, 9)));
        }

        [Test]
        public void ExtendThreeSlopedLines()
        {
            Line line1 = new(0, 0, 4, 4);
            Line line2 = new(6, 6, 9, 9);
            Line line3 = new(12, 12, 18, 18);
            List<Entity> testEntities = [line1, line2, line3];
            Feature testFeature = new(testEntities, false, false);
            testFeature.extendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];
            Assert.IsTrue(finalTestLine.hasPoint(new Point(0, 0)));
            Assert.IsTrue(finalTestLine.hasPoint(new Point(18, 18)));
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
        public void example1Entities()
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
            
                Assert.IsTrue(equalLists);//base entity list and normal entity list is the same (only works for example 1)
                Feature testFeature = new Feature(feature.baseEntityList);
                testFeature.DetectFeatures();
                Assert.IsTrue(feature.Equals(testFeature));
            }
        }

        //Example 3 has no perimeter feature so all entities in in BaseEntityList should be in EntityList and not be changed
        [Test]
        public void example3EveryBaseEntityinOriginalList()
        {
            //Set path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-003.dxf";
            DXFFile exampleOne = new DXFFile(path);

            exampleOne.detectAllFeatures();

            List<Feature> featureList = exampleOne.getFeatureList();
            foreach (Feature feature in featureList)
            {
                bool inBaseList = false;
                for (int i = 0; i < feature.baseEntityList.Count; i++)
                {
                    if (feature.EntityList.Contains(feature.ExtendedEntityList[i]))
                    {
                        inBaseList = true;
                    }
                }

                Assert.IsTrue(inBaseList);//checks that every entity in baseEntityList is in entityList
            }
        }

        // Basic test to understand the logic of an if statement when dealing with inheritance
        [Test]
        public void doesChildPassAsParent()
        {
                ExtendedLine child = new ExtendedLine();
                Assert.IsTrue(child is Line);
        }

        // This is to test that all entities in the perimeter list are in EntityList and have not been changed
        // Does not check if the perimeter features are correct
        [Test]
        public void lilBitOfEverythingPerimeterEntitiesInEntityList()
        {
            //Set path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-LilBitOfEverything.dxf";
            DXFFile exampleOne = new DXFFile(path);

            exampleOne.detectAllFeatures();

            List<Feature> featureList = exampleOne.getFeatureList();
            bool hasSquareFeature = false;
            bool hasCircleFeature = false;
            bool oneFeatureWithTwoPerimeterFeatures = false;
            int count = 0; // number of features with 2 perimeter features
            foreach (Feature feature in featureList)
            {
                if (feature.PerimeterEntityList.Count == 2)
                    {
                    oneFeatureWithTwoPerimeterFeatures = true;
                    count++;
                    Assert.IsTrue(count == 1); // there should be only 1 feature with two perimeter features
                    bool inBaseList = false;
                    foreach (List<Entity> perimeterFeatureEntities in feature.PerimeterEntityList)
                    {
                        Feature perimeterFeature = new Feature(perimeterFeatureEntities);
                        foreach (Entity entity in perimeterFeature.EntityList)
                        {
                            if (feature.EntityList.Contains(entity)) { inBaseList = true; }
                            else { inBaseList = false; }
                        }
                        perimeterFeature.CountEntities(perimeterFeature.EntityList, out int numLines, out int numArcs, out int numCircles);
                        Assert.IsTrue(numCircles == 0);
                        if (numLines == 3 && numArcs == 2) { hasSquareFeature = true; }
                        if (numLines == 2 && numArcs == 1) { hasCircleFeature = true; }
                    }

                    Assert.IsTrue(inBaseList);//checks that every entity in baseEntityList is in entityList
                } 
                else
                {
                    Assert.IsTrue(feature.PerimeterEntityList.Count == 0); // if the feature does not have 2 perimeter features, it should have none
                }
            }
            Assert.IsTrue(oneFeatureWithTwoPerimeterFeatures);
            Assert.IsTrue(hasSquareFeature);
            Assert.IsTrue(hasCircleFeature);
        }
        //should have two perimeter features
        #endregion
    }
}
