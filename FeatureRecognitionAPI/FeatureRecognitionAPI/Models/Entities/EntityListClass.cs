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
public class EntityListClass
{
    private Entity[] entityList;
    //Refers to to the current index at the 'end' of the entityList
    private int entityIndex;
    public EntityListClass()
    {
        //set entity index to -1 to indicate empty list
        entityIndex = -1;
        entityList = new Entity[100];
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
    }

    public bool addEntity(Entity entity)
    {
        //If there is room in the array, add the entity
        if ((entityIndex + 1) < entityList.Length)
        {
            entityIndex++;
            entityList[entityIndex] = entity;
            return true;
        }
        //If there is not room, double entity array size
        else if ((entityIndex + 1) >= entityList.Length)
        {
            Entity[] temp = new Entity[(entityList.Length * 2)];

            for (int i = 0; i < entityList.Length; i++)
            {
                temp[i] = entityList[i];
            }
            entityList = temp;
            //Add Entity
            entityIndex++;
            entityList[entityIndex] = entity;
            return true;
        }
        return false;
    }

}
