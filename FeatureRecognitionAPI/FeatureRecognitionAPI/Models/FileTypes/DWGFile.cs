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


        /*If there is an error with the ACadSharp library reader it throws an exception with 
            message "Attempted to read past the end of the stream." */
        public override void ParseFile()
        {
            DwgReader reader = new DwgReader(Path);
            //Throws exception if file version is unsupported, formatted as "File version not supported: VERSIONHERE"
            CadDocument doc = reader.Read();
            _fileVersion = GetFileVersion(doc.Header.VersionString);
            ReadEntities(doc);
        }
        
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
    }
}
