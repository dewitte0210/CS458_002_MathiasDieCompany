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
            Assert.That(testFeature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Punch));
        }
        #endregion

        #region CheckGroup2A
        public void CheckGroup2A_EllipseMadeOfArcs_ReturnsTrue()
        {
            Arc arc1 = new(1.1399417701703984, 1.8531050558148150, 0.1680136484100009, 0.9555661934363535, 36.8885215710321290);
            Arc arc2 = new(1.0323926156114558, 1.7720412332222049, 0.302691490489563, 36.9411035321518781, 61.1167419575615725);
            Arc arc3 = new(0.9150298597915126, 1.5697777604261320, 0.5365077762296626, 60.5758363679647545, 77.0406837634733819);
            Arc arc4 = new(0.8679928269291335, 1.3844270898032700, 0.7276985077886839, 76.7042919975651216, 90.1280108914693869);
            Arc arc5 = new(0.8647411648347978, 1.3844270898032696, 0.7276985077886844, 89.8719891085306273, 103.2957080024348642);
            Arc arc6 = new(0.8177041319724192, 1.5697777604261309, 0.5365077762296641, 102.9593162365266465, 119.4241636320352455);
            Arc arc7 = new(0.7003413761524744, 1.7720412332222060, 0.3026914904895616, 118.8832580424383423, 143.0588964678481716);
            Arc arc8 = new(0.5927922215935331, 1.8531050558148148, 0.1680136484100012, 143.1114784289678710, 179.0444338065635748);
            Arc arc9 = new(0.5927922215935328, 1.8587089841050546, 0.1680136484100009, 180.9555661934363684, 216.8885215710321006);
            Arc arc10 = new(0.7003413761524755, 1.9397728066976647, 0.3026914904895631, 216.9411035321518568, 241.1167419575615725);
            Arc arc11 = new(0.8177041319724196, 2.1420362794937402, 0.5365077762296656, 240.5758363679647402, 257.0406837634733392);
            Arc arc12 = new(0.8647411648347973, 2.3273869501165962, 0.7276985077886806, 256.7042919975651216, 270.1280108914693869);
            Arc arc13 = new(0.8679928269291336, 2.3273869501165985, 0.7276985077886831, 269.8719891085306131, 283.2957080024348784);
            Arc arc14 = new(0.9150298597915123, 2.1420362794937384, 0.5365077762296634, 282.9593162365266608, 299.4241636320352313);
            Arc arc15 = new(1.0323926156114558, 1.9397728066976647, 0.3026914904895630, 298.8832580424383991, 323.0588964678481148);
            Arc arc16 = new(1.1399417701703980, 1.8587089841050548, 0.1680136484100013, 323.1114784289678710, 359.0444338065635748);
            List<Entity> entities = new List<Entity>() { arc1, arc5, arc7, arc9, arc2, arc4, arc3, arc16, arc11, arc10, arc12, arc6, arc13, arc8, arc14, arc15 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group2A));
        }

        [Test]
        public void CheckGroup2A_EllipseMadeOfArcs_ReturnsFalse()
        {
            Arc arc1 = new(1.1399417701703984, 1.8531050558148150, 0.1680136484100009, 0.9555661934363535, 36.8885215710321290);
            Arc arc2 = new(1.0323926156114558, 1.7720412332222049, 0.302691490489563, 36.9411035321518781, 61.1167419575615725);
            Arc arc3 = new(0.9150298597915126, 1.5697777604261320, 0.5365077762296626, 60.5758363679647545, 77.0406837634733819);
            Arc arc4 = new(0.8679928269291335, 1.3844270898032700, 0.7276985077886839, 76.7042919975651216, 90.1280108914693869);
            Arc arc5 = new(0.8647411648347978, 1.3844270898032696, 0.7276985077886844, 89.8719891085306273, 103.2957080024348642);
            Arc arc6 = new(0.8177041319724192, 1.5697777604261309, 0.5365077762296641, 102.9593162365266465, 119.4241636320352455);
            Arc arc7 = new(0.7003413761524744, 1.7720412332222060, 0.3026914904895616, 118.8832580424383423, 143.0588964678481716);
            Arc arc8 = new(0.5927922215935331, 1.8531050558148148, 0.1680136484100012, 143.1114784289678710, 179.0444338065635748);
            Arc arc9 = new(0.5927922215935328, 1.8587089841050546, 0.1680136484100009, 180.9555661934363684, 216.8885215710321006);
            Arc arc10 = new(0.7003413761524755, 1.9397728066976647, 0.3026914904895631, 216.9411035321518568, 241.1167419575615725);
            Arc arc11 = new(0.8177041319724196, 2.1420362794937402, 0.5365077762296656, 240.5758363679647402, 257.0406837634733392);
            Arc arc12 = new(0.8647411648347973, 2.3273869501165962, 0.7276985077886806, 256.7042919975651216, 270.1280108914693869);
            Arc arc13 = new(0.8679928269291336, 2.3273869501165985, 0.7276985077886831, 269.8719891085306131, 283.2957080024348784);
            Arc arc14 = new(0.9150298597915123, 2.1420362794937384, 0.5365077762296634, 282.9593162365266608, 299.4241636320352313);
            Arc arc15 = new(1.0323926156114558, 1.9397728066976647, 0.3026914904895630, 298.8832580424383991, 323.0588964678481148);
            Arc arc16 = new(1.1399417701703980, 1.8587089841050548, 0.5, 323.1114784289678710, 359.0444338065635748);
            List<Entity> entities = new List<Entity>() { arc1, arc5, arc7, arc9, arc2, arc4, arc3, arc16, arc11, arc10, arc12, arc6, arc13, arc8, arc14, arc15 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Punch));
        }

        [Test]
        public void CheckGroup2A_VerticalBowtieWithSharpCorners_ReturnsTrue()
        {
            Line line1 = new Line(0, 0, 0, 5);
            Line line2 = new Line(4, 0, 4, 5);
            Arc arc1 = new Arc(2, 0, 2, 0, 180);
            Arc arc2 = new Arc(2, 5, 2, 180, 0);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group2A));
        }

        [Test]
        public void CheckGroup2A_HorizontalBowtieWithSharpCorners_ReturnsTrue()
        {
            Line line1 = new Line(2, 2, 7, 2);
            Line line2 = new Line(2, -2, 7, -2);
            Arc arc1 = new Arc(2, 0, 2, 270, 90);
            Arc arc2 = new Arc(7, 0, 2, 90, 270);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group2A));
        }

        [Test]
        public void CheckGroup2A_RotatedBowtieWithSharpCorners_ReturnsTrue()
        {
            Line line1 = new Line(1, 1, 5, 5);
            Line line2 = new Line(3, -1, 7, 3);
            Arc arc1 = new Arc(6, 4, Math.Sqrt(2), 135, 315);
            Arc arc2 = new Arc(2, 0, Math.Sqrt(2), 315, 135);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group2A));
        }

        [Test]
        public void CheckGroup2A_HorizontalBowtieWithSharpCornersWithBigArcs_ReturnsTrue()
        {
            Line line1 = new Line(15, 3, 21, 3);
            Line line2 = new Line(15, -3, 21, -3);
            Arc arc1 = new Arc(25, 0, 5, (Math.Atan(3.0 / -4.0) + Math.PI) * 180 / Math.PI, (Math.Atan(3.0 / 4.0) + Math.PI) * 180 / Math.PI);
            Arc arc2 = new Arc(11, 0, 5, (Math.Atan(-3.0 / 4.0) * 180 / Math.PI), (Math.Atan(3.0 / 4.0)) * 180 / Math.PI);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Group2A));
        }

        [Test]
        public void CheckGroup2A_RandomHorizontalShapeWith2ArcsAndLines_ReturnsFalse()
        {
            Line line1 = new Line(2, 2, 7, 2);
            Line line2 = new Line(2, -2, 7, -2);
            Arc arc1 = new Arc(2, 0, 2, 90, 270);
            Arc arc2 = new Arc(7, 0, 2, 90, 270);
            List<Entity> entities = new List<Entity>() { line1, arc1, line2, arc2 };
            Feature feature = new(entities) { baseEntityList = entities };
            feature.DetectFeatures();
            Assert.That(feature.FeatureType, Is.EqualTo(PossibleFeatureTypes.Punch));
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
