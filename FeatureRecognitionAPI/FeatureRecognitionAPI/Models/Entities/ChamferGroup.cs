namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// A container for a chamfered line and the lines adjacent to it.
/// Makes it easier to keep track of and manipulate chamfers.
/// Does not derive from Entity.
/// </summary>
/// <param name="lineAIndex"> reference to first line </param>
/// <param name="chamferIndex"> reference to chamfered line </param>
/// <param name="lineBIndex"> reference to second line </param>
public class ChamferGroup(int lineAIndex, int chamferIndex, int lineBIndex, Line lineA, Line chamfer, Line lineB)
{
    public bool Confirmed { get; set; } = false;
    public int ChamferIndex { get; } = chamferIndex;
    public int LineAIndex { get; set; } = lineAIndex;
    public int LineBIndex { get; set; } = lineBIndex;

    public Line LineA { get; set; } = lineA;
    public Line Chamfer { get; set; } = chamfer;
    public Line LineB { get; set; } = lineB;
}