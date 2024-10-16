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

        public async Task<(OperationStatus, string)> UploadFile(IFormFile file)
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

                string text;
                string json = "";

                if (File.Exists(path))
                {
                    switch (ext)
                    {
                        case ".dxf":
                            DXFFile dXFFile = new DXFFile(path); // future TODO? make readEntities asynchronous,
                            json = JsonConvert.SerializeObject(dXFFile.GetEntities());
                            //might be slow for large files with mutliple users hitting endpoint at once

                            List<Feature> features = dXFFile.getFeatureList();

                            json = JsonConvert.SerializeObject(features);
                            break;
                        case ".dwg":
                            DWGFile dwgFile = new DWGFile(path);
                            json = JsonConvert.SerializeObject(dwgFile);
                            break;
                        case ".pdf":
                            PDFFile pdfFile = new PDFFile(path); //TODO: need more info to extract entities from pdf
                            text = pdfFile.ExtractTextFromPDF();
                            json = JsonConvert.SerializeObject(text);
                            break;
                        default:
                            Console.WriteLine("ERROR detecting file extension");
                            return (OperationStatus.BadRequest, json);
                    }

                    return (OperationStatus.OK, json);

                }
                else
                    return (OperationStatus.BadRequest, null);
            }
            catch (Exception ex)
            {
                return (OperationStatus.ExternalApiFailure, null);
            }


            
        }
    }
}
