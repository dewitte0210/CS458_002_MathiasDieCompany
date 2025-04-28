namespace FeatureRecognitionAPI.Models.Utility;

public static class MdcMath
{
    public const double DoubleTolerance = 0.001;

    /// <summary>
    /// Compare if two doubles are equal within a tolerance by value
    /// </summary>
    /// <param name="a"> first double </param>
    /// <param name="b"> second double </param>
    /// <param name="tolerance"> defaults to 0.001 but can be changed for custom
    /// tolerances like when comparing radians</param>
    /// <returns> true if doubles are within tolerance </returns>
    public static bool DoubleEquals(double a, double b, double tolerance = DoubleTolerance)
    {
        return a > b - tolerance && a < b + tolerance;
    }

    /// <summary>
    /// Solves the quadratic formula
    /// </summary>
    /// <returns> List of solutions </returns>
    internal static List<double> QuadraticFormula(double a, double b, double c)
    {
        List<double> solutions = new();
        if (DoubleEquals(a, 0))
        {
            return solutions;
        }

        double insideSqrt = Math.Pow(b, 2) - (4 * a * c);
        //Two real solutions
        if (insideSqrt > 0)
        {
            solutions.Add(((-1 * b) + Math.Sqrt(insideSqrt)) / (2 * a));
            solutions.Add(((-1 * b) - Math.Sqrt(insideSqrt)) / (2 * a));
        }
        //One real solution
        else if (DoubleEquals(insideSqrt, 0))
        {
            solutions.Add((-1 * b) / (2 * a));
        }

        return solutions;
    }
}