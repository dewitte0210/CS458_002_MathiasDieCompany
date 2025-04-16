namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// A container for a chamfered line and the lines adjacent to it.
/// Makes it easier to keep track of and manipulate chamfers.
/// Does not derive from Entity.
/// </summary>
/// <param name="lineA"> reference to first line </param>
/// <param name="chamfer"> reference to chamfered line </param>
/// <param name="lineB"> reference to second line </param>
public class ChamferGroup(ref Line lineA, ref Line chamfer, ref Line lineB)
{
    public bool Confirmed { get; set; } = false;
    public Line Chamfer { get; } = chamfer;
    public Line LineA { get; set; } = lineA;
    public Line LineB { get; set; } = lineB;
}