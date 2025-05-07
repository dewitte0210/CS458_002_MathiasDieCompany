using FeatureRecognitionAPI.Models.Entities;
using static FeatureRecognitionAPI.Models.Utility.Angles;
using static FeatureRecognitionAPI.Models.Utility.MdcMath;

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
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(45));
                Assert.That(exterior.Value, Is.EqualTo(315));
            });
        }

        [Test]
        public void TestCounterclockwiseTouchingLarge()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 10, -5);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(225));
                Assert.That(exterior.Value, Is.EqualTo(135));
            });
        }

        [Test]
        public void TestClockwiseTouchingSmall()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 0, 0, 5);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(45));
                Assert.That(exterior.Value, Is.EqualTo(315));
            });
        }

        [Test]
        public void TestClockwiseTouchingLarge()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 0, -10, -5);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(225));
                Assert.That(exterior.Value, Is.EqualTo(135));
            });
        }

        [Test]
        public void TestCounterclockwiseTouchingReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 0, 0);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(0));
                Assert.That(exterior.Value, Is.EqualTo(360));
            });
        }

        [Test]
        public void TestClockwiseTouchingReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 0, 0);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(0));
                Assert.That(exterior.Value, Is.EqualTo(360));
            });
        }

        [Test]
        public void TestCounterclockwiseTouching180()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 10, 0);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(180));
                Assert.That(exterior.Value, Is.EqualTo(180));
            });
        }

        [Test]
        public void TestClockwiseTouching180()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 0, -10, 0);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(180));
                Assert.That(exterior.Value, Is.EqualTo(180));
            });
        }

        //-----------------------------------------

        [Test]
        public void TestCounterclockwiseSeperatedSmall()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 5, 0, 10);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(45));
                Assert.That(exterior.Value, Is.EqualTo(315));
            });
        }

        [Test]
        public void TestCounterclockwiseSeperatedLarge()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 15, -5);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(225));
                Assert.That(exterior.Value, Is.EqualTo(135));
            });
        }

        [Test]
        public void TestClockwiseSeperatedSmall()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 5, 0, 10);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(45));
                Assert.That(exterior.Value, Is.EqualTo(315));
            });
        }

        [Test]
        public void TestClockwiseSeperatedLarge()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 5, -10, 0);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(225));
                Assert.That(exterior.Value, Is.EqualTo(135));
            });
        }

        [Test]
        public void TestCounterclockwiseSeperatedReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 5, 0);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(0));
                Assert.That(exterior.Value, Is.EqualTo(360));
                Assert.That(IsParallel(line1, line2), Is.True);
            });
        }

        [Test]
        public void TestClockwiseSeperatedReversed()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 5, 0);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(0));
                Assert.That(exterior.Value, Is.EqualTo(360));
                Assert.That(IsParallel(line1, line2), Is.True);
            });
        }

        [Test]
        public void TestCounterclockwiseSeperated180()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 5, 10, 5);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Counter).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Counter).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(180));
                Assert.That(exterior.Value, Is.EqualTo(180));
                Assert.That(IsParallel(line1, line2), Is.True);
            });
        }

        [Test]
        public void TestClockwiseSeperated180()
        {
            Line line1 = new(0, 0, -5, 0);
            Line line2 = new(-5, 5, -10, 5);
            Degrees interior = GetAngle(line1, line2, Side.Interior, Orientation.Clockwise).GetDegrees();
            Degrees exterior = GetAngle(line1, line2, Side.Exterior, Orientation.Clockwise).GetDegrees();

            Assert.Multiple(() =>
            {
                Assert.That(interior.Value, Is.EqualTo(180));
                Assert.That(exterior.Value, Is.EqualTo(180));
                Assert.That(IsParallel(line1, line2), Is.True);
            });
        }

        //perpendicular
        [Test]
        public void TestCounterclockwiseTouchingPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 5, 5);

            Assert.That(IsPerpendicular(line1, line2), Is.True);
        }

        [Test]
        public void TestClockwiseTouchingPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(5, 0, 5, -5);

            Assert.That(IsPerpendicular(line1, line2), Is.True);
        }

        [Test]
        public void TestCounterclockwiseSeperatedPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 10, 5);

            Assert.That(IsPerpendicular(line1, line2), Is.True);
        }

        [Test]
        public void TestClockwiseSeperatedPerpendicular()
        {
            Line line1 = new(0, 0, 5, 0);
            Line line2 = new(10, 0, 10, -5);

            Assert.That(IsPerpendicular(line1, line2), Is.True);
        }

        #endregion

        #region MDCMath

        [Test]
        public void DoubleEqualsTest()
        {
            Assert.That(DEQ(0.0, 0.0), Is.True);
        }

        #endregion
    }
}
