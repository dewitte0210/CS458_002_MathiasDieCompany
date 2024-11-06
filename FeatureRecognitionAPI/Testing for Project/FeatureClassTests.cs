using NUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Enums;

namespace Testing_for_Project
{
    internal class FeatureClassTests
    {
        [Test] 
        public void CheckGroup1B_Circle_ReturnsTrue()
        {
            Circle circle = new(0.0, 0.0, 10.0);
            List<Entity> entities = new List<Entity>() { circle };
            Feature testFeature = new(entities) { baseEntityList = entities};
            testFeature.DetectFeatures();
            Assert.That(testFeature.featureType, Is.EqualTo(PossibleFeatureTypes.Group1B1));
        }

        [Test]
        public void CheckGroup1B_RoundedRectangle_ReturnsTrue()
        {
            Arc arc1 = new(0.0, 0.0, 5.0, 90.0, 270.0);
            Arc arc2 = new(10.0, 0.0, 5.0, 270.0, 90.0);
            Line line1 = new(0.0, 5.0, 10.0, 5.0);
            Line line2 = new(0.0, -5.0, 10.0, -5.0);
            List<Entity> entities = new List<Entity>() { arc1, arc2, line1, line2 };
            Feature testFeature = new(entities) { baseEntityList = entities};
            testFeature.DetectFeatures();
            Assert.That(testFeature.featureType, Is.EqualTo(PossibleFeatureTypes.Group1B2));
        }

        [Test]
        public void CheckGroup1B_BadFeature_ReturnsFalse()
        {
            Arc arc = new(0.0, 0.0, 5.0, 90.0, 270.0);
            Circle circle = new(0.0, 0.0, 5.0);
            List<Entity> entities = new List<Entity> { arc, circle };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.featureType, Is.EqualTo(PossibleFeatureTypes.Punch));
        }

        [Test]

        public void VerifyMaxPoint()
        {
            Line line1 = new(10.6, 12.0, 10.0, 5.0);
            Line line2 = new(0.0, -5.0, 13.8, -5.0);
            Line line3 = new(0.0, -5.0, 10.0, -6.0);
            Line line4 = new(-10.0, -5.0, 4, -5.0);
            List<Entity> entities = new List<Entity> { line1, line2, line3, line4 };
            Feature testFeature = new Feature(entities);

            Point maxPoint = testFeature.FindMaxPoint();
            Point minPoint = testFeature.FindMinPoint();

            Assert.That(maxPoint.X, Is.EqualTo(13.8));
            Assert.That(maxPoint.Y, Is.EqualTo(12));

            Assert.That(minPoint.X, Is.EqualTo(-10));
            Assert.That(minPoint.Y, Is.EqualTo(-6));

        }
    }
}
