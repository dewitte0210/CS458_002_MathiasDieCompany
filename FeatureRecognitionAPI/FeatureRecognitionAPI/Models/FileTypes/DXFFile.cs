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
            DxfReader reader = new DxfReader(stream);
            doc = reader.Read();
            _fileVersion = GetFileVersion(doc.Header.VersionString);
            ReadEntities(doc);
        }

        public DXFFile(string path) : base(path)
        {
            FileType = SupportedExtensions.dxf;

            if (File.Exists(path))
            {
                ParseFile();
            }
            else
                throw new FileNotFoundException();
        }

        public override void ParseFile()
        {
            DxfReader reader = new DxfReader(Path);
            doc = reader.Read();
            _fileVersion = GetFileVersion(doc.Header.VersionString);
            ReadEntities(doc);
        }
    }
}
