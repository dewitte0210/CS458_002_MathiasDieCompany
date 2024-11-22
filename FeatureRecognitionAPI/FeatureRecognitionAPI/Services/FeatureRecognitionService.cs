using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Converters;

namespace FeatureRecognitionAPI.Services
{
    public class FeatureRecognitionService : IFeatureRecognitionService
    {
        public FeatureRecognitionService() { }

        public async Task<(OperationStatus, string?)> GetFileExtension(string fileName)
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

        public async Task<(OperationStatus, string?)> UploadFile(IFormFile file)
        {
            try
            {
                var (status, ext) = await GetFileExtension(file.FileName);

                if (status != OperationStatus.OK || ext == null)
                {
                    Console.WriteLine("ERROR detecting file extension");
                    return (OperationStatus.BadRequest, null);
                }

                // maybe change ExampleFiles directory
                string path = Path.Combine(Directory.GetCurrentDirectory(), "ExampleFiles", file.FileName);

                if (!File.Exists(path))
                {
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                
                string json = "";
                
                List<List<Entity>> touchingEntityList;
                List<Feature> features;
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                switch (ext)
                {
                    case ".dxf":
                        using (var dxfStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            DXFFile dXFFile = new DXFFile(dxfStream.Name);
                            touchingEntityList = dXFFile.makeTouchingEntitiesList(dXFFile.GetEntities());
                            features = dXFFile.makeFeatureList(touchingEntityList);
                            json = JsonConvert.SerializeObject(features, settings);
                        }
                        break;
                    case ".dwg":
                        using (var dwgStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            DWGFile dwgFile = new DWGFile(dwgStream.Name);
                            touchingEntityList = dwgFile.makeTouchingEntitiesList(dwgFile.GetEntities());
                            features = dwgFile.makeFeatureList(touchingEntityList);
                            json = JsonConvert.SerializeObject(features, settings);
                        }
                        break;
                    case ".pdf":
                        using (var pdfStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            PDFFile pdfFile = new PDFFile(pdfStream.Name);
                            var text = pdfFile.ExtractTextFromPDF();
                            json = JsonConvert.SerializeObject(text);
                        }
                        break;
                    default:
                        Console.WriteLine("ERROR detecting file extension");
                        return (OperationStatus.BadRequest, null);
                }

                return (OperationStatus.OK, json);
                
            }
            catch (Exception ex)
            {
                if (ex.Message == "Unsupported DWG File")
                    return (OperationStatus.UnsupportedFileType, ex.Message);
                if (ex.Message == "Error: Issue with DXF File")
                    return (OperationStatus.CorruptFile, ex.Message);
                if (ex.Message == "Error: Issue with DWG File")
                    return (OperationStatus.CorruptFile, ex.Message);

                return (OperationStatus.ExternalApiFailure, null);
            }
        }
    }
}
