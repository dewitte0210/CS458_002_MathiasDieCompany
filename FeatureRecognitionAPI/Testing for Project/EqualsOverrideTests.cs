
using FeatureRecognitionAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing_for_Project
{
    internal class EqualsOverrideTests
    {
        [Test]
        public void EqualsOverrideLine()
        {
            Line line1 = new(1, 1, 2, 2);
            Line line2 = new(2, 2, 3, 3);
            Line line3 = new(1, 2, 2, 2);

            bool equals1 = line1.Equals(line2);

            bool equals2 = line1.Equals(line3);

            Assert.That(equals1, Is.EqualTo(true));

            Assert.That(equals2, Is.EqualTo(false));
        }

        [Test]
        public void EqualsOverrideArc()
        {
            Arc arc1 = new(4, 2, 4.5, 30, 50);
            Arc arc2 = new(3, 9, 4.5, 30, 50);
            Arc arc3 = new(4, 2, 5, 30, 50);

            bool equals1 = arc1.Equals(arc2);
            bool equals2 = arc1.Equals(arc3);

            Assert.That(equals1, Is.EqualTo(true));
            Assert.That(equals2, Is.EqualTo(false));
        }

        [Test]
        public void EqualsOverrideCircle()
        {
            Circle circle1 = new(5, 5, 5);
            Circle circle2 = new(4, 4, 5);
            Circle circle3 = new(5, 4, 4);

            bool equals1 = circle1.Equals(circle2);
            bool equals2 = circle1.Equals(circle3);

            Assert.That(equals1, Is.EqualTo(true));
            Assert.That(equals2, Is.EqualTo(false));
        }

        [Test]
        public void EqualsOverrideFeature()
        {
            //Equal versions
            Arc arc1 = new(1, 1, 3, 90, 180);
            Arc arc2 = new(5, 5, 3, 180, 90);

            Line line1 = new(arc1.Start.X, arc1.Start.Y, arc2.Start.X, arc2.Start.Y);
            Line line2 = new(arc1.End.X, arc1.End.Y, arc2.End.X, arc2.End.Y);

            Arc arc5 = new(1, 1, 3, 90, 180);
            Arc arc6 = new(5, 5, 3, 180, 90);

            Line line5 = new(arc5.Start.X, arc5.Start.Y, arc6.Start.X, arc6.Start.Y);
            Line line6 = new(arc5.End.X, arc5.End.Y, arc6.End.X, arc6.End.Y);
            
            List<Entity> entities = new List<Entity>() { arc1, arc2, line1, line2 };
            List<Entity> entities3 = new List<Entity>() { arc5, arc6, line5, line6 };

            //Unequal version
            Arc arc3 = new(10, 10, 4, 90, 180);
            Arc arc4 = new(15, 15, 4, 180, 90);

            Line line3 = new(arc3.Start.X, arc3.Start.Y, arc4.Start.X, arc4.Start.Y);
            Line line4 = new(arc3.End.X, arc3.End.Y, arc4.End.X, arc4.End.Y);

            List<Entity> entities2 = new List<Entity>() { arc3, arc4, line3, line4 };

            //These two should equal
            Feature f1 = new(entities);
            Feature f2 = new(entities3);

            //This one doesn't equal
            Feature f3 = new(entities2);

            bool equals1 = f1.Equals(f2);
            bool equals2 = f1.Equals(f3);

            Assert.That(equals1, Is.EqualTo(true));
            Assert.That(equals2 , Is.EqualTo(false)); 
        }
    }
}
