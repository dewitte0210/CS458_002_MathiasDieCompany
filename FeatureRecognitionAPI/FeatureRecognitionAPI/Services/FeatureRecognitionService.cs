using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Features;
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

        public async Task<(OperationStatus, string?)> GetFileExtension(string fileName)
        {
            if (fileName != null)
            {
                string ext = "";
                bool extBool = false;
                for (int i = 0; i < fileName.Length; i++)
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
                        return (OperationStatus.Ok, ext);
                    case ".dwg":
                        return (OperationStatus.Ok, ext);
                    default:
                        Console.WriteLine("ERROR detecting file extension");
                        return (OperationStatus.Ok, null);
                }
            }

            Console.WriteLine("ERROR File does not exist");
            return (OperationStatus.Ok, null);
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

                if (status != OperationStatus.Ok || ext == null)
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


                SupportedFile supportedFile;
                switch (ext)
                {
                    case ".dxf":
                        using (var dxfStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                             supportedFile = new DXFFile(dxfStream.Name);
                        }

                        break;
                    case ".dwg":
                        using (var dwgStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                             supportedFile = new DWGFile(dwgStream.Name);
                        }

                        break;
                    default:
                        Console.WriteLine("ERROR detecting file extension");
                        return (OperationStatus.BadRequest, null);
                }
                
                // supportedFile.GroupFeatureEntities();
                
                supportedFile.SetEntities(CondenseArcs(supportedFile.GetEntities()));

                supportedFile.DetectAllFeatureTypes();
                
                // Set the feature groups
                List<List<Entity>> touchingEntityList = new List<List<Entity>>();
                foreach (Feature feature in supportedFile.FeatureList)
                {
                    touchingEntityList.Add(feature.EntityList);
                }
                
                // Create JSON that will be sent to the frontend
                var settings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Auto};
                settings.Converters.Add(new StringEnumConverter());
                string json = JsonConvert.SerializeObject(new JsonPackage(touchingEntityList, supportedFile.FeatureGroups), settings);

                return (OperationStatus.Ok, json);
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

        private static List<Entity> CondenseArcs(List<Entity> entities)
        {
            List<Entity> returned = entities.Where(entity => !(entity is Arc)).ToList();

            List<IGrouping<int, Arc>> arcGroups = entities
                .OfType<Arc>()
                .GroupBy(arc => arc.GetHashCode()).ToList();

            List<Arc> arcs = new List<Arc>();
            foreach (var g in arcGroups)
            {
                List<Arc> group = g.ToList();
                Arc initArc = group[0];
                group.RemoveAt(0);

                if (group.Count == 0)
                {
                    arcs.Add(initArc);
                    continue;
                }

                int idx = 0;
                int failCount = 0;
                while (group.Count > 0)
                {
                    if (failCount >= group.Count)
                    {
                        arcs.Add(initArc);
                        idx = 0;
                        initArc = group[0];
                        group.RemoveAt(0);
                        if (group.Count == 0)
                        {
                            arcs.Add(initArc);
                            break;
                        }

                        failCount = 0;
                    }

                    Arc otherArc = group[idx];
                    if (initArc.ConnectsTo(otherArc))
                    {
                        Point center = initArc.Center;
                        double radius = initArc.Radius;

                        bool startAtSmallArcStart =
                            Math.Abs(otherArc.StartAngle + otherArc.CentralAngle - initArc.StartAngle) % 360 <
                            Entity.EntityTolerance;

                        double angleStart = startAtSmallArcStart ? otherArc.StartAngle : initArc.StartAngle;
                        double angleExtent = otherArc.CentralAngle + initArc.CentralAngle;

                        initArc = new Arc(center.X, center.Y, radius, angleStart, angleStart + angleExtent);
                        group.RemoveAt(idx);
                        failCount = 0;
                    }
                    else
                    {
                        failCount++;
                    }

                    if (group.Count == 0)
                    {
                        arcs.Add(initArc);
                        break;
                    }

                    idx = (idx + 1) % group.Count;
                }
            }

            // Convert arcs to circles if necessary
            foreach (Arc arc in arcs)
            {
                if (Math.Abs(arc.CentralAngle - 360) <= Entity.EntityTolerance)
                {
                    returned.Add(new Circle(arc.Center.X, arc.Center.Y, arc.Radius));
                }
                else
                {
                    returned.Add(arc);
                }
            }

            return returned;
        }
    }
}