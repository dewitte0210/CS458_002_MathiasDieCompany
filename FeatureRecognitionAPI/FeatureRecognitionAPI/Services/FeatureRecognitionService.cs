using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Converters;
using FeatureRecognitionAPI.Models.Features;
using iText.Kernel.Pdf;

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
                        ext += char.ToLower(fileName[i]);
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

        /*
        * Handles an uploaded file by performing feature detection based on its extension 
        * and returning the results in JSON format for the frontend.
        * 
        * Supported file types: .dxf, .dwg
        * 
        * @param file The uploaded file as an IFormFile object.
        * @return A tuple containing:
        *   - OperationStatus: The status of the operation, such as OK, BadRequest, or specific error types.
        *   - string: The resulting JSON string if successful; null otherwise.
        *   
        * Note: PDF support is currently commented out and will be implemented in the future.
        * 
        * Exceptions:
        * - Returns specific OperationStatus values for unsupported, corrupt, or external API-related errors.
        */
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
                List<FeatureGroup> featureGroups;
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                switch (ext)
                {
                    // This is where all the operations on the file are run from
                    case ".dxf":
                        using (var dxfStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            DXFFile dXFFile = new DXFFile(dxfStream.Name);
                            // Create the touching entity list
                            touchingEntityList = dXFFile.makeTouchingEntitiesList(dXFFile.GetEntities());
                            // touchingEntityList = condenseArcs(touchingEntityList); TODO: not implemented yet
                            // Set the feature groups
                            featureGroups = dXFFile.SetFeatureGroups(touchingEntityList);
                            // Set features for each feature group
                            for (int i = 0; i < featureGroups.Count; i++)
                            {
                                features = dXFFile.makeFeatureList(featureGroups[i].touchingEntities);
                                featureGroups[i].setFeatureList(features);
                            }
                            // Create JSON that will be sent to the frontend
                            json = JsonConvert.SerializeObject(featureGroups, settings);
                        }
                        break;
                    case ".dwg":
                        // Works exactly the same as DXF above, just with a different file as the parser functions are different
                        using (var dwgStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            DWGFile dWGFile = new DWGFile(dwgStream.Name);
                            touchingEntityList = dWGFile.makeTouchingEntitiesList(dWGFile.GetEntities());
                            featureGroups = dWGFile.SetFeatureGroups(touchingEntityList);
                            for (int i = 0; i < featureGroups.Count; i++)
                            {
                                features = dWGFile.makeFeatureList(featureGroups[i].touchingEntities);
                                featureGroups[i].setFeatureList(features);
                            }
                            json = JsonConvert.SerializeObject(featureGroups, settings);
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

                return (OperationStatus.ExternalApiFailure, ex.ToString());
            }
        }
        //not implemented yet
        // private List<List<Entity>> condenseArcs(List<List<Entity>> entities)
        // {
        //     
        // }    
    }
}
