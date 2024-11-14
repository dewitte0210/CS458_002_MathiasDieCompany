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
        #region CheckGroup1B
        [Test]
        public void CheckGroup1B_Circle_ReturnsTrue()
        {
            Circle circle = new(0.0, 0.0, 10.0);
            List<Entity> entities = new List<Entity>() { circle };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group1B1));
        }

        [Test]
        public void CheckGroup1B_RoundedRectangle_ReturnsTrue()
        {
            Arc arc1 = new(0.0, 0.0, 5.0, 90.0, 270.0);
            Arc arc2 = new(10.0, 0.0, 5.0, 270.0, 90.0);
            Line line1 = new(0.0, 5.0, 10.0, 5.0);
            Line line2 = new(0.0, -5.0, 10.0, -5.0);
            List<Entity> entities = new List<Entity>() { arc1, arc2, line1, line2 };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group1B2));
        }

        [Test]
        public void CheckGroup1B_BadFeature_ReturnsFalse()
        {
            Arc arc = new(0.0, 0.0, 5.0, 90.0, 270.0);
            Circle circle = new(0.0, 0.0, 5.0);
            List<Entity> entities = new List<Entity> { arc, circle };
            Feature testFeature = new(entities) { baseEntityList = entities };
            testFeature.DetectFeatures();
            Assert.That(testFeature.featureType, Is.EqualTo(PossibleFeatureTypes.SideTubePunch));
        }
        #endregion
       
        #region CheckGroup5
        [Test]
        public void CheckGroup5_3LineCompartment_ReturnsTrue()
        {
            Line line1 = new(0.0, 0.0, 0.0, 5.0);
            Line line2 = new(0.0, 5.0, 5.0, 5.0);
            Line line3 = new(5.0, 5.0, 5.0, 0.0);
            List<Entity> entities = new List<Entity>() { line1, line2, line3 };
            Feature testFeature = new(entities) { PerimeterEntityList = new List<List<Entity>>() { entities } };
            testFeature.DetectFeatures();
            Assert.That(testFeature.perimeterFeatures[0], Is.EqualTo(PerimeterFeatureTypes.Group5));
        }

        [Test]
        public void CheckGroup5_2Line1Arc_ReturnsTrue()
        {
            Line line1 = new(-1.0, 0.0, -1.0, 5.0);
            Arc arc1 = new(0.0, 0.0, 1.0, 180, 360);
            Line line2 = new(1.0, 0.0, 1.0, 5.0);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2 };
            Feature testFeature = new(entities) { PerimeterEntityList = new List<List<Entity>> { entities } };
            testFeature.DetectFeatures();
            Assert.That(testFeature.perimeterFeatures[0], Is.EqualTo(PerimeterFeatureTypes.Group5));
        }

        [Test]
        public void CheckGroup5_3Line2Arc_ReturnsTrue()
        {
            Line line1 = new(0.0, 1.0, 0.0, 5.0);
            Arc arc1 = new(1.0, 1.0, 1.0, 180, 270);
            Line line2 = new(1.0, 0.0, 4.0, 0.0);
            Arc arc2 = new(4.0, 1.0, 1.0, 270, 360);
            Line line3 = new(5.0, 1.0, 5.0, 5.0);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2, line3 };
            Feature testFeature = new(entities) { PerimeterEntityList = new List<List<Entity>>() { entities } };
            testFeature.DetectFeatures();
            Assert.That(testFeature.perimeterFeatures[0], Is.EqualTo(PerimeterFeatureTypes.Group5));
        }
        #endregion
        
        #region CheckGroup4
        [Test]
        public void CheckGroup4_2LineAngled_ReturnsTrue()
        {
            Line line1 = new(0.0, 1.0, 1.0, 0.0);
            Line line2 = new(1.0, 0.0, 2.0, 1.0);
            List<Entity> entities = new List<Entity>() { line1, line2 };
            Feature testFeature = new(entities) { PerimeterEntityList = new List<List<Entity>>() { entities } };
            testFeature.DetectFeatures();
            Assert.That(testFeature.perimeterFeatures[0], Is.EqualTo(PerimeterFeatureTypes.Group4));
        }

        [Test]
        public void CheckGroup4_2Arc2Line_ReturnsTrue()
        {

            Arc arc1 = new(0.0, 3.0, 2, 0, 90);
            Line line1 = new(1.0, 2.0, 1.0, 1.0);
            Line line2 = new(1.0, 1.0, 2.0, 1.0);
            Arc arc2 = new(3.0, 0.0, 2, 0, 90);
            List<Entity> entities = new List<Entity> { arc1, line1, line2, arc2 };
            Feature testFeature = new(entities) { PerimeterEntityList = new List<List<Entity>>() { entities } };
            testFeature.DetectFeatures();
            Assert.That(testFeature.perimeterFeatures[0], Is.EqualTo(PerimeterFeatureTypes.Group4));
        }

        [Test]
        public void CheckGroup4_2LineNotAngled_ReturnsTrue()
        {
            Line line1 = new(0.0, 1.0, 0.0, 0.0);
            Line line2 = new(0.0, 0.0, 1.0, 0.0);
            List<Entity> entities = new List<Entity> { line1, line2 };
            Feature testFeature = new(entities) { PerimeterEntityList = new List<List<Entity>> { entities } };
            testFeature.DetectFeatures();
            Assert.That(testFeature.perimeterFeatures[0], Is.EqualTo(PerimeterFeatureTypes.Group4));
        }
        #endregion 
    }
}
