﻿/*
 * Child class from SupportedFiles that handles DWG files.
 */
using ACadSharp;
using ACadSharp.IO;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models
{
    public class DWGFile : SupportedFile
    {
        private FileVersion _fileVersion;
        private string _path;
        public DWGFile(string path) : base(path)
        {
            fileType = SupportedExtensions.dwg;
            _path = path;

            entityList = new List<Entity>();
            if (File.Exists(path))
            {
                try
                {
                    //Also sets file version
                    readEntities();
                }
                catch (Exception ex)
                {
                    //If there is an error reading entities there is a problem w/ dwg file
                    if (ex.Message == "Attempted to read past the end of the stream.")
                        //Corrupt / broken file
                        throw new Exception("Error: Issue with DWG File");
                    else
                        //Unsuported file type
                        throw new Exception("Error with DWG File");
                }
            }
            else
                throw new FileNotFoundException();
        }

        /*
         * Finds all entities withing the file and stores them in entityList
         * Returns false if some error occurs, otherwise returns true
         */
        public bool findEntities()
        {
            //TODO
            return false;
        }

        /*If there is an error with the ACadSharp library reader it throws an exception with 
            message "Attempted to read past the end of the stream." */
        public override void readEntities()
        {
            DwgReader reader = new DwgReader(_path);

            CadDocument doc = reader.Read();

            _fileVersion = GetFileVersion(doc.Header.VersionString);

            CadObjectCollection<ACadSharp.Entities.Entity> entities = doc.Entities;

            for (int i = 0; i < entities.Count(); i++)
            {
                switch (entities[i].ObjectName)
                {
                    case "LINE":
                        {
                            Line lineEntity =
                                new Line(((ACadSharp.Entities.Line)entities[i]).StartPoint.X,
                                ((ACadSharp.Entities.Line)entities[i]).StartPoint.Y,
                                ((ACadSharp.Entities.Line)entities[i]).EndPoint.X,
                                ((ACadSharp.Entities.Line)entities[i]).EndPoint.Y);
                            entityList.Add(lineEntity);
                            break;
                        }
                    case "ARC":
                        {
                            Arc arcEntity =
                                new Arc(((ACadSharp.Entities.Arc)entities[i]).Center.X,
                                ((ACadSharp.Entities.Arc)entities[i]).Center.Y,
                                ((ACadSharp.Entities.Arc)entities[i]).Radius,
                                //Start and end angle return radians, and must be converted to degrees
                                (((ACadSharp.Entities.Arc)entities[i]).StartAngle * (180 / Math.PI)),
                                (((ACadSharp.Entities.Arc)entities[i]).EndAngle * (180 / Math.PI)));
                            entityList.Add(arcEntity);
                            break;
                        }
                    case "CIRCLE":
                        {
                            Circle circleEntity =
                                new Circle(((ACadSharp.Entities.Circle)entities[i]).Center.X,
                                ((ACadSharp.Entities.Circle)entities[i]).Center.Y,
                                ((ACadSharp.Entities.Circle)entities[i]).Radius);
                            entityList.Add(circleEntity);
                            break;
                        }
                    case "ELLIPSE":
                        {
                            Ellipse ellipseEntity =
                                new Ellipse(((ACadSharp.Entities.Ellipse)entities[i]).Center.X,
                                ((ACadSharp.Entities.Ellipse)entities[i]).Center.Y,
                                ((ACadSharp.Entities.Ellipse)entities[i]).EndPoint.X,
                                ((ACadSharp.Entities.Ellipse)entities[i]).EndPoint.Y,
                                ((ACadSharp.Entities.Ellipse)entities[i]).RadiusRatio,
                                ((ACadSharp.Entities.Ellipse)entities[i]).StartParameter,
                                ((ACadSharp.Entities.Ellipse)entities[i]).EndParameter);
                            entityList.Add(ellipseEntity);
                            break;
                        }
                }
            }
        }
        //Throws exeption if file version is unsupported, formatted as "File version not supported: VERSIONHERE"

        public FileVersion GetFileVersion(string version)
        {

            switch (version)
            {
                case "AC1006":
                    return FileVersion.AutoCad10;
                case "AC1009":
                    return FileVersion.AutoCad12;
                case "AC1012":
                    return FileVersion.AutoCad13;
                case "AC1014":
                    return FileVersion.AutoCad14;
                case "AC1015":
                    return FileVersion.AutoCad2000;
                case "AC1018":
                    return FileVersion.AutoCad2004;
                case "AC1021":
                    return FileVersion.AutoCad2007;
                case "AC1024":
                    return FileVersion.AutoCad2010;
                case "AC1027":
                    return FileVersion.AutoCad2013;
                case "AC1032":
                    return FileVersion.AutoCad2018;
                default:
                    return FileVersion.Unknown;
            }


        }

        public List<Entity> GetEntities()
        {
            return entityList;
        }


    }
}
