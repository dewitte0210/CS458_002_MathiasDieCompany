﻿/*
 * SupportedFile child that implements a DXF instance
 * I believe this will not require the use of Entities and can instead just read the data straight into features
 */
using FeatureRecognitionAPI.Models.Enums;
using System;
using System.Reflection;

namespace FeatureRecognitionAPI.Models
{
    public class DXFFile : SupportedFile
    {
        private FileVersion _fileVersion;
        private readonly string[] _lines;
        public DXFFile(string path) : base(path)
        {
            entityList = [];
            this.path = path;
            fileType = SupportedExtensions.dxf;
            _fileVersion = GetFileVersion();
            
            if (File.Exists(path))
            {
                _lines = File.ReadAllLines(path); // Save runtime by setting this once
                readEntities();
            }
        }

        public FileVersion GetFileVersion()
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                if (_lines[i] == "$ACADVER")
                {
                    string version = _lines[i + 1];
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
            }
            return FileVersion.Unknown;
        }

        public override bool findFeatures()
        {
            while(featureList.Count > 0)
            {
                Entity curEntity = entityList.First();

            } 
            return true; 
        }

        //Ignore commented lines for Console.WriteLine* these were used in initial testing and writing (may be removed later)
        //Could be further modularlized by breaking internals of switch statements into helper functions (Future todo?)
        public override void readEntities()
        {
            //find and track index where entities begin in file (where parsing into entities starts)
            int index = GetStartIndex(_lines);

            //While we haven't reached the end of the entities section, loop and grab entities
            while ((_lines[index] != "ENDSEC") && (_lines.Length > index))
            {

                index++;
                //Console.WriteLine("In while loop: " + lines[index]);

                switch (_lines[index])
                {
                    case "LINE":

                        index = ParseLine(_lines, index);
                        break;

                    case "ARC":
                        index = ParseArc(_lines, index);
                        break;

                    case "CIRCLE":
                        index = ParseCircle(_lines, index);
                        break;

                    default:
                        break;
                }

            }
        }

        private int GetStartIndex(string[] lines)
        {
            int startIndex = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                // Console.WriteLine(lines[i]);

                if (lines[i] == "ENTITIES")
                {
                    startIndex = i;
                    break;
                }
            }
            return startIndex;
        }

        #region Parse Entities
        private int ParseLine(string[] lines, int index)
        {
            //Variables used to create entity of type Line
            decimal xStart = 0;
            decimal yStart = 0;
            decimal xEnd = 0;
            decimal yEnd = 0;
            //Index must be incremented along with i in order to keep maintain accurate indexing
            while (lines[index] != "  0")
            {
                //Switch based on which data field is being interpreted
                switch (lines[index])
                {

                    case " 10":
                        index++;
                        xStart = decimal.Parse(lines[index]);
                        //Console.WriteLine(xStart + " xStart");
                        break;

                    case " 11":
                        index++;
                        xEnd = decimal.Parse(lines[index]);
                        //Console.WriteLine(xEnd + " xEnd");
                        break;

                    case " 20":
                        index++;
                        yStart = decimal.Parse(lines[index]);
                        //Console.WriteLine(yStart +" yStart");
                        break;

                    case " 21":
                        index++;
                        yEnd = decimal.Parse(lines[index]);
                        //Console.WriteLine(yEnd + " yEnd");
                        break;

                    default:
                        break;

                }
                index++;

            }
            Line lineEntity = new Line(xStart, yStart, xEnd, yEnd);
            entityList.Add(lineEntity);
            return index;
        }

        private int ParseArc(string[] lines, int index)
        {
            decimal xPoint = 0;
            decimal yPoint = 0;
            decimal radius = 0;
            decimal startAngle = 0;
            decimal endAngle = 0;

            while (lines[index] != "  0")
            {
                switch (lines[index])
                {

                    case " 10":
                        index++;
                        xPoint = decimal.Parse(lines[index]);
                        //Console.WriteLine(xPoint + " xPoint");
                        break;

                    case " 20":
                        index++;
                        yPoint = decimal.Parse(lines[index]);
                        //Console.WriteLine(yPoint + " yPoint");
                        break;

                    case " 40":
                        index++;
                        radius = decimal.Parse(lines[index]);
                        //Console.WriteLine(radius +" yStart");
                        break;

                    case " 50":
                        index++;
                        startAngle = decimal.Parse(lines[index]);
                        //Console.WriteLine(startAngle + " startAngle");
                        break;

                    case " 51":
                        index++;
                        endAngle = decimal.Parse(lines[index]);
                        //Console.WriteLine(endAngle + " endAngle");
                        break;

                    default:
                        break;

                }
                index++;
            }
            Arc arcEntity = new Arc(xPoint, yPoint, radius, startAngle, endAngle);
            entityList.Add(arcEntity);
            return index;
        }

        private int ParseCircle(string[] lines, int index)
        {
            decimal xPoint = 0;
            decimal yPoint = 0;
            decimal radius = 0;
            while (lines[index] != "  0")
            {
                switch (lines[index])
                {

                    case " 10":
                        index++;
                        xPoint = decimal.Parse(lines[index]);
                        //Console.WriteLine(xPoint + " xPoint");
                        break;

                    case " 20":
                        index++;
                        yPoint = decimal.Parse(lines[index]);
                        //Console.WriteLine(yPoint + " yPoint");
                        break;

                    case " 40":
                        index++;
                        radius = decimal.Parse(lines[index]);
                        //Console.WriteLine(radius +" yStart");
                        break;

                    default:
                        break;

                }
                index++;
            }
            Circle circleEntity = new Circle(xPoint, yPoint, radius);
            entityList.Add(circleEntity);
            return index;
        } 
        #endregion

        //May need to be refactored depending on if c# handles this by copy or by reference
        public List<Entity> GetEntities()
        {
            return entityList;
        }


    }
}
