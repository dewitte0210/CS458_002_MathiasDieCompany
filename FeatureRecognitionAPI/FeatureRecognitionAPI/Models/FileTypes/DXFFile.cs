/*
 * SupportedFile child that implements a DXF instance
 * I believe this will not require the use of Entities and can instead just read the data straight into features
 */
using System;
using System.Reflection;

namespace FeatureRecognitionAPI.Models
{
    public class DXFFile : SupportedFile
    {
        public DXFFile(string path) : base(path)
        {
            entityList = [];
            this.path = path;
            fileType = SupportedExtensions.dxf;
            if (File.Exists(path))
            {
                readEntities();
            }
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
            string[] lines = File.ReadAllLines(path);

            //find and track index where entities begin in file (where parsing into entities starts)
            int index = GetStartIndex(lines);

            //While we haven't reached the end of the entities section, loop and grab entities
            while ((lines[index] != "ENDSEC") && (lines.Length > index))
            {

                index++;
                //Console.WriteLine("In while loop: " + lines[index]);

                switch (lines[index])
                {
                    case "LINE":

                        index = ParseLine(lines, index);
                        break;

                    case "ARC":
                        index = ParseArc(lines, index);
                        break;

                    case "CIRCLE":
                        index = ParseCircle(lines, index);
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

        //May need to be refactored depending on if c# handles this by copy or by reference
        public List<Entity> GetEntities()
        {
            return entityList;
        }


    }
}
