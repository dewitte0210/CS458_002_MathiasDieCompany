namespace FeatureRecognitionAPI.Models.Dtos
{
    public class EntityDto
    {

        public StartPointDto? StartPoint { get; set; }

        public EndPointDto? EndPoint { get; set; }

        public double? SlopeY { get; set; }

        public double? SlopeX { get; set; }

        public bool? ExtendedLine { get; set; }

        public double? Length { get; set; }

        public double? CenterX { get; set; }

        public double? CenterY { get; set; }

        public double? StartX { get; set; }

        public double? StartY { get; set; }

        public double? EndX { get; set; }

        public double? EndY { get; set; }

        public double? Radius { get; set; }

        public double? StartAngle { get; set; }

        public double? EndAngle { get; set; }

        public double? CentralAngle { get; set; }
    }

    public class StartPointDto
    {
        public double X { get; set; }

        public double Y { get; set; }

        public bool Intersect { get; set; }
    }

    public class EndPointDto
    {
        public double X { get; set; }

        public double Y { get; set; }

        public bool Intersect { get; set; }
    }
}
