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

        /*
         * Handles an uploaded file by performing feature detection based on its extension
         * and returning the results in JSON format for the frontend.
         *
         * Supported file types: .dxf, .dwg
         *
         * @param file The uploaded file as an IFormFile object.
         * @return 
         *   - string: The resulting JSON string if successful;
         *
         */
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
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(new JsonPackage(touchingEntityList, supportedFile.FeatureGroups), settings);
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