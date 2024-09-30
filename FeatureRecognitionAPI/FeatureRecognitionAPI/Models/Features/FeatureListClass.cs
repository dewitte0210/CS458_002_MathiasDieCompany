/*
 * This class is for an object to hold all features within a DWG, DXF, or PDF file
 */
using System;

namespace FeatureRecognitionAPI.Models
{
    public class FeatureListClass
    {
        private List<Feature> featureList = [];
        public FeatureListClass()
        {
        }

        //Convert featureList to a readable array to be printed in a file
        public string[] toStringArray()
        {
            //TODO
            return new string[0];
        }

        //Convert string array, formatted in the same way as the toStringArray method, to feature list
        public void setFeatureList(string[] stringFeatureList)
        {

        }
    }
}
