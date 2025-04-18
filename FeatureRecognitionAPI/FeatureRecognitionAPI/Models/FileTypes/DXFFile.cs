using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models
{
    public class DXFFile : SupportedFile
    {
        public DXFFile()
        {
        }

        public DXFFile(List<Entity> entities) : base(entities) {}

        public DXFFile(Stream stream)
        {
            FileType = SupportedExtensions.dxf;
            DxfReader reader = new DxfReader(stream);
            doc = reader.Read();
            ParseFile();
        }

        public DXFFile(string path) : base(path)
        {
            FileType = SupportedExtensions.Dxf;

            if (File.Exists(path))
            {
                DxfReader reader = new DxfReader(Path);
                doc = reader.Read();
                ParseFile();
            }
            else
                throw new FileNotFoundException();
        }
    }
}
