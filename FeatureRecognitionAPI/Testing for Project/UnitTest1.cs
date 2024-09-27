using FeatureRecognitionAPI.Models;
namespace Testing_for_Project
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestArcClass()
        {
            Arc arc1 = new Arc(2.6124753611322347, 0.4371346127707472, 0.03, 270, 0.0000000000002120);
            Assert.AreEqual(arc1.startX, (0.03 * Math.Cos(270 * Math.PI / 180) + 2.6124753611322347));
            Assert.AreEqual(arc1.startY, (0.03 * Math.Sin(270 * Math.PI / 180) + 0.4371346127707472));
            Assert.AreEqual(arc1.endX, (0.03 * Math.Cos(0.0000000000002120 * Math.PI / 180) + 2.6124753611322347));
            Assert.AreEqual(arc1.endY, (0.03 * Math.Sin(0.0000000000002120 * Math.PI / 180) + 0.4371346127707472));
            Assert.AreEqual(arc1.radius, 0.03);
            Assert.AreEqual(arc1.centerX, 2.6124753611322347);
            Assert.AreEqual(arc1.centerY, 0.4371346127707472);
            Assert.AreEqual(arc1.startAngle, 270);
            Assert.AreEqual(arc1.endAngle, 0.0000000000002120);
        }
    }
}