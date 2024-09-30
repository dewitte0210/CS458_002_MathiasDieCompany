using System;
using System.IO;
using System.Numerics;

public class Circle : Entity
{
    private decimal xPoint, yPoint, radius;
    private Circle()
    { }

    public Circle(decimal xPoint, decimal yPoint, decimal radius)
    {
        entityType = PossibleEntityTypes.circle;
        this.xPoint=xPoint;
        this.yPoint=yPoint;
        this.radius=radius;
    }
}
