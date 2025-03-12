using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Utility;

namespace Testing_for_Project
{
    class UtilityTests
    {

        #region Angles

        [Test]
        public void TestCounterclockwiseTouchingSmall()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 0, 5);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(45));
            Assert.That(exterior.angle, Is.EqualTo(315));
        }

        [Test]
        public void TestCounterclockwiseTouchingLarge()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 10, -5);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(225));
            Assert.That(exterior.angle, Is.EqualTo(135));
        }

        [Test]
        public void TestClockwiseTouchingSmall()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 0, 0, 5);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(45));
            Assert.That(exterior.angle, Is.EqualTo(315));
        }

        [Test]
        public void TestClockwiseTouchingLarge()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 0, -10, -5);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(225));
            Assert.That(exterior.angle, Is.EqualTo(135));
        }

        [Test]
        public void TestCounterclockwiseTouchingReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 0, 0);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(0));
            Assert.That(exterior.angle, Is.EqualTo(360));
        }

        [Test]
        public void TestClockwiseTouchingReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 0, 0);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(0));
            Assert.That(exterior.angle, Is.EqualTo(360));
        }

        [Test]
        public void TestCounterclockwiseTouching180()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 10, 0);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(180));
            Assert.That(exterior.angle, Is.EqualTo(180));
        }

        [Test]
        public void TestClockwiseTouching180()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 0, -10, 0);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(180));
            Assert.That(exterior.angle, Is.EqualTo(180));
        }

        //-----------------------------------------

        [Test]
        public void TestCounterclockwiseSeperatedSmall()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 5, 0, 10);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(45));
            Assert.That(exterior.angle, Is.EqualTo(315));
        }

        [Test]
        public void TestCounterclockwiseSeperatedLarge()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 15, -5);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(225));
            Assert.That(exterior.angle, Is.EqualTo(135));
        }

        [Test]
        public void TestClockwiseSeperatedSmall()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 5, 0, 10);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(45));
            Assert.That(exterior.angle, Is.EqualTo(315));
        }

        [Test]
        public void TestClockwiseSeperatedLarge()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 5, -10, 0);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(225));
            Assert.That(exterior.angle, Is.EqualTo(135));
        }

        [Test]
        public void TestCounterclockwiseSeperatedReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 5, 0);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(0));
            Assert.That(exterior.angle, Is.EqualTo(360));
            Assert.IsTrue(Angles.IsParallel(line1, line2));
        }

        [Test]
        public void TestClockwiseSeperatedReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 5, 0);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(0));
            Assert.That(exterior.angle, Is.EqualTo(360));
            Assert.IsTrue(Angles.IsParallel(line1, line2));
        }

        [Test]
        public void TestCounterclockwiseSeperated180()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 5, 10, 5);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.COUNTERCLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(180));
            Assert.That(exterior.angle, Is.EqualTo(180));
            Assert.IsTrue(Angles.IsParallel(line1, line2));
        }

        [Test]
        public void TestClockwiseSeperated180()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 5, -10, 5);
            Angles.Degrees interior = Angles.GetAngle(line1, line2, Angles.Side.INTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();
            Angles.Degrees exterior = Angles.GetAngle(line1, line2, Angles.Side.EXTERIOR, Angles.Orientation.CLOCKWISE).GetDegrees();

            Assert.That(interior.angle, Is.EqualTo(180));
            Assert.That(exterior.angle, Is.EqualTo(180));
            Assert.IsTrue(Angles.IsParallel(line1, line2));
        }

        //perpendicular
        [Test]
        public void TestCounterclockwiseTouchingPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 5, 5);

            Assert.IsTrue(Angles.IsPerpendicular(line1, line2));
        }

        [Test]
        public void TestClockwiseTouchingPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 5, -5);

            Assert.IsTrue(Angles.IsPerpendicular(line1, line2));
        }

        [Test]
        public void TestCounterclockwiseSeperatedPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 10, 5);

            Assert.IsTrue(Angles.IsPerpendicular(line1, line2));
        }

        [Test]
        public void TestClockwiseSeperatedPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 10, -5);

            Assert.IsTrue(Angles.IsPerpendicular(line1, line2));
        }

        #endregion
    }
}
