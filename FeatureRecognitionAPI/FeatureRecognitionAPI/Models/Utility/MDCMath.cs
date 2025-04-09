namespace FeatureRecognitionAPI.Models.Utility
{
    public static class MdcMath
    {
        /// <summary>
        /// Compare if two doubles are equal within a tolerance
        /// </summary>
        public static bool DoubleEquals(double a, double b, double tolerance = 0.001)
        {
            return (a > b - tolerance && a < b + tolerance);
        }
    }
}