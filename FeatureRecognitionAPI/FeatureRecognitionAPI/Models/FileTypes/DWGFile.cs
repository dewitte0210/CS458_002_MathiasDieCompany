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

            try
            {
                //Also sets file version
                ParseFile();
            }
            catch (Exception ex)
            {
                //If there is an error reading entities there is a problem w/ dwg file
                if (ex.Message == "Attempted to read past the end of the stream.")
                    //Corrupt / broken file
                    throw new Exception("Error: Issue with DWG File");
                else
                    //Unsuported file type
                    throw new Exception("Error with DWG File");
            }
        }

        public DWGFile(Stream stream)
        {
            DwgReader reader = new DwgReader(stream);
            doc = reader.Read();
            ParseFile();
        }
    }
}
