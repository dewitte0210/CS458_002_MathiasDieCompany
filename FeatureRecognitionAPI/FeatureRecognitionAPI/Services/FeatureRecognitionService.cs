using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json;
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

        public async Task<(OperationStatus, int)> UploadFile(IFormFile file)
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
                    DXFFile dXFFile = new DXFFile(path);

                    string json = JsonConvert.SerializeObject(dXFFile.GetEntities());

                    return (OperationStatus.OK, dXFFile.GetEntities().Count());

                }
                else
                    return (OperationStatus.BadRequest, 0);
            }
            catch (Exception ex)
            {
                return (OperationStatus.ExternalApiFailure, 0);
            }


            
        }
    }
}
