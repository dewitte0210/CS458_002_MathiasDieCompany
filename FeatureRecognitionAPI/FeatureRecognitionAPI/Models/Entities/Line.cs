using System;
using System.IO;
using System.Numerics;

public class Line : Entity
{
    private decimal xStart, yStart, xEnd, yEnd;
    private Line()
    {
        //left empty because this is private
    }

    public Line(decimal xStart, decimal xEnd, decimal yStart, decimal yEnd)
    {
        this.xStart = xStart;
        this.yStart = yStart;
        this.xEnd = xEnd;
        this.yEnd = yEnd;
    }
}
