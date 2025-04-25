using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;

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
            doc = reader.Read();
            ParseFile();
        }

        public DWGFile(Stream stream)
        {
            DwgReader reader = new DwgReader(stream);
            doc = reader.Read();
            ParseFile();
        }
    }
}
