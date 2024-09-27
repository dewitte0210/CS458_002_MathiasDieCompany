/*
 * This class is for an object to hold all features within a DWG, DXF, or PDF file
 */
using System;

namespace FeatureRecognitionAPI.Models
{
    public class EntityListClass
    {
        Entity[] entityList;
        public EntityListClass()
        {
        }

        //Convert entityList to a readable array to be printed in a file
        public string[] toStringArray()
        {
            //TODO
            return new string[0];
        }

        //Convert string array, formatted in the same way as the toStringArray method, to entity list
        public void setEntityList(string[] stringFeatureList)
        {

        }
    }
}
