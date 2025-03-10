﻿using FeatureRecognitionAPI.Models;
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
                var settings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Auto};
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
                            touchingEntityList = touchingEntityList.Select(list => CondenseArcs(list)).ToList();
                            // Set the feature groups
                            featureGroups = dXFFile.SetFeatureGroups(touchingEntityList);
                            // Set features for each feature group
                            for (int i = 0; i < featureGroups.Count; i++)
                            {
                                features = dXFFile.makeFeatureList(featureGroups[i].touchingEntities);
                                featureGroups[i].setFeatureList(features);
                            }

                            // Create JSON that will be sent to the frontend
                            json = JsonConvert.SerializeObject(new JsonPackage(touchingEntityList, featureGroups), settings);
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

        private List<Entity> CondenseArcs(List<Entity> entities)
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

                int idx = 0;
                int failCount = 0;
                while (group.Count > 0)
                {
                    if (failCount >= group.Count)
                    {
                        arcs.Add(initArc);
                        idx = 0;
                        initArc = group[0];
                        if (group.Count == 1)
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
                        break;
                    }

                    idx = (idx + 1) % group.Count;
                }

                arcs.Add(initArc);
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