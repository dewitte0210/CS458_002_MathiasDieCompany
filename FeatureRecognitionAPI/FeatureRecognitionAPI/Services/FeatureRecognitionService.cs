using FeatureRecognitionAPI.Models.Enums;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace FeatureRecognitionAPI.Services
{
    public class FeatureRecognitionService : IFeatureRecognitionService
    {
        public FeatureRecognitionService() { }

        public async Task<(OperationStatus, string)> GetFileExtension(string fileName)
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
                        return (OperationStatus.OK, ext);
                    case ".dwg":
                        Console.WriteLine("This is a dwg file");
                        return (OperationStatus.OK, ext);
                    case ".pdf":
                        Console.WriteLine("This is a pdf file");
                        return (OperationStatus.OK, ext);
                    default:
                        Console.WriteLine("ERROR detecting file extension");
                        return (OperationStatus.OK, null);
                }
            }
            Console.WriteLine("ERROR File does not exist");
            return (OperationStatus.OK, null);
        }

        public async Task<(OperationStatus, List<string>)> UploadFile(IFormFile file)
        {
            try
            {
                string ext = GetFileExtension(file.FileName).Result.Item2;

                // maybe change ExampleFiles directory
                string path = Path.Combine(Directory.GetCurrentDirectory(), "ExampleFiles", file.FileName);

                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);   
                }

                if (File.Exists(path))
                {
                    //Might be unnecessary, but could map to a DTO object to return to client and display features/entities
                    DXFFile dXFFile = new DXFFile(path);

                    //Line to get an array of the lines in the file
                    List<string> lines = File.ReadAllLines(path).ToList();
                    //Print the entire file

                    return (OperationStatus.OK, lines);

                    //        int linesLength = lines.Length;
                    //        int entityIndex = 0;

                    //        //Find the index where entities start
                    //        for (int i = 0; i < linesLength; i++)
                    //        {
                    //            Console.WriteLine(lines[i]);

                    //            if (lines[i] == "ENTITIES")
                    //            {
                    //                entityIndex = i;
                    //                break;
                    //            }
                    //        }

                    //        bool detectingEntities = true;

                    //        int index = entityIndex;

                    //        while (detectingEntities)
                    //        {

                    //            Console.WriteLine(lines[index]);

                    //            switch (lines[index])
                    //            {
                    //                case "LINE":
                    //                    for (int i=index; index < lines.Length; index++, i++)
                    //                    {

                    //                    }
                    //                    return;
                    //                case "ARC":

                    //                case "CIRCLE":

                    //                default:
                    //                    index++;
                    //                    return;
                    //            }




                    //        }
                }
                else
                    return (OperationStatus.BadRequest, new List<string> { "File not recognized" });
            }
            catch (Exception ex)
            {
                return (OperationStatus.ExternalApiFailure, new List<string> { ex.Message });
            }


            
        }
    }
}
