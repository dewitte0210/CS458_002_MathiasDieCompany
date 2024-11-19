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

            //Check that we have the correct number of feature groups (1 from example 1)
            Assert.That(dwgFile.GetFeatureGroupCount(), Is.EqualTo(1));
            //Check that the one feature group has a count of 2
            Assert.That(dwgFile.GetTotalFeatureGroups(), Is.EqualTo(2)); 
        }
    }
}
