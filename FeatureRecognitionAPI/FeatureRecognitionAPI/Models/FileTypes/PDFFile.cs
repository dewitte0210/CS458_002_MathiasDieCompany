/*
 * SupportedFile child that implements a PDF instance
 * will require the use of entities by parsing the data into them
 * then features will be determined based off the entities
 */
using System;

public class PDFFile : SupportedFile
{
    public PDFFile(string path) : base(path)
    {
        fileType = SupportedExtensions.pdf;
    }
   
    /*
     * Finds all entities withing the file and stores them in entityList
     * Returns false if some error occurs, otherwise returns true
     */
    public bool findEntities()
    {
        //TODO
        return false;
    }

    /*
     * Finds all features in the file based of the entities within the file
     * calls findEntities to garuantee an updated entity list
     * returns false if fails, otherwise returns true
     */
    public override bool findFeatures()
    {
        if (findEntities())
        {
            //TODO
        }
        return false;
    }

    public override void readEntities()
    {
        throw new NotImplementedException();
    }
}
