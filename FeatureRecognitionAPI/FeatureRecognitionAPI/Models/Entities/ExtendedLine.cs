namespace FeatureRecognitionAPI.Models;

public class ExtendedLine : Line
{
    public Line Parent1 { get; set; }
    public Line Parent2 { get; set; }

        public ExtendedLine()
        {
        }
        //runs into issues if more than one perimeter feature is on a line
        //would show up as an ExtendedLine being a parent
        //this would also throw an error when trying to find a Path because the parent would not be in EntityList
        public ExtendedLine(Line parent1, Line parent2) // calls line's default constructor to initialize StartPsoint and EndPoint
        {
            Parent1 = parent1;
            Parent2 = parent2;
            CalcPoints();

            SlopeY = End.Y - Start.Y;
            SlopeX = End.X - Start.X;

            this.Length = Point.Distance(Start, End);
        }

        //Function that calculates StartPoint and EndPoint based off parents
        public void CalcPoints()
        {
            if (Parent1 != null && Parent2 != null)
            {
                Point pointToExtend;
                if (Point.Distance(
                        Parent1.Start,
                        Parent2.Start)
                        < Point.Distance(
                        Parent1.End,
                        Parent2.Start))
                //This looks like a lot but all this is doing is finding the closest point on line1 to line2
                {
                    //At this point we know the point to be extended on line1 is the start point, meaning the end point can stay the same
                    //  Hence why tempLine end point is set to line1's
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
                if (Point.Distance(
                    pointToExtend,
                    Parent2.Start)
                    > Point.Distance(
                    pointToExtend,
                    Parent2.End))
                //Similar to the one above but finds what point on line2 is farthest from line1's point to extend
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
    }
}