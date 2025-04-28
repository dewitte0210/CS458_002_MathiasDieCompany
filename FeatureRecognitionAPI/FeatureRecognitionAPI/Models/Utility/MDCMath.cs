namespace FeatureRecognitionAPI.Models.Utility;

public static class MdcMath
{
	/// <summary>
	/// Compare if two doubles are equal within a tolerance
	/// </summary>
	public static bool DoubleEquals(double a, double b, double tolerance = 0.001)
	{
		return (a > b - tolerance && a < b + tolerance);
	}
		
	/// <summary>
	/// Solves the quadratic formula
	/// </summary>
	/// <returns> List of solutions </returns>
	internal static List<double> QuadraticFormula(double a, double b, double c)
	{
		List<double> solns = new List<double>();
		if (a == 0) { return solns; }
		double insideSqrt = Math.Pow(b, 2) - (4 * a * c);
		//Two real solutions
		if (insideSqrt > 0)
		{
			solns.Add(((-1 * b) + Math.Sqrt(insideSqrt)) / (2 * a));
			solns.Add(((-1 * b) - Math.Sqrt(insideSqrt)) / (2 * a));
		}
		//One real solution
		else if (insideSqrt == 0)
		{
			solns.Add((-1 * b) / (2 * a));
		}
		return solns;

	}
}