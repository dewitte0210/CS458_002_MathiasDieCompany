using FeatureRecognitionAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing_for_Project
{
    internal class FeatureGroupTests
    {
        [Test]

        public void TestFeatureGroup()
        {
            string path2 = Directory.GetCurrentDirectory();
            int stringTrim = path2.IndexOf("Testing");
            string path = path2.Substring(0, stringTrim) + "FeatureRecognitionAPI\\ExampleFiles\\Example-001.dwg";

            DWGFile dwgFile = new DWGFile(path);

            dwgFile.SetFeatureGroups();

            Assert.That(dwgFile.GetFeatureGroupsCount(), Is.EqualTo(1));
        }
    }
}
