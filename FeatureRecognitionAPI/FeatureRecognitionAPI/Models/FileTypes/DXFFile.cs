using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models
{
    public class DXFFile : SupportedFile
    {
        private string[] _lines;

        public DXFFile()
        {
        }

        public DXFFile(List<Entity> entities) : base(entities) {}

        public DXFFile(string path) : base(path)
        {
            FileType = SupportedExtensions.Dxf;

            if (File.Exists(path))
            {
                ParseFile();
            }
            else
                throw new FileNotFoundException();
        }

        public FileVersion GetFileVersion()
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                if (_lines[i] == "$ACADVER")
                {
                    string version = _lines[i + 1];
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

            return FileVersion.Unknown;
        }

        public override void ParseFile()
        {
            DxfReader reader = new DxfReader(Path);

            CadDocument doc = reader.Read();

            _lines = File.ReadAllLines(Path);
            _fileVersion = GetFileVersion();
            ReadEntities(doc);

        }
    }
}
