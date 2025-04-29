using System.Runtime.CompilerServices;
using FeatureRecognitionAPI.Models.Utility;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Testing_for_Project")]

namespace FeatureRecognitionAPI.Models.Entities;

/// <summary>
/// Abstract class that represents a generic entity in dxf or dwg files
/// </summary>
public abstract class Entity
{
    public Point Start { get; set; }
    public Point End { get; set; }
    public bool KissCut { get; set; }
        
    [JsonIgnore] public List<Entity> AdjList { get; set; }
        
    public const double EntityTolerance = 0.00005;

    protected Entity()
    {
        AdjList = new List<Entity>();
        KissCut = false;
        Start = new Point();
        End = new Point();
    }
        
    public abstract double GetLength();

    public abstract override bool Equals(object? obj);

    /// <returns> Return true when entities compared have similar traits,
    /// length is the same (but start and end point, or mid point can vary) </returns>
    public abstract bool Compare(object? obj);

    public abstract double MinX();
    public abstract double MinY();
    public abstract double MaxX();
    public abstract double MaxY();

    public abstract Entity Transform(Matrix3 transform);
}