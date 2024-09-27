using System.IO;

namespace FeatureRecognitionAPI.Services
{
    public class FeatureRecognitionService
    {
        public FeatureRecognitionService() { }

        public static string GetFileStructure(string fileName)
        {
            if (fileName != null)
            {
                string ext = "";
                bool extBool = false;
                for (int i = 0; i < fileName.Length;i++)
                {
                    if (fileName[i] == '.')
                    {
                        extBool = true;
                    }
                    if (extBool)
                    {
                        ext += fileName[i];
                    }
                }

                switch (ext)
                {
                    case ".dxf":
                        Console.WriteLine("This is a dxf file");
                        return ext;
                    case ".dwg":
                        Console.WriteLine("This is a dwg file");
                        return ext;
                    case ".pdf":
                        Console.WriteLine("This is a pdf file");
                        return ext;
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
