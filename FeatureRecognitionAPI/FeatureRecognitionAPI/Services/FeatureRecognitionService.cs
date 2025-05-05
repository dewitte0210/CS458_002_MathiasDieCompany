using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;
using FeatureRecognitionAPI.Models.FileTypes;
using FeatureRecognitionAPI.Models.Utility;
using iText.Commons.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FeatureRecognitionAPI.Services
{
    public class FeatureRecognitionService : IFeatureRecognitionService
    {
        public FeatureRecognitionService()
        {
        }

        /// <summary>
        /// Handles an uploaded file by performing feature detection based on its extension
        /// and returning the results in JSON format for the frontend.
        /// Supported file types: .dxf, .dwg
        /// </summary>
        /// <param name="file"> The uploaded file as an IFormFile object. </param>
        /// <returns> The resulting JSON string if successful </returns>
        /// <exception cref="IOException"></exception>
        public async Task<string?> UploadFile(IFormFile file)
        {
            string? ext = Path.GetExtension(file.FileName);

            if (ext == null)
            {
                throw new IOException("Error detecting file extension");
            }


            SupportedFile supportedFile;
            using (Stream stream = file.OpenReadStream())
            {
                switch (ext.ToLower())
                {
                    case ".dxf":
                        supportedFile = new DXFFile(stream);

                        break;
                    case ".dwg":
                        supportedFile = new DWGFile(stream);

                        break;
                    default:
                        throw new IOException("Invalid file extension: " + ext);
                }
            }

            supportedFile.SetEntities(EntityTools.CondenseArcs(supportedFile.GetEntities()));

            supportedFile.DetectAllFeatureTypes();
            
            List<Entity> touchingEntityList = new List<Entity>();
            foreach (Feature feature in supportedFile.FeatureList)
            {
                touchingEntityList.AddRange(feature.EntityList);
            }

            // Create JSON that will be sent to the frontend
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(new JsonPackage(touchingEntityList, supportedFile.FeatureGroups), settings);
        }

        
    }
}