
using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Features;

namespace Testing_for_Project
{
    internal class CompareAndEqualMethodTests
    {
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

            //This one shouldn't equal the others
            Feature f3 = new(entities2);

            bool equals1 = f1.Compare(f2);
            bool equals2 = f1.Compare(f3);

            Assert.That(equals1, Is.EqualTo(true));
            Assert.That(equals2, Is.EqualTo(false));
        }

        [Test]
        public void FeatureGroupEquals()
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

            //These two should equal each other
            Feature f1 = new(entities);
            Feature f2 = new(entities3);

            //This one doesn't equal
            Feature f3 = new(entities2);

            //Two lists with the same feature
            List<Feature> features1 = new List<Feature>() { f1, f2, f3 };
            FeatureGroup fg1 = new(features1);
            List<Feature> features2 = new List<Feature> { f1, f2, f3 };
            FeatureGroup fg2 = new(features2);

            //Another list without equal entities
            List<Feature> features3 = new List<Feature> { f1, f3, f3 };
            FeatureGroup fg3 = new(features3);


            bool test1 = fg1.Equals(fg2);
            bool test2 = fg1.Equals(fg3);
            Assert.That(test1, Is.EqualTo(true));
            Assert.That(test2, Is.EqualTo(false));
        }
    }
}
