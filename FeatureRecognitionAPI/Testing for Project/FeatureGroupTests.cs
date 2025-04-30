using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Features;

namespace Testing_for_Project
{
    internal class FeatureGroupTests
    {
        public int GetTotalFeatureGroups(SupportedFile file)
        {
            int tmp = 0;
            foreach (FeatureGroup fGroup in file.FeatureGroups)
            {
                tmp += fGroup.NumIdenticalFeatureGroups;
            }

            return tmp;
        }
        [Test]

        public void TestFeatureGroupExample1()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-001.dwg";

            DWGFile dwgFile = new DWGFile(path);

            dwgFile.SetFeatureGroups();

            //Check that we have the correct number of feature groups (1 from example 1)
            Assert.That(dwgFile.FeatureGroups.Count, Is.EqualTo(1));
            //Check that the one feature group has a count of 2
            Assert.That(GetTotalFeatureGroups(dwgFile), Is.EqualTo(2));
        }

        [Test]
        public void TestFeatureGroupExample2()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-002.dxf";

            DXFFile dxfFile = new DXFFile(path);

            dxfFile.SetFeatureGroups();

            //Check that we have the correct number of feature groups (1 from example 2)
            Assert.That(dxfFile.FeatureGroups.Count, Is.EqualTo(1));
            //Check that the one feature group has a count of 6
            Assert.That(GetTotalFeatureGroups(dxfFile), Is.EqualTo(6));
        }

        [Test]
        public void TestFeatureGroupExample3()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-003.dxf";

            DXFFile dxfFile = new DXFFile(path);

            dxfFile.SetFeatureGroups();

            //Check that we have the correct number of feature groups (1 from example 3)
            Assert.That(dxfFile.FeatureGroups.Count, Is.EqualTo(1));
            //Check that the one feature group has a count of 2
            Assert.That(GetTotalFeatureGroups(dxfFile), Is.EqualTo(2));
        }

        [Test]
        public void TestFeatureGroupExample4()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-004.dxf";

            DXFFile dxfFile = new DXFFile(path);

            dxfFile.SetFeatureGroups();

            //Check that we have the correct number of feature groups (1 from example 3)
            Assert.That(dxfFile.FeatureGroups.Count, Is.EqualTo(1));
            //Check that the one feature group has a count of 2
            Assert.That(GetTotalFeatureGroups(dxfFile), Is.EqualTo(1));
        }
    }
}
