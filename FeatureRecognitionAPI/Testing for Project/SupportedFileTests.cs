using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureRecognitionAPI.Models;

namespace Testing_for_Project
{
    internal class SupportedFileTests
    {
        [Test]
        public void makeTouchingEntitiesList_HappyPath_ReturnTrue()
        {
            Line line1 = new(7, 4, 7, 6);
            Line line2 = new(9, 4, 9, 6);
            Circle circle1 = new(4, 4, 1);
            Line line3 = new(2, 2, 12, 2);
            Line line4 = new(12, 8, 2, 8);
            Arc arc1 = new(8, 6, 1, 0, 180);
            Arc arc2 = new(8, 4, 1, 180, 0);
            Line line5 = new(2, 2, 2, 8);
            Line line6 = new(12, 2, 12, 8);
            DXFFile dxf = new("dfghj");
            List<Entity> entities = new List<Entity>() { line1, line2, circle1, line3, line4, arc1, arc2, line5, line6 };
            List<List<Entity>> expectedTouchingEntities = new List<List<Entity>>();
            expectedTouchingEntities.Add(new List<Entity>(){ line1, arc1, line2, arc2 });
            expectedTouchingEntities.Add(new List<Entity>(){ circle1 });
            expectedTouchingEntities.Add(new List<Entity>(){ line3, line5, line4, line6 });
            List<List<Entity>> actualTouchingEntites = dxf.makeTouchingEntitiesList(entities);
            Assert.That(actualTouchingEntites ==  expectedTouchingEntities);
        }
    }
}
