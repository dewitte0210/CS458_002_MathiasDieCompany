using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Utility;
using NuGet.Frameworks;

namespace Testing_for_Project
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }
        #region EntityClassTests
        [Test]
        public void TestLineClass()
        {
            Line line1 = new Line(1.9724753611322341, 0.4071346127707478, 2.6124753611322347, 0.4071346127707472);

            Assert.IsTrue(line1.HasPoint(new Point(1.9724753611322341, 0.4071346127707478)));
            Assert.IsTrue(line1.HasPoint(new Point(2.6124753611322347, 0.4071346127707472)));
        }
        [Test]
        public void TestArcClass()
        {
            Arc arc1 = new Arc(2.6124753611322347, 0.4371346127707472, 0.03, 270, 0.0000000000002120);
            Assert.That(arc1.Start.X, Is.EqualTo((0.03 * Math.Cos(270 * Math.PI / 180) + 2.6124753611322347)));
            Assert.That(arc1.Start.Y, Is.EqualTo((0.03 * Math.Sin(270 * Math.PI / 180) + 0.4371346127707472)));
            Assert.That(arc1.End.X, Is.EqualTo((0.03 * Math.Cos(0.0000000000002120 * Math.PI / 180) + 2.6124753611322347)));
            Assert.That(arc1.End.Y, Is.EqualTo((0.03 * Math.Sin(0.0000000000002120 * Math.PI / 180) + 0.4371346127707472)));
            Assert.That(arc1.Radius, Is.EqualTo(0.03));
            Assert.That(arc1.Center.X, Is.EqualTo(2.6124753611322347));
            Assert.That(arc1.Center.Y, Is.EqualTo(0.4371346127707472));
            Assert.That(arc1.StartAngle, Is.EqualTo(270));
            Assert.That(arc1.EndAngle, Is.EqualTo(0.0000000000002120));
            Assert.That(arc1.CentralAngle, Is.EqualTo(0.0000000000002120 - 270 + 360));
        }
        [Test]
        public void TestCircleClass()
        {
            Circle circle1 = new Circle(0.7124999999999999, 0.7124999999999999, 0.2577228596164672);
            Assert.That(circle1.Center.X, Is.EqualTo(0.7124999999999999));
            Assert.That(circle1.Center.Y, Is.EqualTo(0.7124999999999999));
            Assert.That(circle1.Radius, Is.EqualTo(0.2577228596164672));
        }

        #region EllipseClass
        [Test]
        public void TestFullEllipseClass()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, 2 * Math.PI);
            Assert.That(ellipse1.GetLength(), Is.EqualTo(15.865439589290595));
        }

        [Test]
        public void TestPartialEllipseClass()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, Math.PI);
            Assert.That(Math.Round(ellipse1.GetLength(), 3), Is.EqualTo(Math.Round(15.865439589290595 / 2, 3)));
        }

        [Test]
        public void TestPartialRotatedEllipseClass()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 0, 3, 2.0 / 3.0, 0, Math.PI);
            Assert.That(Math.Round(ellipse1.GetLength(), 3), Is.EqualTo(Math.Round(15.865439589290595 / 2, 3)));
        }

        [Test]
        public void TestIsInEllipseRange_ReturnsTrue()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, Math.PI);
            Point point1 = new Point(0, 2);
            Assert.That(ellipse1.IsInEllipseRange(point1), Is.True);
        }
        [Test]
        public void TestIsInEllipseRangeForRotatedEllipse_ReturnsTrue()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 0, 3, 2.0 / 3.0, 0, Math.PI);
            Point point1 = new Point(-2, 0);
            Assert.That(ellipse1.IsInEllipseRange(point1), Is.True);
        }
        [Test]
        public void TestIsInEllipseRange_ReturnsFalse()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, Math.PI);
            Point point1 = new Point(0, -2);
            Assert.That(ellipse1.IsInEllipseRange(point1), Is.False);
        }
        [Test]
        public void TestPointOnEllipseGivenAngleInRadians_ReturnsTrue()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, Math.PI);

        }

        [Test]
        public void TestEllipseBoundsNotRotated()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 3, 0, 2.0 / 3.0, 0, 2 * Math.PI);
            Assert.That(ellipse1.MinX(), Is.EqualTo(-3));
            Assert.That(ellipse1.MinY(), Is.EqualTo(-2));
            Assert.That(ellipse1.MaxX(), Is.EqualTo(3));
            Assert.That(ellipse1.MaxY(), Is.EqualTo(2));
        }

        [Test]
        public void TestEllipseBoundsRotated90Degrees()
        {
            Ellipse ellipse1 = new Ellipse(0, 0, 0, 3, 2.0 / 3.0, 0, 2 * Math.PI);
            Assert.That(ellipse1.MinX(), Is.EqualTo(-2));
            Assert.That(ellipse1.MinY(), Is.EqualTo(-3));
            Assert.That(ellipse1.MaxX(), Is.EqualTo(2));
            Assert.That(ellipse1.MaxY(), Is.EqualTo(3));
        }

        [Test]
        public void TestEllipseBoundsRotated45Degrees()
        {
            Ellipse ellipse1 = new Ellipse(1, 1, 3 / Math.Sqrt(2), 3 / Math.Sqrt(2), 2.0 / 3.0, 0, 2 * Math.PI);
            Assert.That(Math.Round(ellipse1.MinX(), 4), Is.EqualTo(-1.5495));
            Assert.That(Math.Round(ellipse1.MinY(), 4), Is.EqualTo(-1.5495));
            Assert.That(Math.Round(ellipse1.MaxX(), 4), Is.EqualTo(3.5495));
            Assert.That(Math.Round(ellipse1.MaxY(), 4), Is.EqualTo(3.5495));
        }

        #endregion

        [Test]
        public void TestPointEquals()
        {
            Point point1 = new Point(2, 3);
            Point point2 = new Point(2, 3);
            Assert.That(point1.Equals(point2));
        }

        [Test]
        public void TestQuadraticFormula()
        {
            Line test = new Line(0, 0, 0, 0);
            List<double> solutions1 = new List<double>() { 2, 5 };
            List<double> solutions2 = new List<double>() { 5, 2 };
            List<double> actual = MdcMath.QuadraticFormula(1, -7, 10);
            Assert.That((actual[0] == solutions1[0] && actual[1] == solutions1[1]) || (actual[0] == solutions2[0] && actual[1] == solutions2[1]));
        }

        [Test]
        public void TestVectorFromCenterWithArc()
        {
            Arc test = new Arc(1, 1, 2, 0, 180);
            Assert.That(test.VectorFromCenter(Math.PI / 2).Equals(new Line(1, 1, 1, 3)));
        }
        #endregion

        [Test]

        public void TestParallelLines()
        {
            //Parralel lines with start and end points oposite of eachother have the same slope (but will be positive/negative)
            Line line1 = new(1, 1, 3, 3);
            Line line2 = new(3, 3, 1, 1);
            //bool check1 = line1.isParallel(line2);
            bool check1 = Angles.IsParallel(line1, line2);
            Assert.IsTrue(check1);

            //Same x different Y
            Line line3 = new(1, 1, 1, 3);
            Line line4 = new(1, 3, 1, 1);
            //bool check2 = line3.isParallel(line4);
            bool check2 = Angles.IsParallel(line3, line4);
            Assert.IsTrue(check2);
        }

        #region TestingDXF&DWG
        [Test]
        public void TestDXFFileClassGoodInput()
        {
            //Set Path to any filepath containing the 3rd example dxf file
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-003.dxf";

            bool pathTest = File.Exists(path);

            //Make sure the file exists so that DXF can be created
            Assert.That(pathTest, Is.True);
            DXFFile testFile = new DXFFile(path);

            //Thirteen known entities in Example-003.dxf that must be verified to have been accurately ingested
            List<Entity> testList = testFile.GetEntities();
            Assert.That(testList[0] is Arc);
            Arc test1 = (Arc)testList[0];

            Assert.That(test1.Center.X, Is.EqualTo(11.2650987394999991));
            Assert.That(test1.Center.Y, Is.EqualTo(0.7549998982999999));
            Assert.That(test1.Radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test1.StartAngle, Is.EqualTo(270.0000000000000000));
            Assert.That(test1.EndAngle, Is.EqualTo(30.0000000000000036));

            Assert.That(testList[1] is Line);
            Line test2 = (Line)testList[1];

            Assert.That(test2.Start.X, Is.EqualTo(11.2650987394999991));
            Assert.That(test2.Start.Y, Is.EqualTo(0.0049998983000000));
            Assert.That(test2.End.X, Is.EqualTo(6.0146749507999999));
            Assert.That(test2.End.Y, Is.EqualTo(0.0049998983000000));

            Assert.That(testList[2] is Arc);
            Arc test3 = (Arc)testList[2];

            Assert.That(test3.Center.X, Is.EqualTo(6.0146749507999999));
            Assert.That(test3.Center.Y, Is.EqualTo(0.7549998982999998));
            Assert.That(test3.Radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test3.StartAngle, Is.EqualTo(149.9999999999999716));
            Assert.That(test3.EndAngle, Is.EqualTo(270.0000000000000000));

            Assert.That(testList[3] is Circle);
            Circle test4 = (Circle)testList[3];

            Assert.That(test4.Center.X, Is.EqualTo(3.3752118942999996));
            Assert.That(test4.Center.Y, Is.EqualTo(2.6843885206000002));
            Assert.That(test4.Radius, Is.EqualTo(1.0000000000000000));

            Assert.That(testList[4] is Arc);
            Arc test5 = (Arc)testList[4];

            Assert.That(test5.Center.X, Is.EqualTo(6.0004237886999992));
            Assert.That(test5.Center.Y, Is.EqualTo(4.2000000000000011));
            Assert.That(test5.Radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test5.StartAngle, Is.EqualTo(329.9990346727777819));
            Assert.That(test5.EndAngle, Is.EqualTo(90.0000000000000000));

            Assert.That(testList[5] is Line);
            Line test6 = (Line)testList[5];

            Assert.That(test6.Start.X, Is.EqualTo(6.0004237886999992));
            Assert.That(test6.Start.Y, Is.EqualTo(4.9500000000000011));
            Assert.That(test6.End.X, Is.EqualTo(0.7500000000000000));
            Assert.That(test6.End.Y, Is.EqualTo(4.9500000000000011));

            Assert.That(testList[6] is Arc);
            Arc test7 = (Arc)testList[6];

            Assert.That(test7.Center.X, Is.EqualTo(0.7499999999999996));
            Assert.That(test7.Center.Y, Is.EqualTo(4.2000000000000011));
            Assert.That(test7.Radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test7.StartAngle, Is.EqualTo(89.9999999999999716));
            Assert.That(test7.EndAngle, Is.EqualTo(210.0009653272222181));

            Assert.That(testList[7] is Circle);
            Circle test8 = (Circle)testList[7];

            Assert.That(test8.Center.X, Is.EqualTo(8.6398868451999995));
            Assert.That(test8.Center.Y, Is.EqualTo(2.2706113776999994));
            Assert.That(test8.Radius, Is.EqualTo(1.0000000000000000));

            Assert.That(testList[8] is Line);
            Line test9 = (Line)testList[8];

            Assert.IsTrue(test9.HasPoint(new Point(5.3651558979999994, 1.1299998982999999)));
            Assert.IsTrue(test9.HasPoint(new Point(7.0403281640286135, 4.0313704835897957)));

            Assert.That(testList[9] is Arc);
            Arc test10 = (Arc)testList[9];

            Assert.That(test10.Center.X, Is.EqualTo(8.6398868451999995));
            Assert.That(test10.Center.Y, Is.EqualTo(3.1078289152999994));
            Assert.That(test10.Radius, Is.EqualTo(1.8470292371454000));
            Assert.That(test10.StartAngle, Is.EqualTo(30.0009653272222003));
            Assert.That(test10.EndAngle, Is.EqualTo(149.9990346727777535));

            Assert.That(testList[10] is Line);
            Line test11 = (Line)testList[10];

            Assert.IsTrue(test11.HasPoint(new Point(10.2394455263713855, 4.0313704835897939)));
            Assert.IsTrue(test11.HasPoint(new Point(11.9146177922999996, 1.1299998982999999)));

            Assert.That(testList[11] is Line);
            Line test12 = (Line)testList[11];

            Assert.That(test12.Start.X, Is.EqualTo(6.6499365233942473));
            Assert.That(test12.Start.Y, Is.EqualTo(3.8249890568663383));
            Assert.That(test12.End.X, Is.EqualTo(4.9747705754328919));
            Assert.That(test12.End.Y, Is.EqualTo(0.9236294146435339));

            Assert.That(testList[12] is Arc);
            Arc test13 = (Arc)testList[12];

            Assert.That(test13.Center.X, Is.EqualTo(3.3752118942999996));
            Assert.That(test13.Center.Y, Is.EqualTo(1.8471709829999998));
            Assert.That(test13.Radius, Is.EqualTo(1.8470292371454000));
            Assert.That(test13.StartAngle, Is.EqualTo(210.0009653296103238));
            Assert.That(test13.EndAngle, Is.EqualTo(329.9990346703896762));

            Assert.That(testList[13] is Line);
            Line test14 = (Line)testList[13];

            Assert.That(test14.Start.X, Is.EqualTo(1.7756532131671074));
            Assert.That(test14.Start.Y, Is.EqualTo(0.9236294146435343));
            Assert.That(test14.End.X, Is.EqualTo(0.1004872653057516));
            Assert.That(test14.End.Y, Is.EqualTo(3.8249890568663383));

        }

        [Test]
        public void TestDWGFileClassGoodInput()
        {
            //Set Path to any filepath containing the 3rd example dxf file
            // string Path = "C:\\Users\\ice-haskinss0550\\Source\\Repos\\CS458_002_MathiasDieCompany\\FeatureRecognitionAPI\\FeatureRecognitionAPI\\ExampleFiles\\Example-001.dwg";

            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-001.dwg";

            bool pathTest = File.Exists(path);

            //Make sure the file exists so that DXF can be created
            Assert.That(pathTest, Is.True);
            DWGFile testFile = new DWGFile(path);

            //Thirteen known entities in Example-003.dxf that must be verified to have been accurately ingested
            List<Entity> testList = testFile.GetEntities();
            Assert.That(testList[0] is Line);
            Line test1 = (Line)testList[0];

            Assert.IsTrue(test1.HasPoint(new Point(1.9724753611322341, 0.4071346127707478)));
            Assert.IsTrue(test1.HasPoint(new Point(2.6124753611322347, 0.4071346127707472)));

            Assert.That(testList[1] is Arc);
            Arc test2 = (Arc)testList[1];

            Assert.That(test2.Center.X, Is.EqualTo(2.6124753611322347));
            Assert.That(test2.Center.Y, Is.EqualTo(0.4371346127707472));
            Assert.That(test2.Radius, Is.EqualTo(0.0300000000000000));
            Assert.That(test2.StartAngle, Is.EqualTo(270.0000000000000000));
            Assert.That(test2.EndAngle, Is.EqualTo(0.0000000000002120));

            Assert.That(testList[2] is Line);
            Line test3 = (Line)testList[2];

            Assert.IsTrue(test3.HasPoint(new Point(2.6424753611322358, 0.4371346127707473)));
            Assert.IsTrue(test3.HasPoint(new Point(2.6424753611322340, 0.9771346127707479)));

            Assert.That(testList[3] is Arc);
            Arc test4 = (Arc)testList[3];

            Assert.That(test4.Center.X, Is.EqualTo(2.6124753611322347));
            Assert.That(test4.Center.Y, Is.EqualTo(0.9771346127707479));
            Assert.That(test4.Radius, Is.EqualTo(0.0300000000000000));
            Assert.That(test4.StartAngle, Is.EqualTo(0.0000000000000000));
            Assert.That(test4.EndAngle, Is.EqualTo(90.0000000000000000));

            Assert.That(testList[4] is Line);
            Line test5 = (Line)testList[4];

            Assert.That(test5.Start.X, Is.EqualTo(2.6124753611322347));
            Assert.That(test5.Start.Y, Is.EqualTo(1.0071346127707477));
            Assert.That(test5.End.X, Is.EqualTo(1.9724753611322341));
            Assert.That(test5.End.Y, Is.EqualTo(1.0071346127707477));


            Assert.That(testList[5] is Arc);
            Arc test6 = (Arc)testList[5];

            Assert.That(test6.Center.X, Is.EqualTo(1.9724753611322341));
            Assert.That(test6.Center.Y, Is.EqualTo(0.9771346127707479));
            Assert.That(test6.Radius, Is.EqualTo(0.0300000000000000));
            Assert.That(test6.StartAngle, Is.EqualTo(90.0000000000000000));
            Assert.That(test6.EndAngle, Is.EqualTo(180.0000000000000000));

            Assert.That(testList[6] is Line);
            Line test7 = (Line)testList[6];

            Assert.That(test7.Start.X, Is.EqualTo(1.9424753611322347));
            Assert.That(test7.Start.Y, Is.EqualTo(0.9771346127707479));
            Assert.That(test7.End.X, Is.EqualTo(1.9424753611322347));
            Assert.That(test7.End.Y, Is.EqualTo(0.4371346127707478));

            Assert.That(testList[7] is Arc);
            Arc test8 = (Arc)testList[7];

            Assert.That(test8.Center.X, Is.EqualTo(1.9724753611322341));
            Assert.That(test8.Center.Y, Is.EqualTo(0.4371346127707478));
            Assert.That(test8.Radius, Is.EqualTo(0.0300000000000000));
            Assert.That(test8.StartAngle, Is.EqualTo(180.0000000000000000));
            Assert.That(test8.EndAngle, Is.EqualTo(270.0000000000000000));

            Assert.That(testList[8] is Circle);
            Circle test9 = (Circle)testList[8];

            Assert.That(test9.Center.X, Is.EqualTo(0.7124999999999999));
            Assert.That(test9.Center.Y, Is.EqualTo(0.7124999999999999));
            Assert.That(test9.Radius, Is.EqualTo(0.2577228596164672));

            Assert.That(testList[9] is Line);
            Line test10 = (Line)testList[9];

            Assert.IsTrue(test10.HasPoint(new Point(1.5688878087292752, 0.6554637349171067)));
            Assert.IsTrue(test10.HasPoint(new Point(1.5688878087292759, 0.7854637349171074)));


            Assert.That(testList[10] is Line);
            Line test11 = (Line)testList[10];

            Assert.That(test11.Start.X, Is.EqualTo(1.3488878087292762));
            Assert.That(test11.Start.Y, Is.EqualTo(0.7854637349171074));
            Assert.That(test11.End.X, Is.EqualTo(1.3488878087292764));
            Assert.That(test11.End.Y, Is.EqualTo(0.6554637349171064));

            Assert.That(testList[11] is Circle);
            Circle test12 = (Circle)testList[11];

            Assert.That(test12.Center.X, Is.EqualTo(4.4637612222222218));
            Assert.That(test12.Center.Y, Is.EqualTo(1.0653138888888889));
            Assert.That(test12.Radius, Is.EqualTo(0.0787500000000000));

            Assert.That(testList[12] is Circle);
            Circle test13 = (Circle)testList[12];

            Assert.That(test13.Center.X, Is.EqualTo(3.1978177228351270));
            Assert.That(test13.Center.Y, Is.EqualTo(0.8976976764264863));
            Assert.That(test13.Radius, Is.EqualTo(0.0787500000000000));

            Assert.That(testList[13] is Circle);
            Circle test14 = (Circle)testList[13];

            Assert.That(test14.Center.X, Is.EqualTo(3.5815706303148396));
            Assert.That(test14.Center.Y, Is.EqualTo(0.7212595580450096));
            Assert.That(test14.Radius, Is.EqualTo(0.1562012667054717));

        }
        [Test]

        //Feed it a DXF file containing missing data internally
        public void TestDXFCorrupt()
        {

            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\CorruptExamples\\CorruptExample-001.dxf";

            try
            {
                DXFFile test = new DXFFile(path);
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("Error: Issue with DXF File"));
            }

        }


        [Test]
        public void TestDWGCorrupt()
        {
            //string Path = "C:\\Users\\ice-haskinss0550\\Source\\Repos\\CS458_002_MathiasDieCompany\\FeatureRecognitionAPI\\FeatureRecognitionAPI\\ExampleFiles\\CorruptExamples\\CorruptExample-001.dwg";
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\CorruptExamples\\CorruptExample-001.dwg";

            try
            {
                DWGFile _ = new DWGFile(path);
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("Attempted to read past the end of the stream."));
            }
        }
        [Test]
        public void TestDWGUnsupportedVersion()
        {
            //string Path = "C:\\Users\\ice-haskinss0550\\Source\\Repos\\CS458_002_MathiasDieCompany\\FeatureRecognitionAPI\\FeatureRecognitionAPI\\ExampleFiles\\CorruptExamples\\WrongVersion.dwg";

            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\CorruptExamples\\WrongVersion.dwg";

            try
            {
                DWGFile _ = new DWGFile(path);
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("File version not supported: AC1009"));
            }


        }

        #endregion

        [Test]
        //Test to verify that library returns the expected entity count
        public void testingACadSharpLibrary()
        {

            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-001.dwg";

            DwgReader dwgReader = new DwgReader(path);

            CadDocument test = dwgReader.Read();

            CadObjectCollection<ACadSharp.Entities.Entity> testList = test.Entities;

            Assert.That(testList.Count(), Is.EqualTo(42));
        }
    }
}