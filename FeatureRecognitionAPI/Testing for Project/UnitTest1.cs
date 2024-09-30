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
    }
}