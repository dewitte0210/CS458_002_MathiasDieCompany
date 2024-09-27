using System.IO;
using System;

namespace FeatureRecognitionAPI.Services
{
    public class FeatureRecognitionService
    {
        public FeatureRecognitionService() { }

        public static SupportedFile GetFileStructure(string fileName)
        {
            if (File.Exists(fileName))
            {
                string ext = Path.GetExtension(fileName);

                switch (ext)
                {
                    case ".dxf":
                        Console.WriteLine("This is a dxf file");
                        return new DXFFile(fileName);
                    case ".dwg":
                        Console.WriteLine("This is a dwg file");
                        return new DWGFile(fileName);
                    case ".pdf":
                        Console.WriteLine("This is a pdf file");
                        return new PDFFile(fileName);
                    default:
                        Console.WriteLine("ERROR detecting file extension");
                        return null;
                }
            }
            Console.WriteLine("ERROR File does not exist");
            return null;
        }
    }
}
