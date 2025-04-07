using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models
{
    public class DWGFile : SupportedFile
    {
        public DWGFile(string path) : base(path)
        {
            FileType = SupportedExtensions.dwg;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            //Also sets file version
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
