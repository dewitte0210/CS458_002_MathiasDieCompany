using FeatureRecognitionAPI.Models;
using NuGet.Frameworks;
using DecimalMath;
using Microsoft.AspNetCore.Rewrite;

namespace Testing_for_Project
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestLineClass()
        {
            Line line1 = new Line(1.9724753611322341, 0.4071346127707478, 2.6124753611322347, 0.4071346127707472);
            Assert.That(line1.StartX, Is.EqualTo(1.9724753611322341));
            Assert.That(line1.StartY, Is.EqualTo(0.4071346127707478));
            Assert.That(line1.EndX, Is.EqualTo(2.6124753611322347));
            Assert.That(line1.EndY, Is.EqualTo(0.4071346127707472));
            Assert.That(line1.Length, Is.EqualTo(Math.Sqrt(Math.Pow(2.6124753611322347 - 1.9724753611322341, 2) + Math.Pow(0.4071346127707472 - 0.4071346127707478, 2))));
        }
        [Test]
        public void TestArcClass()
        {
            Arc arc1 = new Arc(2.6124753611322347, 0.4371346127707472, 0.03, 270, 0.0000000000002120);
            Assert.That(arc1.startX, Is.EqualTo((0.03 * Math.Cos(270 * Math.PI / 180) + 2.6124753611322347)));
            Assert.That(arc1.startY, Is.EqualTo((0.03 * Math.Sin(270 * Math.PI / 180) + 0.4371346127707472)));
            Assert.That(arc1.endX, Is.EqualTo((0.03 * Math.Cos(0.0000000000002120 * Math.PI / 180) + 2.6124753611322347)));
            Assert.That(arc1.endY, Is.EqualTo((0.03 * Math.Sin(0.0000000000002120 * Math.PI / 180) + 0.4371346127707472)));
            Assert.That(arc1.radius, Is.EqualTo(0.03));
            Assert.That(arc1.centerX, Is.EqualTo(2.6124753611322347));
            Assert.That(arc1.centerY, Is.EqualTo(0.4371346127707472));
            Assert.That(arc1.startAngle, Is.EqualTo(270));
            Assert.That(arc1.endAngle, Is.EqualTo(0.0000000000002120));
            Assert.That(arc1.centralAngle, Is.EqualTo(0.0000000000002120 - 270 + 360));
            Assert.That(arc1.length, Is.EqualTo(2 * Math.PI * 0.03 * ((0.0000000000002120 - 270 + 360) / 360)));
        }
        [Test]
        public void TestCircleClass()
        {
            Circle circle1 = new Circle(0.7124999999999999, 0.7124999999999999, 0.2577228596164672);
            Assert.That(circle1.centerX, Is.EqualTo(0.7124999999999999));
            Assert.That(circle1.centerY, Is.EqualTo(0.7124999999999999));
            Assert.That(circle1.radius, Is.EqualTo(0.2577228596164672));
            Assert.That(circle1.perimeter, Is.EqualTo((2 * Math.PI * 0.2577228596164672)));
        }

        [Test]
        public void TestDXFFileClass()
        {
            //Set path to any filepath containing the 3rd example dxf file
            string path = @"C:\Users\Ice-HaskinsStephen\Downloads\Example-003.dxf";
            bool pathTest = File.Exists(path);

            //Make sure the file exists so that DXF can be created
            Assert.That(pathTest, Is.True);
            DXFFile testFile = new DXFFile(path);

            //Thirteen known entities in Example-003.dxf that must be verified to have been accurately ingested
            List<Entity> testList = testFile.GetEntities();
            Assert.That(testList[0].GetEntityType(), Is.EqualTo("arc"));
            Arc test1 = (Arc)testList[0];

            Assert.That(test1.centerX, Is.EqualTo(11.2650987394999991));
            Assert.That(test1.centerY, Is.EqualTo(0.7549998982999999));
            Assert.That(test1.radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test1.startAngle, Is.EqualTo(270.0000000000000000));
            Assert.That(test1.endAngle, Is.EqualTo(30.0000000000000036));

            Assert.That(testList[1].GetEntityType(), Is.EqualTo("line"));
            Line test2 = (Line)testList[1];

            Assert.That(test2.StartX, Is.EqualTo(11.2650987394999991));
            Assert.That(test2.StartY, Is.EqualTo(0.0049998983000000));
            Assert.That(test2.EndX, Is.EqualTo(6.0146749507999999));
            Assert.That(test2.EndY, Is.EqualTo(0.0049998983000000));

            Assert.That(testList[2].GetEntityType(), Is.EqualTo("arc"));
            Arc test3 = (Arc)testList[2];

            Assert.That(test3.centerX, Is.EqualTo(6.0146749507999999));
            Assert.That(test3.centerY, Is.EqualTo(0.7549998982999998));
            Assert.That(test3.radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test3.startAngle, Is.EqualTo(149.9999999999999716));
            Assert.That(test3.endAngle, Is.EqualTo(270.0000000000000000));

            Assert.That(testList[3].GetEntityType(), Is.EqualTo("circle"));
            Circle test4 = (Circle)testList[3];

            Assert.That(test4.centerX, Is.EqualTo(3.3752118942999996));
            Assert.That(test4.centerY, Is.EqualTo(2.6843885206000002));
            Assert.That(test4.radius, Is.EqualTo(1.0000000000000000));

            Assert.That(testList[4].GetEntityType(), Is.EqualTo("arc"));
            Arc test5 = (Arc)testList[4];

            Assert.That(test5.centerX, Is.EqualTo(6.0004237886999992));
            Assert.That(test5.centerY, Is.EqualTo(4.2000000000000011));
            Assert.That(test5.radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test5.startAngle, Is.EqualTo(329.9990346727777819));
            Assert.That(test5.endAngle, Is.EqualTo(90.0000000000000000));

            Assert.That(testList[5].GetEntityType(), Is.EqualTo("line"));
            Line test6 = (Line)testList[5];

            Assert.That(test6.StartX, Is.EqualTo(6.0004237886999992));
            Assert.That(test6.StartY, Is.EqualTo(4.9500000000000011));
            Assert.That(test6.EndX, Is.EqualTo(0.7500000000000000));
            Assert.That(test6.EndY, Is.EqualTo(4.9500000000000011));

            Assert.That(testList[6].GetEntityType(), Is.EqualTo("arc"));
            Arc test7 = (Arc)testList[6];

            Assert.That(test7.centerX, Is.EqualTo(0.7499999999999996));
            Assert.That(test7.centerY, Is.EqualTo(4.2000000000000011));
            Assert.That(test7.radius, Is.EqualTo(0.7499999999999999));
            Assert.That(test7.startAngle, Is.EqualTo(89.9999999999999716));
            Assert.That(test7.endAngle, Is.EqualTo(210.0009653272222181));

            Assert.That(testList[7].GetEntityType(), Is.EqualTo("circle"));
            Circle test8 = (Circle)testList[7];

            Assert.That(test8.centerX, Is.EqualTo(8.6398868451999995));
            Assert.That(test8.centerY, Is.EqualTo(2.2706113776999994));
            Assert.That(test8.radius, Is.EqualTo(1.0000000000000000));

            Assert.That(testList[8].GetEntityType(), Is.EqualTo("line"));
            Line test9 = (Line)testList[8];

            Assert.That(test9.StartX, Is.EqualTo(5.3651558979999994));
            Assert.That(test9.StartY, Is.EqualTo(1.1299998982999999));
            Assert.That(test9.EndX, Is.EqualTo(7.0403281640286135));
            Assert.That(test9.EndY, Is.EqualTo(4.0313704835897957));

            Assert.That(testList[9].GetEntityType(), Is.EqualTo("arc"));
            Arc test10 = (Arc)testList[9];

            Assert.That(test10.centerX, Is.EqualTo(8.6398868451999995));
            Assert.That(test10.centerY, Is.EqualTo(3.1078289152999994));
            Assert.That(test10.radius, Is.EqualTo(1.8470292371454000));
            Assert.That(test10.startAngle, Is.EqualTo(30.0009653272222003));
            Assert.That(test10.endAngle, Is.EqualTo(149.9990346727777535));

            Assert.That(testList[10].GetEntityType(), Is.EqualTo("line"));
            Line test11 = (Line)testList[10];

            Assert.That(test11.StartX, Is.EqualTo(10.2394455263713855));
            Assert.That(test11.StartY, Is.EqualTo(4.0313704835897939));
            Assert.That(test11.EndX, Is.EqualTo(11.9146177922999996));
            Assert.That(test11.EndY, Is.EqualTo(1.1299998982999999));

            Assert.That(testList[11].GetEntityType, Is.EqualTo("line"));
            Line test12 = (Line)testList[11];

            Assert.That(test12.StartX, Is.EqualTo(6.6499365233942473));
            Assert.That(test12.StartY, Is.EqualTo(3.8249890568663383));
            Assert.That(test12.EndX, Is.EqualTo(4.9747705754328919));
            Assert.That(test12.EndY, Is.EqualTo(0.9236294146435339));

            Assert.That(testList[12].GetEntityType(), Is.EqualTo("arc"));
            Arc test13 = (Arc)testList[12];

            Assert.That(test13.centerX, Is.EqualTo(3.3752118942999996));
            Assert.That(test13.centerY, Is.EqualTo(1.8471709829999998));
            Assert.That(test13.radius, Is.EqualTo(1.8470292371454000));
            Assert.That(test13.startAngle, Is.EqualTo(210.0009653296103238));
            Assert.That(test13.endAngle, Is.EqualTo(329.9990346703896762));

            Assert.That(testList[13].GetEntityType(), Is.EqualTo("line"));
            Line test14 = (Line)testList[13];

            Assert.That(test14.StartX, Is.EqualTo(1.7756532131671074));
            Assert.That(test14.StartY, Is.EqualTo(0.9236294146435343));
            Assert.That(test14.EndX, Is.EqualTo(0.1004872653057516));
            Assert.That(test14.EndY, Is.EqualTo(3.8249890568663383));

        }
    }
}