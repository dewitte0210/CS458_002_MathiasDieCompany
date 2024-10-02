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
            Line line1 = new Line(1.9724753611322341m, 0.4071346127707478m, 2.6124753611322347m, 0.4071346127707472m);
            Assert.That(line1.StartX, Is.EqualTo(1.9724753611322341m));
            Assert.That(line1.StartY, Is.EqualTo(0.4071346127707478m));
            Assert.That(line1.EndX, Is.EqualTo(2.6124753611322347m));
            Assert.That(line1.EndY, Is.EqualTo(0.4071346127707472m));
            Assert.That(line1.Length, Is.EqualTo(DecimalEx.Sqrt(DecimalEx.Pow(2.6124753611322347m - 1.9724753611322341m, 2) + DecimalEx.Pow(0.4071346127707472m - 0.4071346127707478m, 2))));
        }
        [Test]
        public void TestArcClass()
        {
            Arc arc1 = new Arc(2.6124753611322347m, 0.4371346127707472m, 0.03m, 270m, 0.0000000000002120m);
            Assert.That(arc1.startX, Is.EqualTo((0.03m * DecimalEx.Cos(270m * DecimalEx.Pi / 180m) + 2.6124753611322347m)));
            Assert.That(arc1.startY, Is.EqualTo((0.03m * DecimalEx.Sin(270m * DecimalEx.Pi / 180m) + 0.4371346127707472m)));
            Assert.That(arc1.endX, Is.EqualTo((0.03m * DecimalEx.Cos(0.0000000000002120m * DecimalEx.Pi / 180m) + 2.6124753611322347m)));
            Assert.That(arc1.endY, Is.EqualTo((0.03m * DecimalEx.Sin(0.0000000000002120m * DecimalEx.Pi / 180m) + 0.4371346127707472m)));
            Assert.That(arc1.radius, Is.EqualTo(0.03m));
            Assert.That(arc1.centerX, Is.EqualTo(2.6124753611322347m));
            Assert.That(arc1.centerY, Is.EqualTo(0.4371346127707472m));
            Assert.That(arc1.startAngle, Is.EqualTo(270m));
            Assert.That(arc1.endAngle, Is.EqualTo(0.0000000000002120m));
            Assert.That(arc1.centralAngle, Is.EqualTo(0.0000000000002120m - 270m + 360));
            Assert.That(arc1.length, Is.EqualTo(2 * DecimalEx.Pi * 0.03m * ((0.0000000000002120m - 270m + 360m) / 360)));
        }
        [Test]
        public void TestCircleClass()
        {
            Circle circle1 = new Circle(0.7124999999999999m, 0.7124999999999999m, 0.2577228596164672m);
            Assert.That(circle1.centerX, Is.EqualTo(0.7124999999999999m));
            Assert.That(circle1.centerY, Is.EqualTo(0.7124999999999999m));
            Assert.That(circle1.radius, Is.EqualTo(0.2577228596164672m));
            Assert.That(circle1.perimeter, Is.EqualTo((2 * DecimalEx.Pi * 0.2577228596164672m)));
        }

        [Test]
        public void TestDXFFileClass()
        {
            string path = @"C:\Users\Ice-HaskinsStephen\Downloads\Example-003.dxf";
            bool pathTest = File.Exists(path);

            //Make sure the file exists so that DXF can be created
            Assert.That(pathTest, Is.True);
            DXFFile testFile = new DXFFile(path);

            List<Entity> testList = testFile.GetEntities();            
            Assert.That(testList[0].GetEntityType(), Is.EqualTo("arc"));
            Arc test1 = (Arc)testList[0];

            Assert.That(test1.centerX, Is.EqualTo(11.2650987394999991m));
            Assert.That(test1.centerY, Is.EqualTo(0.7549998982999999m));
            Assert.That(test1.radius, Is.EqualTo(0.7499999999999999m));
            Assert.That(test1.startAngle, Is.EqualTo(270.0000000000000000m));
            Assert.That(test1.endAngle, Is.EqualTo(30.0000000000000036m));

            Assert.That(testList[1].GetEntityType(), Is.EqualTo("line"));
            Line test2 = (Line)testList[1];

            Assert.That(test2.StartX, Is.EqualTo(11.2650987394999991m));
            Assert.That(test2.StartY, Is.EqualTo(0.0049998983000000m));
            Assert.That(test2.EndX, Is.EqualTo(6.0146749507999999m));
            Assert.That(test2.EndY, Is.EqualTo(0.0049998983000000m));

            Assert.That(testList[2].GetEntityType(), Is.EqualTo("arc"));
            Arc test3 = (Arc)testList[2];

            Assert.That(test3.centerX, Is.EqualTo(6.0146749507999999m));
            Assert.That(test3.centerY, Is.EqualTo(0.7549998982999998m));
            Assert.That(test3.radius, Is.EqualTo(0.7499999999999999m));
            Assert.That(test3.startAngle, Is.EqualTo(149.9999999999999716m));
            Assert.That(test3.endAngle, Is.EqualTo(270.0000000000000000m));


            




        }
    }
}