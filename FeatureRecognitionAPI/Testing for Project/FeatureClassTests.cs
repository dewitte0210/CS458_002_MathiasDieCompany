using FeatureRecognitionAPI.Controllers;
using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;
using FeatureRecognitionAPI.Models.Utility;
using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Http;

namespace Testing_for_Project
{
    internal class FeatureClassTests
    {

        #region CheckGroup1B
        [Test]
        public void CheckGroup1B_Circle_ReturnsTrue()
        {
            Circle circle = new(0.0, 0.0, 10.0);
            List<Entity> entities = new List<Entity>() { circle };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group1B1));
        }

        [Test]
        public void CheckGroup1B_RoundedRectangle_ReturnsTrue()
        {
            Arc arc1 = new(0.0, 0.0, 5.0, 90.0, 270.0);
            Arc arc2 = new(10.0, 0.0, 5.0, 270.0, 90.0);
            Line line1 = new(0.0, 5.0, 10.0, 5.0);
            Line line2 = new(0.0, -5.0, 10.0, -5.0);
            List<Entity> entities = new List<Entity>() { arc1, arc2, line1, line2 };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group1B2));
        }

        [Test]
        public void CheckGroup1B_BadFeature_ReturnsFalse()
        {
            Arc arc = new(0.0, 0.0, 5.0, 90.0, 270.0);
            Circle circle = new(0.0, 0.0, 5.0);
            List<Entity> entities = new List<Entity> { arc, circle };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Unknown));
        }
        #endregion

        #region CheckGroup1C

        [Test]
        //Make sure a standard triangle is detected as a triangle
        public void CheckGroup1C_Good()
        {
            Line line1 = new(1, 1, 1, 3);
            Line line2 = new(1, 3, 3, 2);
            Line line3 = new(1, 1, 3, 2);

            List<Entity> entities = new List<Entity>() { line1, line2, line3 };

            Feature feature = new Feature(entities);
            bool isTriangle = feature.CheckGroup1C();

            Assert.That(isTriangle, Is.True);
        }

        [Test]
        public void CheckGroup1C_Bad()
        {
            //Square
            Line line1 = new(1, 1, 1, 3);
            Line line2 = new(3, 1, 1, 1);
            Line line3 = new(3, 3, 1, 3);
            Line line4 = new(3, 3, 3, 1);

            Feature square = new Feature(new List<Entity>() { line1, line2, line3, line4 });
            PossibleFeatureTypes test;
            bool squareCheck = square.CheckGroup1C();

            //Circle
            Circle circle1 = new(1, 1, 4.5);

            Feature circle = new Feature(new List<Entity>() { circle1 });
            bool circleCheck = circle.CheckGroup1C();

            //3 Arcs + 3 lines that are not triangle
            Line line5 = new(1, 1, 1, 4);
            Line line6 = new(3, 1, 1, 1);
            Line line7 = new(4, 3, 4, 1);
            Arc arc1 = new(1.5, 3, .5, 0, 180);
            Arc arc2 = new(2.5, 3, .5, 0, 180);
            Arc arc3 = new(3.5, 3, .5, 0, 180);


            Feature fakeTriangle = new Feature(new List<Entity>() { line5, line6, line7, arc1, arc2, arc3 });
            fakeTriangle.baseEntityList = fakeTriangle.EntityList;
            bool fakeCheck = fakeTriangle.CheckGroup1C();
            //Assert all are expected

            Assert.That(squareCheck, Is.False);
            Assert.That(circleCheck, Is.False);

            //This edge case shouldn't be detected as a triangle
            Assert.That(fakeCheck, Is.False);
        }

        [Test]
        public void CheckGroup1C_Example3()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-003.dxf";

            DXFFile example3 = new DXFFile(path);

            example3.SetFeatureGroups();

            List<FeatureGroup> fGroups = example3.FeatureGroups;

            List<Feature> fList = fGroups[0].GetFeatures();

            Feature baseShape = fList[1];

            PossibleFeatureTypes pType;
            Assert.IsTrue(baseShape.CheckGroup1C());

        }

        #endregion

        #region CheckGroup2A
        [Test]
        public void CheckGroup2A_EllipseMadeOfArcs_ReturnsTrue()
        {
            Arc arc1 = new(1.1399417701703984, 1.8531050558148150, 0.1680136484100009, 0.9555661934363535, 36.8885215710321290);
            Arc arc2 = new(1.0323926156114558, 1.7720412332222049, 0.302691490489563, 36.9411035321518781, 61.1167419575615725);
            Arc arc3 = new(0.9150298597915126, 1.5697777604261320, 0.5365077762296626, 60.5758363679647545, 77.0406837634733819);
            Arc arc4 = new(0.8679928269291335, 1.3844270898032700, 0.7276985077886839, 76.7042919975651216, 90.1280108914693869);
            Arc arc5 = new(0.8647411648347978, 1.3844270898032696, 0.7276985077886844, 89.8719891085306273, 103.2957080024348642);
            Arc arc6 = new(0.8177041319724192, 1.5697777604261309, 0.5365077762296641, 102.9593162365266465, 119.4241636320352455);
            Arc arc7 = new(0.7003413761524744, 1.7720412332222060, 0.3026914904895616, 118.8832580424383423, 143.0588964678481716);
            Arc arc8 = new(0.5927922215935331, 1.8531050558148148, 0.1680136484100012, 143.1114784289678710, 179.0444338065635748);
            Arc arc9 = new(0.5927922215935328, 1.8587089841050546, 0.1680136484100009, 180.9555661934363684, 216.8885215710321006);
            Arc arc10 = new(0.7003413761524755, 1.9397728066976647, 0.3026914904895631, 216.9411035321518568, 241.1167419575615725);
            Arc arc11 = new(0.8177041319724196, 2.1420362794937402, 0.5365077762296656, 240.5758363679647402, 257.0406837634733392);
            Arc arc12 = new(0.8647411648347973, 2.3273869501165962, 0.7276985077886806, 256.7042919975651216, 270.1280108914693869);
            Arc arc13 = new(0.8679928269291336, 2.3273869501165985, 0.7276985077886831, 269.8719891085306131, 283.2957080024348784);
            Arc arc14 = new(0.9150298597915123, 2.1420362794937384, 0.5365077762296634, 282.9593162365266608, 299.4241636320352313);
            Arc arc15 = new(1.0323926156114558, 1.9397728066976647, 0.3026914904895630, 298.8832580424383991, 323.0588964678481148);
            Arc arc16 = new(1.1399417701703980, 1.8587089841050548, 0.1680136484100013, 323.1114784289678710, 359.0444338065635748);
            List<Entity> entities = new List<Entity>() { arc1, arc5, arc7, arc9, arc2, arc4, arc3, arc16, arc11, arc10, arc12, arc6, arc13, arc8, arc14, arc15 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group2A1));
        }

        [Test]
        public void CheckGroup2A_EllipseMadeOfArcs_ReturnsFalse()
        {
            Arc arc1 = new(1.1399417701703984, 1.8531050558148150, 0.1680136484100009, 0.9555661934363535, 36.8885215710321290);
            Arc arc2 = new(1.0323926156114558, 1.7720412332222049, 0.302691490489563, 36.9411035321518781, 61.1167419575615725);
            Arc arc3 = new(0.9150298597915126, 1.5697777604261320, 0.5365077762296626, 60.5758363679647545, 77.0406837634733819);
            Arc arc4 = new(0.8679928269291335, 1.3844270898032700, 0.7276985077886839, 76.7042919975651216, 90.1280108914693869);
            Arc arc5 = new(0.8647411648347978, 1.3844270898032696, 0.7276985077886844, 89.8719891085306273, 103.2957080024348642);
            Arc arc6 = new(0.8177041319724192, 1.5697777604261309, 0.5365077762296641, 102.9593162365266465, 119.4241636320352455);
            Arc arc7 = new(0.7003413761524744, 1.7720412332222060, 0.3026914904895616, 118.8832580424383423, 143.0588964678481716);
            Arc arc8 = new(0.5927922215935331, 1.8531050558148148, 0.1680136484100012, 143.1114784289678710, 179.0444338065635748);
            Arc arc9 = new(0.5927922215935328, 1.8587089841050546, 0.1680136484100009, 180.9555661934363684, 216.8885215710321006);
            Arc arc10 = new(0.7003413761524755, 1.9397728066976647, 0.3026914904895631, 216.9411035321518568, 241.1167419575615725);
            Arc arc11 = new(0.8177041319724196, 2.1420362794937402, 0.5365077762296656, 240.5758363679647402, 257.0406837634733392);
            Arc arc12 = new(0.8647411648347973, 2.3273869501165962, 0.7276985077886806, 256.7042919975651216, 270.1280108914693869);
            Arc arc13 = new(0.8679928269291336, 2.3273869501165985, 0.7276985077886831, 269.8719891085306131, 283.2957080024348784);
            Arc arc14 = new(0.9150298597915123, 2.1420362794937384, 0.5365077762296634, 282.9593162365266608, 299.4241636320352313);
            Arc arc15 = new(1.0323926156114558, 1.9397728066976647, 0.3026914904895630, 298.8832580424383991, 323.0588964678481148);
            Arc arc16 = new(1.1399417701703980, 1.8587089841050548, 0.5, 323.1114784289678710, 359.0444338065635748);
            List<Entity> entities = new List<Entity>() { arc1, arc5, arc7, arc9, arc2, arc4, arc3, arc16, arc11, arc10, arc12, arc6, arc13, arc8, arc14, arc15 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Unknown));
        }

        [Test]
        public void CheckGroup2A_EllipseFromEllipseClass_ReturnsTrue()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, 2 * Math.PI);
            List<Entity> entities = new List<Entity>() { ellipse1 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group2A1));
        }

        [Test]
        public void CheckGroup2A_EllipseFromEllipseClass_ReturnsFalse()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, Math.PI);
            List<Entity> entities = new List<Entity>() { ellipse1 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Unknown));
        }
        #endregion

        #region CheckGroup3

        [Test]
        public void TestGetLinesFromEntityListSquare()
        {
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 5);
            Line line3 = new Line(5, 5, 0, 5);
            Line line4 = new Line(0, 5, 0, 0);
            List<Entity> ll = [line1, line2, line3, line4];

            List<Line> fromLL = Feature.GetLinesFromEntityList(ll);
            
            Assert.That(fromLL.Count, Is.EqualTo(4));
            Assert.Contains(line1, fromLL);
            Assert.Contains(line2, fromLL);
            Assert.Contains(line3, fromLL);
            Assert.Contains(line4, fromLL);
        }
        
        [Test]
        public void TestGetLinesFromEntityListSquareWithArcs()
        {
            Line line1 = new Line(1, 0, 4, 0);
            Arc a1 = new Arc(0.5, 0.5, 0.5, 0.5, 0.5);
            Line line2 = new Line(4, 0, 5, 5);
            Arc a2 = new Arc(0.5, 0.5, 0.5, 0.5, 0.5);
            Line line3 = new Line(5, 5, 0, 5);
            Arc a3 = new Arc(0.5, 0.5, 0.5, 0.5, 0.5);
            Line line4 = new Line(0, 5, 0, 0);
            Arc a4 = new Arc(0.5, 0.5, 0.5, 0.5, 0.5);
            List<Entity> eList = [line1, a1, line2, a2, line3, a3, line4, a4];

            List<Line> ll = Feature.GetLinesFromEntityList(eList);
            
            Assert.That(ll.Count, Is.EqualTo(4));
            Assert.Contains(line1, ll);
            Assert.Contains(line2, ll);
            Assert.Contains(line3, ll);
            Assert.Contains(line4, ll);
        }

        [Test]
        public void TestGetTouchingLineNoTouching()
        {
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(99, 99, 100, 100);
            List<Line> ll = [line1, line2];

            Line? touchingLine = Feature.GetTouchingLine(line1, ll).Item1;
            Assert.That(touchingLine, Is.EqualTo(null));
        }
        
        [Test]
        public void TestGetTouchingLineOneTouching()
        {
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 5);
            List<Line> ll = [line1, line2];

            Line? touchingLine = Feature.GetTouchingLine(line1, ll).Item1;
            Assert.That(touchingLine, Is.Not.Null);
        }

        [Test]
        public void TestGetOrderedLinesSquare()
        {
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 5);
            Line line3 = new Line(5, 5, 0, 5);
            Line line4 = new Line(0, 5, 0, 0);
            List<Line> ll = [line1, line2, line3, line4];
            
            List<List<Line>> orderedLines = Feature.GetOrderedLines(ll);
            
            Assert.That(orderedLines.Count, Is.EqualTo(1));
            Assert.That(orderedLines[0].Count, Is.EqualTo(4));
            Assert.That(orderedLines[0], Is.EqualTo(ll));
        }
        
        [Test]
        public void TestGetOrderedLinesTwoSquare()
        {
            Line line1A = new Line(0, 0, 5, 0);
            Line line2A = new Line(5, 0, 5, 5);
            Line line3A = new Line(5, 5, 0, 5);
            Line line4A = new Line(0, 5, 0, 0);
            List<Line> lla = [line1A, line2A, line3A, line4A];
            
            Line line1B = new Line(10, 10, 15, 10);
            Line line2B = new Line(15, 10, 15, 15);
            Line line3B = new Line(15, 15, 10, 15);
            Line line4B = new Line(10, 15, 10, 10);
            List<Line> llb = [line1B, line2B, line3B, line4B];
            
            List<Line> ll = [line1A, line2A, line3A, line4A, line1B, line2B, line3B, line4B];
            List<List<Line>> orderedLines = Feature.GetOrderedLines(ll);
            
            Assert.That(orderedLines.Count, Is.EqualTo(2));
            Assert.That(orderedLines[0].Count, Is.EqualTo(4));
            Assert.That(orderedLines[1].Count, Is.EqualTo(4));
            Assert.That(orderedLines[0], Is.EqualTo(lla));
            Assert.That(orderedLines[1], Is.EqualTo(llb));
        }
        
        /*
        [Test]
        public void TestGetPossibleChamfersOneChamfer()
        {
            // counterclockwise
            // one chamfer in top left corner
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 5);
            Line line3 = new Line(5, 5, 2, 5);
            Line lineChamfer = new Line(2, 5, 0, 3);
            Line line5 = new Line(0, 3, 0, 0);
            List<Line> ll = [line1, line2, line3, lineChamfer, line5];
            
            List<Line> possibleChamList = Feature.SetPossibleChamfers(Feature.GetOrderedLines(ll));
            
            Assert.That(possibleChamList.Count, Is.EqualTo(1));
            Assert.That(possibleChamList[0], Is.EqualTo(lineChamfer));
        }
        
        [Test]
        public void TestGetPossibleChamfersThreeChamfer()
        {
            // counterclockwise
            // two chamfer in top left and right corner
            // and top line should be recognized as well
            // so 3 possible chamfers total
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 3);
            Line lineCham3 = new Line(5, 3, 3, 5);
            Line line4 = new Line(3, 5, 2, 5);
            Line lineCham5 = new Line(2, 5, 0, 3);
            Line line6 = new Line(0, 3, 0, 0);
            List<Line> ll = [line1, line2, lineCham3, line4, lineCham5, line6];
            
            List<Line> possibleChamList = Feature.SetPossibleChamfers(Feature.GetOrderedLines(ll));
            
            Assert.That(possibleChamList.Count, Is.EqualTo(3));
            Assert.Contains(lineCham3, possibleChamList);
            Assert.Contains(line4, possibleChamList);
            Assert.Contains(lineCham5, possibleChamList);
        }
        
        [Test]
        public void TestGetPossibleChamfersEightChamfer()
        {
            // counterclockwise
            // one chamfer on each corner of a square
            // like an octagon
            // each line could be a possible chamfer so
            // list size should be eight
            Line line1 = new Line(2, 0, 3, 0);
            Line line2 = new Line(3, 0, 5, 2);
            Line line3 = new Line(5, 2, 5, 3);
            Line line4 = new Line(5, 3, 3, 5);
            Line line5 = new Line(3, 5, 2, 5);
            Line line6 = new Line(2, 5, 0, 3);
            Line line7 = new Line(0, 3, 0, 2);
            Line line8 = new Line(0, 2, 2, 0);
            List<Line> ll = [line1, line2, line3, line4, line5, line6, line7, line8];
            
            List<Line> possibleChamList = Feature.SetPossibleChamfers(Feature.GetOrderedLines(ll));
            
            Assert.That(possibleChamList.Count, Is.EqualTo(8));
            Assert.Contains(line1, possibleChamList);
            Assert.Contains(line2, possibleChamList);
            Assert.Contains(line3, possibleChamList);
            Assert.Contains(line4, possibleChamList);
            Assert.Contains(line5, possibleChamList);
            Assert.Contains(line6, possibleChamList);
            Assert.Contains(line7, possibleChamList);
            Assert.Contains(line8, possibleChamList);
        }
        */
        
        [Test]
        public void CheckGroup3NoChamferSquare()
        {
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 5);
            Line line3 = new Line(5, 5, 0, 5);
            Line line4 = new Line(0, 5, 0, 0);
            List<Entity> eList = [line1, line2, line3, line4];
            Feature f = new(eList);
            
            Assert.That(f.ChamferList.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void CheckGroup3OneChamfer()
        {
            // counterclockwise
            // one chamfer in top left corner
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 5);
            Line line3 = new Line(5, 5, 2, 5);
            Line lineCham = new Line(2, 5, 0, 3);
            Line line5 = new Line(0, 3, 0, 0);
            List<Entity> eList = [line1, line2, line3, lineCham, line5];
            Feature f = new(eList);
            
            //detects all groups including group3
            f.DetectFeatures();
            
            Assert.That(f.ChamferList.Count, Is.EqualTo(1));
            //Assert.That(f.ChamferList[0].ChamferIndex, Is.EqualTo(lineCham));
        }
        
        [Test]
        public void CheckGroup3TwoCornerChamfer()
        {
            // counterclockwise
            // two chamfer in top left and right corner
            // and top line should be recognized as well
            // so 3 possible chamfers total but 2 confirmed
            Line line1 = new Line(0, 0, 5, 0);
            Line line2 = new Line(5, 0, 5, 3);
            Line lineCham3 = new Line(5, 3, 3, 5);
            Line line4 = new Line(3, 5, 2, 5);
            Line lineCham5 = new Line(2, 5, 0, 3);
            Line line6 = new Line(0, 3, 0, 0);
            List<Entity> eList = [line1, line2, lineCham3, line4, lineCham5, line6];
            Feature f = new(eList);
            
            //detects all groups including group3
            f.DetectFeatures();
            
            Assert.That(f.ChamferList.Count, Is.EqualTo(2));
            //Assert.That(f.ChamferList[0].ChamferIndex, Is.EqualTo(lineCham3));
            //Assert.That(f.ChamferList[1].ChamferIndex, Is.EqualTo(lineCham5));
        }
        
        [Test]
        public void CheckGroup3Octagon()
        {
            // counterclockwise
            // one chamfer on each corner of a square like an octagon
            // each line could be a possible chamfer but confirm 4
            Line line1 = new Line(2, 0, 3, 0);
            Line line2 = new Line(3, 0, 5, 2);
            Line line3 = new Line(5, 2, 5, 3);
            Line line4 = new Line(5, 3, 3, 5);
            Line line5 = new Line(3, 5, 2, 5);
            Line line6 = new Line(2, 5, 0, 3);
            Line line7 = new Line(0, 3, 0, 2);
            Line line8 = new Line(0, 2, 2, 0);
            List<Entity> eList = [line1, line2, line3, line4, line5, line6, line7, line8];
            Feature f = new(eList);
            
            // detects all groups including group3
            f.DetectFeatures();
            
            Assert.That(f.ChamferList.Count, Is.EqualTo(8));
        }

        [Test]
        public void CheckGroup3NoChamferFromFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\test-files\\square.dxf";
            DXFFile squareFile = new DXFFile(path);
            squareFile.DetectAllFeatureTypes();

            Assert.That(squareFile.FeatureList[0].ChamferList.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void CheckGroup3OneChamferFromFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\test-files\\one-chamfer-square.dxf";
            DXFFile squareFile = new DXFFile(path);
            squareFile.DetectAllFeatureTypes();

            int numChamfers = 0;
            foreach (FeatureGroup fg in squareFile.FeatureGroups)
            {
                numChamfers += GetNumChamferFeatures(fg.GetFeatures());
            }
            
            Assert.That(GetNumChamferFeatures(squareFile.FeatureGroups), Is.EqualTo(1));
            foreach (Entity entity in squareFile.FeatureGroups[0].GetFeatures()[0].EntityList)
            {
                // todo: actually test for this when getLength function is implemented
                //Assert.That(entity.);
            }
        }
        
        [Test]
        public void CheckGroup3TwoChamfersWithRadiusesFromFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\test-files\\square-two-radius-two-chamfer.dxf";
            DXFFile squareFile = new DXFFile(path);
            squareFile.DetectAllFeatureTypes();

            int numChamfers = 0;
            foreach (FeatureGroup fg in squareFile.FeatureGroups)
            {
                numChamfers += GetNumChamferFeatures(fg.GetFeatures());
            }
            
            Assert.That(numChamfers, Is.EqualTo(2));
        }

        private static int GetNumChamferFeatures(List<Feature> featureList)
        {
            int numChamfers = 0;
            foreach (Feature f in featureList)
            {
                if (f.FeatureType == PossibleFeatureTypes.Group3)
                {
                    numChamfers++;
                }
            }
            return numChamfers;
        }
        
        private static int GetNumChamferFeatures(List<FeatureGroup> featureGroupList)
        {
            int numChamfers = 0;
            foreach (FeatureGroup f in featureGroupList)
            {
                numChamfers += GetNumChamferFeatures(f.GetFeatures());
            }
            return numChamfers;
        }
        
        [Test]
        public void CheckGroup3ExampleOneFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\ExampleFiles\\Example-001.dxf";
            DXFFile exampleFile = new DXFFile(path);
            exampleFile.DetectAllFeatureTypes();

            Assert.That(GetNumChamferFeatures(exampleFile.FeatureGroups), Is.EqualTo(0));
        }
        
        [Test]
        public void CheckGroup3ExampleTwoFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\ExampleFiles\\Example-002-single-pattern.dxf";
            DXFFile exampleFile = new DXFFile(path);
            exampleFile.DetectAllFeatureTypes();
            int numChamfers = GetNumChamferFeatures(exampleFile.FeatureGroups);
            
            Assert.That(numChamfers, Is.EqualTo(1));
        }
        
        [Test]
        public void CheckGroup3ExampleThreeFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\ExampleFiles\\Example-003.dxf";
            DXFFile exampleFile = new DXFFile(path);
            exampleFile.DetectAllFeatureTypes();
            
            Assert.That(GetNumChamferFeatures(exampleFile.FeatureGroups), Is.EqualTo(0));
        }
        
        [Test]
        public void CheckGroup3ExampleFourFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\ExampleFiles\\Example-004.dxf";
            DXFFile exampleFile = new DXFFile(path);
            exampleFile.DetectAllFeatureTypes();
            
            Assert.That(GetNumChamferFeatures(exampleFile.FeatureGroups), Is.EqualTo(0));
        }
        
        [Test]
        public void CheckGroup3ExampleEverythingFile()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) 
                          + "FeatureRecognitionAPI\\ExampleFiles\\Example-LilBitOfEverything.dxf";
            DXFFile exampleFile = new DXFFile(path);
            exampleFile.DetectAllFeatureTypes();

            Assert.That(GetNumChamferFeatures(exampleFile.FeatureGroups), Is.EqualTo(1));
        }

        #endregion

        #region CheckGroup5
        [Test]
        public void CheckGroup5_3LineCompartment_ReturnsTrue()
        {
            Line line1 = new(0.0, 0.0, 0.0, 5.0);
            Line line2 = new(0.0, 5.0, 5.0, 5.0);
            Line line3 = new(5.0, 5.0, 5.0, 0.0);
            List<Entity> entities = new List<Entity>() { line1, line2, line3 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature>() { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group5));
        }

        [Test]
        public void CheckGroup5_2Line1Arc_ReturnsTrue()
        {
            Line line1 = new(-1.0, 0.0, -1.0, 5.0);
            Arc arc1 = new(0.0, 0.0, 1.0, 180, 360);
            Line line2 = new(1.0, 0.0, 1.0, 5.0);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature> { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group5));
        }

        [Test]
        public void CheckGroup5_3Line2Arc_ReturnsTrue()
        {
            Line line1 = new(0.0, 1.0, 0.0, 5.0);
            Arc arc1 = new(1.0, 1.0, 1.0, 180, 270);
            Line line2 = new(1.0, 0.0, 4.0, 0.0);
            Arc arc2 = new(4.0, 1.0, 1.0, 270, 360);
            Line line3 = new(5.0, 1.0, 5.0, 5.0);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2, line3 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature>() { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group5));
        }
        #endregion

        #region CheckGroup4
        [Test]
        public void CheckGroup4_2LineAngled_ReturnsTrue()
        {
            Line line1 = new(0.0, 1.0, 1.0, 0.0);
            Line line2 = new(1.0, 0.0, 2.0, 1.0);
            List<Entity> entities = new List<Entity>() { line1, line2 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature> { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group4));
        }

        [Test]
        public void CheckGroup4_2Arc2Line_ReturnsTrue()
        {
            Arc arc1 = new(0.0, 3.0, 2, 0, 90);
            Line line1 = new(1.0, 2.0, 1.0, 1.0);
            Line line2 = new(1.0, 1.0, 2.0, 1.0);
            Arc arc2 = new(3.0, 0.0, 2, 0, 90);
            List<Entity> entities = new List<Entity> { arc1, line1, line2, arc2 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature> { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)}};
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group4));
        }

        [Test]
        public void CheckGroup4_2LineNotAngled_ReturnsTrue()
        {
            Line line1 = new(0.0, 1.0, 0.0, 0.0);
            Line line2 = new(0.0, 0.0, 1.0, 0.0);
            List<Entity> entities = new List<Entity> { line1, line2 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature> { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group4));
        }
        #endregion

        #region CheckGroup6

        [Test]
        public void CheckGroup6_4Arc3Line_ReturnsTrue()
        {
            Arc arc1 = new(0.0, 3.0, 1, 0, 90);
            Line line1 = new(1.0, 3.0, 1.0, 1.0);
            Arc arc2 = new(2.0, 1.0, 1, 180, 270);
            Line line2 = new(2.0, 0.0, 3.0, 0.0);
            Arc arc3 = new(3.0, 1.0, 1.0, 270, 0);
            Line line3 = new(4.0, 1.0, 4.0, 3.0);
            Arc arc4 = new(4.0, 3.0, 1.0, 90, 180);
            List<Entity> entities = new List<Entity>() { arc1, arc2, arc3, arc4, line1, line2, line3 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature> { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group6));
        }

        [Test]
        public void CheckGroup6_3Arc2Line_ReturnsTrue()
        {
            Arc arc1 = new(0.0, 3.0, 1, 0, 90);
            Line line1 = new(1.0, 3.0, 1.0, 1.0);
            Arc arc2 = new(2.0, 1.0, 1.0, 180, 0);
            Line line2 = new(3.0, 1.0, 3.0, 3.0);
            Arc arc3 = new(4.0, 3.0, 1.0, 90, 180);
            List<Entity> entities = new List<Entity>() { arc1, arc2, arc3, line1, line2 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature> { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.EqualTo(PossibleFeatureTypes.Group6));
        }

        [Test]
        public void CheckGroup6_Group5Feature_ReturnsFalse()
        {
            Line line1 = new(0.0, 1.0, 0.0, 5.0);
            Arc arc1 = new(1.0, 1.0, 1.0, 180, 270);
            Line line2 = new(1.0, 0.0, 4.0, 0.0);
            Arc arc2 = new(4.0, 1.0, 1.0, 270, 360);
            Line line3 = new(5.0, 1.0, 5.0, 5.0);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2, line3 };
            Feature testFeature = new(entities) { PerimeterFeatureList = new List<Feature> { new(entities) }, baseEntityList = new List<Entity>() {new Line(5.0, 5.0, 10.0, 10.0)} };
            testFeature.DetectFeatures();
            Assert.That(testFeature.PerimeterFeatureList[0].FeatureType, Is.Not.EqualTo(PossibleFeatureTypes.Group6));
        }
        #endregion

        #region CheckGroup10
        [Test]
        public void CheckGroup10_ReturnsTrue()
        {
            Line line1a = new Line(3, 5, 3, 6);
            Line line1b = new Line(3, 1, 3, 0);
            Arc arc1a = new Arc(3, 3, 2, 270, 90);
            Arc arc1b = new Arc(3, 3, 3, 270, 90);
            List<Entity> entities1 = new List<Entity>() { line1a, line1b, arc1a, arc1b };
            Feature feature1 = new(entities1) { baseEntityList = entities1 };
            feature1.DetectFeatures();

            Line line2a = new Line(3, 5, 3, 6);
            Line line2b = new Line(0, 3, 1, 3);
            Arc arc2a = new Arc(3, 3, 2, 180, 90);
            Arc arc2b = new Arc(3, 3, 3, 180, 90);
            List<Entity> entities2 = new List<Entity>() { line2a, line2b, arc2a, arc2b };
            Feature feature2 = new(entities2) { baseEntityList = entities2 };
            feature2.DetectFeatures();

            Line line3a = new Line(0, 3, 1, 3);
            Line line3b = new Line(3, 1, 3, 0);
            Arc arc3a = new Arc(3, 3, 2, 270, 180);
            Arc arc3b = new Arc(3, 3, 3, 270, 180);
            List<Entity> entities3 = new List<Entity>() { line3a, line3b, arc3a, arc3b };
            Feature feature3 = new(entities3) { baseEntityList = entities3 };
            feature3.DetectFeatures();

            Line line4a = new Line(0, 3, 1, 3);
            Line line4b = new Line(5, 3, 6, 3);
            Arc arc4a = new Arc(3, 3, 2, 0, 180);
            Arc arc4b = new Arc(3, 3, 3, 0, 180);
            List<Entity> entities4 = new List<Entity>() { line4a, line4b, arc4a, arc4b };
            Feature feature4 = new(entities4) { baseEntityList = entities4 };
            feature4.DetectFeatures();

            Assert.That(feature1.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group10));
            Assert.That(feature2.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group10));
            Assert.That(feature3.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group10));
            Assert.That(feature4.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group10));
        }
        #endregion

        [Test]
        public void TestIsSubshapeRectangle_ReturnsTrue()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(0, 4, 5, 4);
            Arc arc1 = new(0, 2, 2, 90, 270);
            Arc arc2 = new(5, 2, 2, 270, 90);
            List<Entity> entities = new List<Entity>() { line1, line2, arc1, arc2 };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.IsSubshapeRectangle(), Is.True);
        }

        [Test]
        public void TestIsSubshapeRectangle_ReturnsFalse()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(2, 4, 7, 4);
            Arc arc1 = new(0, 2, 2, 90, 270);
            Arc arc2 = new(5, 2, 2, 270, 90);
            List<Entity> entities = new List<Entity>() { line1, line2, arc1, arc2 };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.IsSubshapeRectangle(), Is.False);
        }

        [Test]

        public void TestSortedFeatureList()
        {
            Line line1 = new(4, 5, 4, 0);
            Line line2 = new(0, 0, 0, 5);
            Line line3 = new(4, 0, 2, -2);
            Arc arc1 = new(2, 5, 2, 0, 180);
            Line line4 = new(2, -2, 0, 0);
            List<Entity> entities = new List<Entity>() { line1, line2, line3, arc1, line4 };
            Feature testFeature = new(entities) { ExtendedEntityList = entities };
            bool testBool = testFeature.SeperateBaseEntities();
            Assert.That(testBool, Is.True);
        }
    }
}
