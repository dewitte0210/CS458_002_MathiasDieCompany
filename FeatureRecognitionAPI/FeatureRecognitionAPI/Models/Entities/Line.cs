﻿using FeatureRecognitionAPI.Models.Utility;
using static FeatureRecognitionAPI.Models.Utility.MdcMath;

namespace FeatureRecognitionAPI.Models.Entities;

public class Line : Entity
{
    // Don't Delete. Called from ExtendedLine constructor.
    protected Line()
    {
        Start = new Point();
        End = new Point();
    }

    public Line(Line line)
    {
        Start = new Point(line.Start);
        End = new Point(line.End);
    }

    public Line(double startX, double startY, double endX, double endY)
    {
        Start = new Point(startX, startY);
        End = new Point(endX, endY);
    }

    public Line(Point startPoint, Point endPoint)
    {
        Start = new Point(startPoint);
        End = new Point(endPoint);
    }

    // Constructor with extendedLine parameter.
    public Line(double startX, double startY, double endX, double endY, bool extendedLine)
    {
        Start = new Point(startX, startY);
        End = new Point(endX, endY);
    }

    public Line SwapStartEnd()
    {
        return new Line(End.X, End.Y, Start.X, Start.Y);
    }

    public bool HasPoint(Point point)
    {
        return (Start.Equals(point) || End.Equals(point));
    }

    public override double GetLength()
    {
        return Point.Distance(Start, End);
    }

    public double GetSlopeX()
    {
        return End.X - Start.X;
    }

    public double GetSlopeY()
    {
        return End.Y - Start.Y;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Line lineComp
            && ((Start.Equals(lineComp.Start) && End.Equals(lineComp.End))
            || (Start.Equals(lineComp.End) && End.Equals(lineComp.Start))))
        {
            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public override double MinX()
    {
        return Math.Min(Start.X, End.X);
    }

    public override double MinY()
    {
        return Math.Min(Start.Y, End.Y);
    }

    public override double MaxX()
    {
        return Math.Max(Start.X, End.X);
    }

    public override double MaxY()
    {
        return Math.Max(Start.Y, End.Y);
    }

    public Point GetDelta()
    {
        return new Point(End.X - Start.X, End.Y - Start.Y);
    }

    public override Line Transform(Matrix3 transform)
    {
        Point newStart = transform * Start;
        Point newEnd = transform * End;
        return new Line(newStart, newEnd);
    }
}