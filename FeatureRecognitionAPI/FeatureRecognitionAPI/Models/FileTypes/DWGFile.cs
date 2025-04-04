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


        /*If there is an error with the ACadSharp library reader it throws an exception with 
            message "Attempted to read past the end of the stream." */
        public override void ParseFile()
        {
            DwgReader reader = new DwgReader(Path);
            doc = reader.Read();
            _fileVersion = GetFileVersion(doc.Header.VersionString);
            ReadEntities(doc);
        }

    }
}
