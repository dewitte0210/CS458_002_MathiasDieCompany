﻿using FeatureRecognitionAPI.Models;
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
                    List<List<Entity>> touchingEntityList;
                    List<Feature> features;
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter());
                    switch (ext)
                    {
                        case ".dxf":
                            DXFFile dXFFile = new DXFFile(path); // future TODO? make readEntities asynchronous,
                            touchingEntityList = dXFFile.makeTouchingEntitiesList(dXFFile.GetEntities());
                            //might be slow for large files with mutliple users hitting endpoint at once

                            features = dXFFile.getFeatureList(touchingEntityList);

                            json = JsonConvert.SerializeObject(features, settings);
                            break;
                        case ".dwg":
                            DWGFile dwgFile = new DWGFile(path);
                            touchingEntityList = dwgFile.makeTouchingEntitiesList(dwgFile.GetEntities());
                            //might be slow for large files with mutliple users hitting endpoint at once

                            features = dwgFile.getFeatureList(touchingEntityList);
                            json = JsonConvert.SerializeObject(features, settings);
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
