namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// A container for a chamfered line and the lines adjacent to it.
/// Makes it easier to keep track of and manipulate chamfers.
/// Does not derive from Entity.
/// </summary>
public class ChamferGroup(Line lineA, Line chamfer, Line lineB)
{
    public bool Confirmed { get; set; } = false;
    public Line Chamfer { get; } = chamfer;
    public Line LineA { get; } = lineA;
    public Line LineB { get; } = lineB;
}