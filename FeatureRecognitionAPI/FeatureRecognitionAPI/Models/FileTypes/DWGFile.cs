using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models
{
    public class DWGFile : SupportedFile
    {
        public DWGFile(string path) : base(path)
        {
            fileType = SupportedExtensions.dwg;

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
            DwgReader reader = new DwgReader(path);
            CadDocument doc = reader.Read();
            _fileVersion = GetFileVersion(doc.Header.VersionString);
            ReadEntities(doc);
        }
        //Throws exeption if file version is unsupported, formatted as "File version not supported: VERSIONHERE"

        public FileVersion GetFileVersion(string version)
        {

            switch (version)
            {
                case "AC1006":
                    return FileVersion.AutoCad10;
                case "AC1009":
                    return FileVersion.AutoCad12;
                case "AC1012":
                    return FileVersion.AutoCad13;
                case "AC1014":
                    return FileVersion.AutoCad14;
                case "AC1015":
                    return FileVersion.AutoCad2000;
                case "AC1018":
                    return FileVersion.AutoCad2004;
                case "AC1021":
                    return FileVersion.AutoCad2007;
                case "AC1024":
                    return FileVersion.AutoCad2010;
                case "AC1027":
                    return FileVersion.AutoCad2013;
                case "AC1032":
                    return FileVersion.AutoCad2018;
                default:
                    return FileVersion.Unknown;
            }
        }

        public override List<Entity> GetEntities()
        {
            return entityList;
        }
    }
}
