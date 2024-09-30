using FeatureRecognitionAPI.Models;
using NuGet.Frameworks;
using DecimalMath;

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
            Assert.AreEqual(1.9724753611322341m, line1.StartX);
            Assert.AreEqual(0.4071346127707478m, line1.StartY);
            Assert.AreEqual(2.6124753611322347m, line1.EndX);
            Assert.AreEqual(0.4071346127707472m, line1.EndY);
            Assert.AreEqual(DecimalEx.Sqrt(DecimalEx.Pow(2.6124753611322347m - 1.9724753611322341m, 2) + DecimalEx.Pow(0.4071346127707472m - 0.4071346127707478m, 2)), line1.Length);
        }
        [Test]
        public void TestArcClass()
        {
            Arc arc1 = new Arc(2.6124753611322347m, 0.4371346127707472m, 0.03m, 270m, 0.0000000000002120m);
            Assert.AreEqual((0.03m * DecimalEx.Cos(270m * DecimalEx.Pi / 180m) + 2.6124753611322347m), arc1.startX);
            Assert.AreEqual((0.03m * DecimalEx.Sin(270m * DecimalEx.Pi / 180m) + 0.4371346127707472m), arc1.startY);
            Assert.AreEqual((0.03m * DecimalEx.Cos(0.0000000000002120m * DecimalEx.Pi / 180m) + 2.6124753611322347m), arc1.endX);
            Assert.AreEqual((0.03m * DecimalEx.Sin(0.0000000000002120m * DecimalEx.Pi / 180m) + 0.4371346127707472m), arc1.endY);
            Assert.AreEqual(0.03m, arc1.radius);
            Assert.AreEqual(2.6124753611322347m, arc1.centerX);
            Assert.AreEqual(0.4371346127707472m, arc1.centerY);
            Assert.AreEqual(270m, arc1.startAngle);
            Assert.AreEqual(0.0000000000002120m, arc1.endAngle);
            Assert.AreEqual(0.0000000000002120m - 270m + 360, arc1.centralAngle);
            Assert.AreEqual(2 * DecimalEx.Pi * 0.03m * ((0.0000000000002120m - 270m + 360m) / 360), arc1.length);
        }
        [Test]
        public void TestCircleClass()
        {
            Circle circle1 = new Circle(0.7124999999999999m, 0.7124999999999999m, 0.2577228596164672m);
            Assert.AreEqual(0.7124999999999999m, circle1.centerX);
            Assert.AreEqual(0.7124999999999999m, circle1.centerY);
            Assert.AreEqual(0.2577228596164672m, circle1.radius);
            Assert.AreEqual((2 * DecimalEx.Pi * 0.2577228596164672m), circle1.perimeter);
        }
    }
}