using System.IO;
using System;
using FeatureRecognitionAPI.Models;

namespace FeatureRecognitionAPI
{
    public class MyMainClass
    {
        static void Main()
        {
            String path = "";
            Console.WriteLine("Please enter the path to your file");
            path = Console.ReadLine();
            SupportedFile myFile = getFileType(path);

            if (myFile == null)
            {
                Console.WriteLine("ERROR file is null");
                System.Environment.Exit(1);
            }
            myFile.findFeatures();
        }

        /*
            Method to get the File type with specific supported extensions
            @param path string value that contains the path to the file
            @return SupportedFile child with matching file extension
        */
        static SupportedFile getFileType(String path)
        {
            if (File.Exists(path))
            {
                String ext = Path.GetExtension(path);

                switch (ext)
                {
                    case ".dxf":
                        Console.WriteLine("This is a dxf file");
                        return new DXFFile(path);
                    case ".dwg":
                        Console.WriteLine("This is a dwg file");
                        return new DWGFile(path);
                    case ".pdf":
                        Console.WriteLine("This is a pdf file");
                        return new PDFFile(path);
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