/*
 * SupportedFile child that implements a PDF instance
 * will require the use of entities by parsing the data into them
 * then features will be determined based off the entities
 */
using FeatureRecognitionAPI.Models.Enums;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text;

namespace FeatureRecognitionAPI.Models
{
    public class PDFFile : SupportedFile
    {
        public PDFFile(string path) : base(path)
        {
            FileType = SupportedExtensions.pdf;
        }

        public string ExtractTextFromPDF()
        {
            PdfReader reader = new PdfReader(Path);
            PdfDocument pdfDoc = new PdfDocument(reader);
            StringBuilder text = new StringBuilder();

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                text.Append(currentText);
            }

            return text.ToString();
        }

        private void AnalyzeShapesInPDF(PdfDocument pdfDoc)
        {
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                PdfPage page = pdfDoc.GetPage(i);
                PdfCanvasProcessor processor = new PdfCanvasProcessor(new RenderListener());
                processor.ProcessPageContent(page);

            }
        }

        /*
         * Finds all entities withing the file and stores them in EntityList
         * Returns false if some error occurs, otherwise returns true
         */
        public bool findEntities()
        {
            return false;
        }

        public override void readEntities()
        {
            throw new NotImplementedException();
        }
        
        public override List<Entity> GetEntities()
        {
            throw new NotImplementedException();
        }
    }

    public class RenderListener : IEventListener
    {
        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_PATH)
            {
                PathRenderInfo renderInfo = (PathRenderInfo)data;
                // Analyze the Path to detect lines and arcs
                foreach (Subpath subpath in renderInfo.GetPath().GetSubpaths())
                {
                    foreach (IPathSegment segment in subpath.GetSegments())
                    {
                        if (segment is LineSegment)
                        {
                            // Handle line segment
                        }
                        else if (segment is BezierCurve)
                        {
                            // Handle arc segment
                        }
                    }
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new List<EventType> { EventType.RENDER_PATH };
        }
    }
}
