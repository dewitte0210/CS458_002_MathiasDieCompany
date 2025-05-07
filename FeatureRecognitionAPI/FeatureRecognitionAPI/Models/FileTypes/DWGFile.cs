using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.FileTypes;

namespace FeatureRecognitionAPI.Models
{
    public class DWGFile : SupportedFile
    {
        public DWGFile(string path) : base(path)
        {
            FileType = SupportedExtensions.Dwg;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            DwgReader reader = new DwgReader(path);
            Doc = reader.Read();
            ParseFile();
        }

        public DWGFile(Stream stream)
        {
            DwgReader reader = new DwgReader(stream);
            Doc = reader.Read();
            ParseFile();
        }
    }
}
