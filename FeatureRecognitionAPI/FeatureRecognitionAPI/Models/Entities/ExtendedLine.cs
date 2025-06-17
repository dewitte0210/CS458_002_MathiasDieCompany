namespace FeatureRecognitionAPI.Models.Entities;

public class ExtendedLine : Line
{
    public Line Parent1 { get; set; }
    public Line Parent2 { get; set; }

    public ExtendedLine()
    {
    }

    /**
     * Runs into issues if more than one perimeter feature is on a line.
     * Would show up as an ExtendedLine being a parent. This would also 
     * throw an error when trying to find a Path because the parent would 
     * not be in EntityList.
     */

    /// <summary>
    /// Calls line's default constructor to initialize StartPoint and EndPoint.
    /// </summary>
    /// <param name="parent1"></param>
    /// <param name="parent2"></param>
    public ExtendedLine(Line parent1, Line parent2)
    {
        Parent1 = parent1;
        Parent2 = parent2;
        CalcPoints();
    }

    /// <summary>
    /// Function that calculates StartPoint and EndPoint based off parents.
    /// </summary>
    private void CalcPoints()
    {
        Point pointToExtend;

        // This looks like a lot but all this is doing is finding the closest point on line1 to line2.
        if (Point.Distance(Parent1.Start, Parent2.Start) < Point.Distance(Parent1.End, Parent2.Start))
        {
            /** At this point we know the point to be extended on line1 is the start point,
             * meaning the end point can stay the same.
             * Hence why tempLine end point is set to line1's.
             */
            pointToExtend = new Point(Parent1.Start);
            Start.X = Parent1.End.X;
            Start.Y = Parent1.End.Y;
        }
        else
        {
            pointToExtend = new Point(Parent1.End);
            Start.X = Parent1.Start.X;
            Start.Y = Parent1.Start.Y;
        }

        /** Similar to the one above but finds what point on line2 is farthest 
         * from line1's point to extend.
         */
        if (Point.Distance(pointToExtend, Parent2.Start) > Point.Distance(pointToExtend, Parent2.End))
        {
            End.X = Parent2.Start.X;
            End.Y = Parent2.Start.Y;
        }
        else
        {
            End.X = Parent2.End.X;
            End.Y = Parent2.End.Y;
        }
    }
}