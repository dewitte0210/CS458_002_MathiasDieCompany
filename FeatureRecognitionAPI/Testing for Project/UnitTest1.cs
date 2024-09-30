using FeatureRecognitionAPI.Models;
using NuGet.Frameworks;
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
            Assert.AreEqual(1.9724753611322341, line1.StartX);
            Assert.AreEqual(0.4071346127707478, line1.StartY);
            Assert.AreEqual(2.6124753611322347, line1.EndX);
            Assert.AreEqual(0.4071346127707472, line1.EndY);
            Assert.AreEqual(Math.Sqrt(Math.Pow(2.6124753611322347 - 1.9724753611322341, 2) + Math.Pow(0.4071346127707472 - 0.4071346127707478, 2)), line1.Length);
        }
        [Test]
        public void TestArcClass()
        {
            Arc arc1 = new Arc(2.6124753611322347, 0.4371346127707472, 0.03, 270, 0.0000000000002120);
            Assert.AreEqual((0.03 * Math.Cos(270 * Math.PI / 180) + 2.6124753611322347), arc1.startX);
            Assert.AreEqual((0.03 * Math.Sin(270 * Math.PI / 180) + 0.4371346127707472), arc1.startY);
            Assert.AreEqual((0.03 * Math.Cos(0.0000000000002120 * Math.PI / 180) + 2.6124753611322347), arc1.endX);
            Assert.AreEqual((0.03 * Math.Sin(0.0000000000002120 * Math.PI / 180) + 0.4371346127707472), arc1.endY);
            Assert.AreEqual(0.03, arc1.radius);
            Assert.AreEqual(2.6124753611322347, arc1.centerX);
            Assert.AreEqual(0.4371346127707472, arc1.centerY);
            Assert.AreEqual(270, arc1.startAngle);
            Assert.AreEqual(0.0000000000002120, arc1.endAngle);
            Assert.AreEqual(0.0000000000002120 - 270 + 360, arc1.centralAngle);
            Assert.AreEqual(2 * Math.PI * 0.03 * ((0.0000000000002120 - 270 + 360) / 360), arc1.length);
        }
        [Test]
        public void TestCircleClass()
        {
            Circle circle1 = new Circle(0.7124999999999999, 0.7124999999999999, 0.2577228596164672);
            Assert.AreEqual(0.7124999999999999, circle1.centerX);
            Assert.AreEqual(0.7124999999999999, circle1.centerY);
            Assert.AreEqual(0.2577228596164672, circle1.radius);
            Assert.AreEqual((2 * Math.PI * 0.2577228596164672), circle1.perimeter);
        }
    }
}