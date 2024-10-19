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
            expectedTouchingEntities.Add(new List<Entity>() { line1, arc1, line2, arc2 });
            expectedTouchingEntities.Add(new List<Entity>() { circle1 });
            expectedTouchingEntities.Add(new List<Entity>() { line3, line5, line4, line6 });
            List<List<Entity>> actualTouchingEntites = dxf.makeTouchingEntitiesList(entities);
            bool areEqual = AreListsOfListsEqual<Entity>(actualTouchingEntites, expectedTouchingEntities);
            Assert.IsTrue(areEqual);
        }

        /**
         * Used to check if two lists are equal in their contents
         */
        static bool AreListsOfListsEqual<T>(List<List<T>> list1, List<List<T>> list2)
        {
            // Check if both lists are null
            if (list1 == null && list2 == null) return true;

            // Check if one of the lists is null
            if (list1 == null || list2 == null) return false;

            // Check if the outer lists have the same number of elements
            if (list1.Count != list2.Count) return false;

            // Compare each inner list
            for (int i = 0; i < list1.Count; i++)
            {
                var innerList1 = list1[i];
                var innerList2 = list2[i];

                // Check if both inner lists are null
                if (innerList1 == null && innerList2 == null) continue;

                // Check if one of the inner lists is null
                if (innerList1 == null || innerList2 == null) return false;

                // Check if the inner lists have the same number of elements
                if (innerList1.Count != innerList2.Count) return false;

                // Compare each element in the inner lists
                for (int j = 0; j < innerList1.Count; j++)
                {
                    if (!EqualityComparer<T>.Default.Equals(innerList1[j], innerList2[j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
