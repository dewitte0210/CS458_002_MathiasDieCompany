namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// A container for a chamfered line and the lines adjacent to it.
/// Makes it easier to keep track of and manipulate chamfers.
/// Does not derive from Entity.
/// </summary>
/// <param name="lineAIndex"> Reference to first line. </param>
/// <param name="chamferIndex"> Reference to chamfered line. </param>
/// <param name="lineBIndex"> Reference to second line. </param>
public class ChamferGroup(int lineAIndex, int chamferIndex, int lineBIndex, Line chamfer)
{
    public bool Confirmed { get; set; } = false;

    public Line Chamfer { get; set; } = chamfer;
    public int ChamferIndex { get; } = chamferIndex;

    public int LineAIndex { get; set; } = lineAIndex;
    public int LineBIndex { get; set; } = lineBIndex;
}