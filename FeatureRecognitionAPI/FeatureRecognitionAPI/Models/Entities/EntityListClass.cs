/*
 * This class is for an object to hold all features within a DWG, DXF, or PDF file
 */
using System;

namespace FeatureRecognitionAPI.Models
{
    public class EntityListClass
    {
        private List<Entity> entityList = [];
        //Refers to to the current index at the 'end' of the entityList
        private int entityIndex;
        public EntityListClass()
        {
            //set entity index to -1 to indicate empty list
            entityIndex = -1;
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

        public bool addEntity(Entity entity)
        {
            entityList.Add(entity);
            entityIndex++; 
            return true;
        }
    }
}
