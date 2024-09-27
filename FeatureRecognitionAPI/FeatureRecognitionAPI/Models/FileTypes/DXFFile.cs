/*
 * SupportedFile child that implements a DXF instance
 * I believe this will not require the use of Entities and can instead just read the data straight into features
 */
using System;

namespace FeatureRecognitionAPI.Models
{
    public class DXFFile : SupportedFile
    {
        public DXFFile(string path) : base(path)
        {
            fileType = SupportedExtensions.dxf;
        }

        public override bool findFeatures()
        {
            //TODO
            return false;
        }

        public override void readEntities()
        {
            throw new NotImplementedException();
        }
    }
}
