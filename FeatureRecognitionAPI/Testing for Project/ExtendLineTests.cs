using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Entities;

namespace Testing_for_Project
{
    internal class ExtendLineTests
    {
        #region ExtendingLines
        [Test]
        public void ExtendVerticalLines()
        {
            Line line1 = new(7, 4, 7, 6);
            Line line2 = new(7, 8, 7, 10);
            List<Entity> testEntities = [line1, line2];
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];

            Assert.IsTrue(finalTestLine.HasPoint(new Point(7, 4)));
            Assert.IsTrue(finalTestLine.HasPoint(new Point(7, 10)));
        }

        [Test]
        public void ExtendThreeVerticalLines()
        {
            Line line1 = new(7, 4, 7, 6);
            Line line2 = new(7, 8, 7, 10);
            Line line3 = new(7, 12, 7, 14);

            List<Entity> testEntities = [line1, line2, line3];
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];

            Assert.IsTrue(finalTestLine.HasPoint(new Point(7, 4)));
            Assert.IsTrue(finalTestLine.HasPoint(new Point(7, 14)));
        }

        [Test]
        public void ExtendHorizontalLines()
        {
            Line line1 = new(4, 7, 6, 7);
            Line line2 = new(8, 7, 10, 7);
            List<Entity> testEntities = [line1, line2];
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];

            Assert.IsTrue(finalTestLine.HasPoint(new Point(4, 7)));
            Assert.IsTrue(finalTestLine.HasPoint(new Point(10, 7)));
        }

        [Test]
        public void ExtendThreeHorizontalLines()
        {
            Line line1 = new(4, 7, 6, 7);
            Line line2 = new(8, 7, 10, 7);
            Line line3 = new(12, 7, 14, 7);
            List<Entity> testEntities = [line1, line2, line3];
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];

            Assert.IsTrue(finalTestLine.HasPoint(new Point(4, 7)));
            Assert.IsTrue(finalTestLine.HasPoint(new Point(14, 7)));
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
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            // Checks for ExtendedEntityList
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 4);
            bool hasExtendedLine = false;
            foreach (Entity entity in testFeature.ExtendedEntityList)
            {
                if (entity is ExtendedLine)
                {

                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(10, 7)));
                    hasExtendedLine = true;
                }
            }
            Assert.IsTrue(hasExtendedLine);

            Assert.IsTrue(testFeature.ExtendedEntityList.Contains(line2));
            Assert.IsTrue(testFeature.ExtendedEntityList.Contains(line3));
            Assert.IsTrue(testFeature.ExtendedEntityList.Contains(line4));
            // Checks that EntityList has not changed
            Assert.IsTrue(testFeature.EntityList.Count == 5);
            Assert.IsTrue(testFeature.EntityList.Contains(line1));
            Assert.IsTrue(testFeature.EntityList.Contains(line2));
            Assert.IsTrue(testFeature.EntityList.Contains(line3));
            Assert.IsTrue(testFeature.EntityList.Contains(line4));
            Assert.IsTrue(testFeature.EntityList.Contains(line5));
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
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 7);
            bool hasExtendedLine = false;
            foreach (Entity entity in testFeature.ExtendedEntityList)
            {
                if (entity is ExtendedLine)
                {
                    Assert.IsFalse(hasExtendedLine);
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(14, 7)));
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
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];
            Assert.IsTrue(finalTestLine.HasPoint(new Point(0, 0)));
            Assert.IsTrue(finalTestLine.HasPoint(new Point(9, 9)));
        }

        [Test]
        public void ExtendThreeSlopedLines()
        {
            Line line1 = new(0, 0, 4, 4);
            Line line2 = new(6, 6, 9, 9);
            Line line3 = new(12, 12, 18, 18);
            List<Entity> testEntities = [line1, line2, line3];
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 1);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Line);
            Line finalTestLine = (Line)testFeature.ExtendedEntityList[0];
            Assert.IsTrue(finalTestLine.HasPoint(new Point(0, 0)));
            Assert.IsTrue(finalTestLine.HasPoint(new Point(18, 18)));
        }

        [Test]
        public void DontExtendLines()
        {
            Line line1 = new(0, 3, 4, 4);
            Line line2 = new(1, 6, 10, 9);
            List<Entity> testEntities = [line1, line2];
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
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
            Feature testFeature = new(testEntities, false, 0);
            testFeature.ExtendAllEntities();
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 2);
            Assert.IsTrue(testFeature.ExtendedEntityList[0] is Arc && testFeature.ExtendedEntityList[1] is Circle);
            Arc finalTestLine1 = (Arc)testFeature.ExtendedEntityList[0];
            Circle finalTestLine2 = (Circle)testFeature.ExtendedEntityList[1];
            Assert.IsTrue(finalTestLine1 == arc1);
            Assert.IsTrue(finalTestLine2 == circle1);
        }

        [Test]
        public void ChangeBackAdjList()
        {
            // two Lines that should be extended properly with the adjacency list being changed
            Line line1 = new(0, 0, .5, 0); // has line5
            Line line2 = new(1, 0, 2, 0); // has line3 and line6
            // exLine should have line3, line5, and line6
            
            // filler feature entities
            Line line3 = new(2, 0, 2, 2); // has line4 and exLine 
            Line line4 = new(2, 2, 0, 2); // has line3, line5, and line7
            Line line5 = new(0, 2, 0, 0); // has line4 and exLine
            
            // two lines that will be extended but should be added pack for perimeterList with the adjacency list being changed back
            // These will appear as two separate perimeter features in the end
            Line line6 = new(1, 0, 1, 1); // has exLine
            Line line7 = new(1, 1.5, 1, 2);// has line4

            DXFFile testFile = new (new List<Entity>() { line1, line2, line3, line4, line5, line6, line7 });
            testFile.GroupFeatureEntities();
            testFile.FeatureList[0].ExtendAllEntities();
            // Check lines were extended properly (just to be sure)
            Assert.IsTrue(testFile.FeatureList[0].ExtendedEntityList.Count == 5);
            testFile.FeatureList[0].SeperateBaseEntities();
            testFile.FeatureList[0].SeperatePerimeterEntities();
            Assert.IsTrue(testFile.FeatureList[0].PerimeterFeatureList.Count == 2);
            Assert.IsTrue(testFile.FeatureList[0].PerimeterFeatureList[0].EntityList.Count == 1);
            Assert.IsTrue(testFile.FeatureList[0].PerimeterFeatureList[1].EntityList.Count == 1);

            // AdjList checks:
            Assert.IsTrue(line1.AdjList.Count == 1 && line1.AdjList.Contains(line5));
            Assert.IsTrue(line2.AdjList.Count == 2 && line2.AdjList.Contains(line3) && line2.AdjList.Contains(line6));
            Assert.IsTrue(line3.AdjList.Count == 2 && line3.AdjList.Contains(line4) &&(line3.AdjList[0] is ExtendedLine || line3.AdjList[1] is ExtendedLine));
            // get exLine from line3 (easiest way without changing protection level of baseEntityList)
            ExtendedLine exLine;
            if (line3.AdjList[0] is ExtendedLine)
            {
                exLine = (ExtendedLine)line3.AdjList[0];
            }
            else
            {
                exLine = (ExtendedLine)line3.AdjList[1];
            }
            Assert.IsTrue(line4.AdjList.Count == 3 && line4.AdjList.Contains(line3) && line4.AdjList.Contains(line5) && line4.AdjList.Contains(line7));
            Assert.IsTrue(line5.AdjList.Count == 2 && line5.AdjList.Contains(line4) && line5.AdjList.Contains(exLine));
            Assert.IsTrue(line6.AdjList.Count == 1 && line6.AdjList.Contains(exLine));
            Assert.IsTrue(line7.AdjList.Count == 1 && line7.AdjList.Contains(line4));
            
            // exLine AdjList check
            Assert.IsTrue(exLine.AdjList.Count == 3 && exLine.AdjList.Contains(line3) && exLine.AdjList.Contains(line5) && exLine.AdjList.Contains(line6));
        }
        #endregion

        #region seperationTests

        [Test]
        public void SeperateBaseEntitiesWithOnePerimeterFeature() // Horizontal lines
        {
            // Extend Lines
            // Added lines to make the feature a closed shape
            Line line6 = new(4, 7, 4, 3);
            Line line7 = new(4, 3, 10, 3);
            Line line8 = new(10, 3, 10, 7);
            // Pretty much the same as a test above
            Line line1 = new(4, 7, 6, 7);
            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);
            Line line5 = new(8, 7, 10, 7);
            List<Entity> testEntities = [line1, line2, line3, line4, line5, line6, line7, line8];
            Feature testFeature = new(testEntities, false, 0);

            testFeature.ExtendAllEntities();

            // Checks for ExtendedEntityList
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 7);
            bool hasExtendedLine = false;
            foreach (Entity entity in testFeature.ExtendedEntityList)
            {
                if (entity is ExtendedLine)
                {
                    Assert.IsFalse(hasExtendedLine); // Checks if only one extended line
                    // Checks correct point values
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(10, 7)));
                    // Checks correct parent values
                    Assert.IsTrue(((ExtendedLine)entity).Parent1.Equals(line1) || ((ExtendedLine)entity).Parent1.Equals(line5));
                    Assert.IsTrue(((ExtendedLine)entity).Parent2.Equals(line1) || ((ExtendedLine)entity).Parent2.Equals(line5));

                    hasExtendedLine = true;
                }
                else
                {
                    Assert.IsTrue(entity is Line); // if not extended line it must be a line
                                                   // Technically an extended line would pass here but it would be caught above

                    Assert.IsTrue(testEntities.Contains(entity)); // If entity is not an extended line it should be contained in testEntities
                }
            }
            Assert.IsTrue(hasExtendedLine);

            // Checks that EntityList did not change
            Assert.IsTrue(testFeature.EntityList.Equals(testEntities));
            Assert.IsTrue(testFeature.EntityList.Count == 8);

            // Seperate base entities
            testFeature.SeperateBaseEntities();

            // Make sure these still pass
            // Checks for ExtendedEntityList
            Assert.IsTrue(testFeature.ExtendedEntityList.Count == 7);
            hasExtendedLine = false;
            foreach (Entity entity in testFeature.ExtendedEntityList)
            {
                if (entity is ExtendedLine)
                {
                    Assert.IsFalse(hasExtendedLine); // Checks if only one extended line
                    // Checks correct point values
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(10, 7)));
                    // Checks correct parent values
                    Assert.IsTrue(((ExtendedLine)entity).Parent1.Equals(line1) || ((ExtendedLine)entity).Parent1.Equals(line5));
                    Assert.IsTrue(((ExtendedLine)entity).Parent2.Equals(line1) || ((ExtendedLine)entity).Parent2.Equals(line5));

                    hasExtendedLine = true;
                }
                else
                {
                    Assert.IsTrue(entity is Line); // if not extended line it must be a line
                                                   // Technically an extended line would pass here but it would be caught above

                    Assert.IsTrue(testEntities.Contains(entity)); // If entity is not an extended line it should be contained in testEntities
                }
            }
            Assert.IsTrue(hasExtendedLine);

            // Checks that EntityList did not change
            Assert.IsTrue(testFeature.EntityList.Equals(testEntities));
            Assert.IsTrue(testFeature.EntityList.Count == 8);

            // Checks for baseEntityList
            hasExtendedLine = false;
            foreach (Entity entity in testFeature.baseEntityList)
            {
                Assert.IsTrue(testFeature.ExtendedEntityList.Contains(entity));
                if (entity is ExtendedLine)
                {
                    Assert.IsFalse(hasExtendedLine);
                    // Checks correct point values
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(10, 7)));
                    // Checks correct parent values
                    Assert.IsTrue(((ExtendedLine)entity).Parent1.Equals(line1) || ((ExtendedLine)entity).Parent1.Equals(line5));
                    Assert.IsTrue(((ExtendedLine)entity).Parent2.Equals(line1) || ((ExtendedLine)entity).Parent2.Equals(line5));

                    hasExtendedLine = true;
                }
            }
            Assert.IsTrue(hasExtendedLine);
        }

        [Test]
        public void SeperateBaseEntitiesWithTwoPerimeterFeature() // Horizontal lines
        {
            // Added lines to make the feature a closed shape
            Line line10 = new(4, 7, 4, 0);
            Line line11 = new(4, 0, 14, 0);
            Line line12 = new(14, 0, 14, 7);
            // Pretty much the same as a test above
            Line line1 = new(4, 7, 6, 7);

            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);

            Line line5 = new(8, 7, 10, 7);

            Line line6 = new(10, 7, 10, 3);
            Line line7 = new(10, 3, 12, 3);
            Line line8 = new(12, 3, 12, 7);

            Line line9 = new(12, 7, 14, 7);
            List<Entity> testEntities = [line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12];
            Feature testFeature = new(testEntities, false, 0);

            testFeature.ExtendAllEntities();

            testFeature.SeperateBaseEntities();

            // Checks for baseEntityList
            bool hasExtendedLine = false;
            foreach (Entity entity in testFeature.baseEntityList)
            {
                Assert.IsTrue(testFeature.ExtendedEntityList.Contains(entity));
                if (entity is ExtendedLine)
                {
                    Assert.IsFalse(hasExtendedLine); // Checks if only one extended line
                    // Checks correct point values
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(14, 7)));
                    // Checks correct parent values
                    ExtendedLine extendedLineParent = new ExtendedLine();
                    Line lineParent = new ExtendedLine();

                    if (((ExtendedLine)entity).Parent1 is ExtendedLine)
                    {
                        extendedLineParent = (ExtendedLine)((ExtendedLine)entity).Parent1;
                        lineParent = ((ExtendedLine)entity).Parent2;
                    }
                    else if (((ExtendedLine)entity).Parent2 is ExtendedLine)
                    {
                        extendedLineParent = (ExtendedLine)((ExtendedLine)entity).Parent2;
                        lineParent = ((ExtendedLine)entity).Parent1;
                    }
                    else { Assert.IsTrue(false); } // if neither is an ExtendedLine test fails

                    Assert.IsFalse(lineParent is ExtendedLine);

                    if ((extendedLineParent.Parent1.Equals(line1) && extendedLineParent.Parent2.Equals(line5)) ||
                        (extendedLineParent.Parent1.Equals(line5) && extendedLineParent.Parent2.Equals(line1)))
                    {
                        // entity Parent1 is an extended line and has parents of line1 and line5, which means entity parent 2 must be line9
                        Assert.IsTrue(lineParent.Equals(line9));
                    }
                    else if ((extendedLineParent.Parent1.Equals(line5) && extendedLineParent.Parent2.Equals(line9)) ||
                        (extendedLineParent.Parent1.Equals(line9) && extendedLineParent.Parent2.Equals(line5)))
                    {
                        // entity Parent1 is an extended line and has parents of line5 and line9, which means entity parent 2 must be line1
                        Assert.IsTrue(lineParent.Equals(line1));
                    }
                    else { Assert.IsTrue(false); } // if it does not meet either conditions test fails

                    hasExtendedLine = true;
                }
                else
                {
                    Assert.IsTrue(entity is Line); // if not extended line it must be a line
                                                   // Technically an extended line would pass here but it would be caught above

                    Assert.IsTrue(testEntities.Contains(entity)); // If entity is not an extended line it should be contained in testEntities
                }
            }
            Assert.IsTrue(hasExtendedLine);
        }

        [Test]
        public void SeperateBaseEntitiesWithTwoParallelPerimeterFeature() // Horizontal lines
        {
            // Added lines to make the feature a closed shape
            Line line10 = new(4, 7, 4, 0);
            Line line11 = new(4, 0, 14, 0);
            Line line12 = new(14, 0, 14, 7);
            // Pretty much the same as a test above
            Line line1 = new(4, 7, 6, 7);

            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);

            Line line5 = new(8, 7, 10, 7);

            Line line6 = new(10, 7, 10, 4);
            Line line7 = new(10, 4, 12, 4);
            Line line8 = new(12, 4, 12, 7);

            Line line9 = new(12, 7, 14, 7);
            List<Entity> testEntities = [line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12];
            Feature testFeature = new(testEntities, false, 0);

            testFeature.ExtendAllEntities();

            testFeature.SeperateBaseEntities();

            // Checks for baseEntityList
            bool hasExtendedLine = false;
            foreach (Entity entity in testFeature.baseEntityList)
            {
                Assert.IsTrue(testFeature.ExtendedEntityList.Contains(entity));
                if (entity is ExtendedLine)
                {
                    Assert.IsFalse(hasExtendedLine); // Checks if only one extended line
                    // Checks correct point values
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(4, 7)));
                    Assert.IsTrue(((ExtendedLine)entity).HasPoint(new Point(14, 7)));
                    // Checks correct parent values
                    ExtendedLine extendedLineParent = new ExtendedLine();
                    Line lineParent = new ExtendedLine();

                    if (((ExtendedLine)entity).Parent1 is ExtendedLine)
                    {
                        extendedLineParent = (ExtendedLine)((ExtendedLine)entity).Parent1;
                        lineParent = ((ExtendedLine)entity).Parent2;
                    }
                    else if (((ExtendedLine)entity).Parent2 is ExtendedLine)
                    {
                        extendedLineParent = (ExtendedLine)((ExtendedLine)entity).Parent2;
                        lineParent = ((ExtendedLine)entity).Parent1;
                    }
                    else { Assert.IsTrue(false); } // if neither is an ExtendedLine test fails

                    Assert.IsFalse(lineParent is ExtendedLine);

                    if ((extendedLineParent.Parent1.Equals(line1) && extendedLineParent.Parent2.Equals(line5)) ||
                        (extendedLineParent.Parent1.Equals(line5) && extendedLineParent.Parent2.Equals(line1)))
                    {
                        // entity Parent1 is an extended line and has parents of line1 and line5, which means entity parent 2 must be line9
                        Assert.IsTrue(lineParent.Equals(line9));
                    }
                    else if ((extendedLineParent.Parent1.Equals(line5) && extendedLineParent.Parent2.Equals(line9)) ||
                        (extendedLineParent.Parent1.Equals(line9) && extendedLineParent.Parent2.Equals(line5)))
                    {
                        // entity Parent1 is an extended line and has parents of line5 and line9, which means entity parent 2 must be line1
                        Assert.IsTrue(lineParent.Equals(line1));
                    }
                    else { Assert.IsTrue(false); } // if it does not meet either conditions test fails

                    hasExtendedLine = true;
                }
                else
                {
                    Assert.IsTrue(entity is Line); // if not extended line it must be a line
                                                   // Technically an extended line would pass here but it would be caught above

                    Assert.IsTrue(testEntities.Contains(entity)); // If entity is not an extended line it should be contained in testEntities
                }
            }
            Assert.IsTrue(hasExtendedLine);
        }

        [Test]
        public void SeperateOnePerimeterFeatures()
        {
            // Extend Lines
            // Added lines to make the feature a closed shape
            Line line6 = new(4, 7, 4, 3);
            Line line7 = new(4, 3, 10, 3);
            Line line8 = new(10, 3, 10, 7);
            // Pretty much the same as a test above
            Line line1 = new(4, 7, 6, 7);

            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);

            Line line5 = new(8, 7, 10, 7);
            List<Entity> testEntities = [line1, line2, line3, line4, line5, line6, line7, line8];
            Feature testFeature = new(testEntities, false, 0);

            testFeature.ExtendAllEntities();

            testFeature.SeperateBaseEntities();

            testFeature.SeperatePerimeterEntities();

            Assert.IsTrue(testFeature.PerimeterFeatureList.Count == 1);
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Count == 3);

            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line2));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line3));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line4));
        }

        [Test]
        public void SeperateTwoPerimeterFeatures()
        {
            // Added lines to make the feature a closed shape
            Line line10 = new(4, 7, 4, 0);
            Line line11 = new(4, 0, 14, 0);
            Line line12 = new(14, 0, 14, 7);
            // Pretty much the same as a test above
            Line line1 = new(4, 7, 6, 7);

            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);

            Line line5 = new(8, 7, 10, 7);

            Line line6 = new(10, 7, 10, 3);
            Line line7 = new(10, 3, 12, 3);
            Line line8 = new(12, 3, 12, 7);

            Line line9 = new(12, 7, 14, 7);
            List<Entity> testEntities = [line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12];
            Feature testFeature = new(testEntities, false, 0);

            testFeature.ExtendAllEntities();

            testFeature.SeperateBaseEntities();

            testFeature.SeperatePerimeterEntities();

            Assert.IsTrue(testFeature.PerimeterFeatureList.Count == 2);
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Count == 3 && testFeature.PerimeterFeatureList[1].EntityList.Count == 3);

            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line2) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line2));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line3) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line3));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line4) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line4));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line6) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line6));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line7) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line7));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line8) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line8));

            Assert.IsTrue(
                (testFeature.PerimeterFeatureList[0].EntityList.Contains(line2)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line3)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line4)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line6)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line7)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line8))
                ||
                ((testFeature.PerimeterFeatureList[1].EntityList.Contains(line2)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line3)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line4)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line6)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line7)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line8)))
                );
        }

        [Test]
        public void SeperateTwoParallelPerimeterFeatures()
        {
            // Added lines to make the feature a closed shape
            Line line10 = new(4, 7, 4, 0);
            Line line11 = new(4, 0, 14, 0);
            Line line12 = new(14, 0, 14, 7);
            // Pretty much the same as a test above
            Line line1 = new(4, 7, 6, 7);

            Line line2 = new(6, 7, 6, 4);
            Line line3 = new(6, 4, 8, 4);
            Line line4 = new(8, 4, 8, 7);

            Line line5 = new(8, 7, 10, 7);

            Line line6 = new(10, 7, 10, 4);
            Line line7 = new(10, 4, 12, 4);
            Line line8 = new(12, 4, 12, 7);

            Line line9 = new(12, 7, 14, 7);
            List<Entity> testEntities = [line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12];
            Feature testFeature = new(testEntities, false, 0);

            testFeature.ExtendAllEntities();

            testFeature.SeperateBaseEntities();

            testFeature.SeperatePerimeterEntities();

            Assert.IsTrue(testFeature.PerimeterFeatureList.Count == 2);
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Count == 3 && testFeature.PerimeterFeatureList[1].EntityList.Count == 3);

            // Checks that the needed lines are contained in either perimeter feature
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line2) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line2));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line3) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line3));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line4) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line4));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line6) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line6));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line7) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line7));
            Assert.IsTrue(testFeature.PerimeterFeatureList[0].EntityList.Contains(line8) || testFeature.PerimeterFeatureList[1].EntityList.Contains(line8));

            // Check that the entities are grouped together
            Assert.IsTrue(
                (testFeature.PerimeterFeatureList[0].EntityList.Contains(line2)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line3)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line4)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line6)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line7)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line8))
                ||
                ((testFeature.PerimeterFeatureList[1].EntityList.Contains(line2)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line3)
                && testFeature.PerimeterFeatureList[1].EntityList.Contains(line4)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line6)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line7)
                && testFeature.PerimeterFeatureList[0].EntityList.Contains(line8)))
                );
        }
        #endregion

        #region exampleFileTests
        [Test]
        public void example1Entities()
        {
            //Set Path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-001.dxf";
            DXFFile exampleOne = new DXFFile(path);

            exampleOne.DetectAllFeatureTypes();

            List<Feature> featureList = exampleOne.FeatureList;
            for (int i = 0; i < 8; i++) // Only the firt 8 features should fulfill this since the
                                        // later half will be recognized as equal features and wont
                                        // have feature detection ran on them, meaning the
                                        // baseEntityList wont be initialized
            {
                bool equalLists = false;
                if (featureList[i].EntityList.Count == featureList[i].baseEntityList.Count)
                {
                    for (int j = 0; j < featureList[i].EntityList.Count; j++)
                    {
                        if (featureList[i].baseEntityList.Contains(featureList[i].EntityList[j]))
                        {
                            equalLists = true;
                        }
                    }
                }

                Assert.IsTrue(equalLists);//base entity list and normal entity list is the same (only works for example 1)
                Feature testFeature = new Feature(featureList[i].baseEntityList);
                testFeature.DetectFeatures();
                Assert.IsTrue(featureList[i].Equals(testFeature));
            }
        }

        //Example 3 has no perimeter feature so all entities in BaseEntityList should be in EntityList and not be changed
        [Test]
        public void example3EveryBaseEntityinOriginalList()
        {
            //Set Path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-003.dxf";
            DXFFile exampleOne = new DXFFile(path);
            
            exampleOne.GroupFeatureEntities();
            // same as SupportedFile.DetectAllFeatureTypes besides perimeter feature stuff because that destroys ExtendedEntityList
            foreach (Feature feature in exampleOne.FeatureList)
            {
                feature.ExtendAllEntities();
                feature.SeperateBaseEntities();
            }

            List<Feature> featureList = exampleOne.FeatureList;
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

                Assert.IsTrue(inBaseList);//checks that every entity in baseEntityList is in EntityList
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
            //Set Path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-LilBitOfEverything.dxf";
            DXFFile exampleOne = new DXFFile(path);

            exampleOne.DetectAllFeatureTypes();

            List<Feature> featureList = exampleOne.FeatureList;
            bool hasSquareFeature = false;
            bool hasCircleFeature = false;
            bool oneFeatureWithTwoPerimeterFeatures = false;
            int count = 0; // number of features with 2 perimeter features
            foreach (Feature feature in featureList)
            {
                if (feature.PerimeterFeatureList.Count == 2)
                {
                    oneFeatureWithTwoPerimeterFeatures = true;
                    count++;
                    Assert.IsTrue(count == 1); // there should be only 1 feature with two perimeter features
                    bool inBaseList = false;
                    foreach (Feature perimeterFeature in feature.PerimeterFeatureList)
                    {
                        foreach (Entity entity in perimeterFeature.EntityList)
                        {
                            if (feature.EntityList.Contains(entity)) { inBaseList = true; }
                            else { inBaseList = false; }
                        }
                        perimeterFeature.CountEntities(perimeterFeature.EntityList, out int numLines, out int numArcs, out int numCircles, out int numEllipses);
                        Assert.IsTrue(numCircles == 0 && numEllipses == 0);
                        if (numLines == 3 && numArcs == 2) { hasSquareFeature = true; }
                        if (numLines == 2 && numArcs == 1) { hasCircleFeature = true; }
                    }

                    Assert.IsTrue(inBaseList);//checks that every entity in baseEntityList is in EntityList
                }
                else
                {
                    Assert.IsTrue(feature.PerimeterFeatureList.Count == 0); // if the feature does not have 2 perimeter features, it should have none
                }
            }
            Assert.IsTrue(oneFeatureWithTwoPerimeterFeatures);
            Assert.IsTrue(hasSquareFeature);
            Assert.IsTrue(hasCircleFeature);
            //should have two perimeter features
        }

        [Test]
        public void Example2PerimeterFeatures() // This is working without chamfered corners being added as their own feature
        {
            //Set Path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-002.dxf";
            DXFFile exampleTwo = new DXFFile(path);

            exampleTwo.DetectAllFeatureTypes();

            Assert.IsTrue(exampleTwo.FeatureGroups.Count == 1); // one overall feature when combined
            Assert.IsTrue(exampleTwo.FeatureGroups[0].NumIdenticalFeatureGroups == 7); // 6 identical features
            List<Feature> singleFeatureList = exampleTwo.FeatureGroups[0].GetFeatures();

            Assert.IsTrue(singleFeatureList.Count == 7); // 6 features make up the die
            
            int totalFeatures = 0;
            foreach (Feature feature in singleFeatureList)
            {
                totalFeatures += feature.count;
            }
            totalFeatures *= exampleTwo.FeatureGroups[0].NumIdenticalFeatureGroups;
            Assert.IsTrue(totalFeatures == 63); // number of features accounting for combined features
        }

        #endregion
    }
}
