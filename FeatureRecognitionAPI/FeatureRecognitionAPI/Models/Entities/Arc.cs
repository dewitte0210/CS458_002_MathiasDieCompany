using System;
using System.IO;
using System.Numerics;

public class Arc : Entity
{
    private decimal xPoint, yPoint, radius, startAngle, endAngle;

    private Arc()
    {
        //left empty because private (unusable)
    }

    public Arc(decimal xPoint, decimal yPoint, decimal radius, decimal startAngle, decimal endAngle)
    {
        entityType = PossibleEntityTypes.arc;
        this.xPoint=xPoint;
        this.yPoint=yPoint;
        this.radius=radius;
        this.startAngle=startAngle;
        this.endAngle=endAngle;
    }
}
