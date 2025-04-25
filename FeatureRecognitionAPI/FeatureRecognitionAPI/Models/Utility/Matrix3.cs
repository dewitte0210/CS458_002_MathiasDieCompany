using CSMath;

namespace FeatureRecognitionAPI.Models.Utility;

/// <summary>
/// Wrapper around Matrix4 so we can reuse rotate, scale, transform, etc. from that class.
/// </summary>
public class Matrix3
{
    private Matrix4 _matrix;

    public Matrix3()
    {
        _matrix = Matrix4.Identity;
    }

    public Matrix3(Matrix4 mtx4)
    {
        _matrix = new Matrix4(
            mtx4.m00, mtx4.m10, mtx4.m20, 0,
            mtx4.m01, mtx4.m11, mtx4.m21, 0,
            mtx4.m02, mtx4.m12, mtx4.m22, 0,
            0, 0, 0, 0);
    }

    public Matrix3(double a, double b, double c, double d, double e, double f, double g, double h, double i)
    {
        _matrix = new Matrix4([
            a, b, c, 0,
            d, e, f, 0,
            g, h, i, 0,
            0, 0, 0, 1
        ]);
    }

    public static Matrix3 Scale(double xScale, double yScale)
    {
        return new Matrix3(
            xScale, 0, 0,
            0, yScale, 0,
            0, 0, 1);
    }


    public static Matrix3 Rotate(double radians)
    {
        double sinTheta = Math.Sin(radians);
        double cosTheta = Math.Cos(radians);
        return new Matrix3(
            cosTheta, -sinTheta, 0.0,
            sinTheta, cosTheta, 0.0,
            0.0, 0.0, 1.0
        );
    }

    public static Matrix3 Translate(double x, double y)
    {
        return new Matrix3(
            1, 0, x,
            0, 1, y,
            0, 0, 1);
    }

    public Matrix4 GetUnderlyingMatrix()
    {
        return _matrix;
    }

    public static Matrix3 operator *(Matrix3 a, Matrix3 b) => new(Matrix4.Multiply(a._matrix, b._matrix));

    public static Point operator *(Matrix3 matrix, Point value)
    {
        XYZM right = new(value.X, value.Y, 1, 1.0);
        List<XYZM> rows = matrix._matrix.GetRows();
        return new Point()
        {
            X = rows[0].Dot(right),
            Y = rows[1].Dot(right),
        };
    }
}