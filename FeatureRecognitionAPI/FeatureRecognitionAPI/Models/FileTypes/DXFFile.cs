using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.FileTypes;

namespace FeatureRecognitionAPI.Models
{
    public class DXFFile : SupportedFile
    {
        public DXFFile() {}
        public DXFFile(List<Entity> entities) : base(entities) {}

        public DXFFile(Stream stream)
        {
            FileType = SupportedExtensions.Dxf;
            DxfReader reader = new DxfReader(stream);
            Doc = reader.Read();
            ParseFile();
        }

        public DXFFile(string path) : base(path)
        {
            FileType = SupportedExtensions.Dxf;

            if (File.Exists(path))
            {
                DxfReader reader = new DxfReader(Path);
                Doc = reader.Read();
                ParseFile();
            }
            else
                throw new FileNotFoundException();
        }
    }
}
