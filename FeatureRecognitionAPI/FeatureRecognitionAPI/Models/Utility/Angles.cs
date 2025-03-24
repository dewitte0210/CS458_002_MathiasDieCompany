using NHibernate;

/// Author: Andrew Schmidt
/// <summary>
/// This file is used for calculating the angle between lines and on what side they lay
/// </summary>
namespace FeatureRecognitionAPI.Models.Utility
{
	/// <summary>
	/// Angles contains classes and functions for calculating angles
	/// Not to be used directly, use Angle class, Side and Orientation enums, etc
	/// </summary>
	public static class Angles
	{
		public const double ANGLETOLDEG = 0.001;
		public const double ANGLETOLRAD = ANGLETOLDEG * Math.PI / 180;

		/// <summary>
		/// The side an angle lies in relation to the rest of the shape
		/// </summary>
		public enum Side
		{
			INTERIOR,
			EXTERIOR,
			UNKNOWN
		}

		/// <summary>
		/// The general orientation of a polygon
		/// This is the direction its lines are drawn
		/// Ideally all lines of a polygon should have a consistent direction to them
		/// </summary>
		public enum Orientation
		{
			COUNTERCLOCKWISE,
			CLOCKWISE,
			UNKNOWN
		}

		public class Degrees(double angle)
		{
			public readonly double angle = angle;

			public override bool Equals(object? obj)
			{
				if (obj == null) return false;
				
                if (obj is Degrees objD
                    && angle > objD.angle - ANGLETOLDEG
                    && angle < objD.angle + ANGLETOLDEG) 
				{
					return true;
				}
				try
				{
					//if numeric try to cast to double
					double objDbl = Convert.ToDouble(obj);
					if (angle > objDbl - ANGLETOLDEG
						&& angle < objDbl + ANGLETOLDEG)
					{
						return true;
					}
				}
				catch
				{
					return false;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(angle);
			}

			public Degrees GetOppositeAngle()
			{
				return new Degrees(360 - angle);
			}

			public Radians ToRadians()
			{
				return new Radians(angle * Math.PI / 180);
			}
		}

		public class Radians(double angle)
		{
			public readonly double angle = angle;

			public Radians GetOppositeAngle()
			{
				return new Radians((2 * Math.PI) - angle);
			}

			public Degrees ToDegrees()
			{
				return new Degrees(angle * 180 / Math.PI);
			}

			public override bool Equals(object? obj)
			{
				if (obj == null) return false;
				if (obj is Degrees objR
					&& angle > objR.angle - ANGLETOLRAD
					&& angle < objR.angle + ANGLETOLRAD)
				{
					return true;
				}
                try
                {
                    //if numeric try to cast to double
                    double objDbl = Convert.ToDouble(obj);
                    if (angle > objDbl - ANGLETOLDEG
                        && angle < objDbl + ANGLETOLDEG)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
                return false;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(angle);
			}
		}

		/// <summary>
		/// The angle and side that angle is on between two lines
		/// </summary>
		public class Angle
		{
			private readonly Degrees angle;
			private readonly Side side;

			public Angle(Degrees angle, Side side)
			{
				this.angle = angle;
				this.side = side;
			}

			public Angle(Radians angle, Side side)
			{
				this.angle = angle.ToDegrees();
				this.side = side;
			}

			public Degrees GetDegrees()
			{
				return angle;
			}

			public Radians GetRadians()
			{
				return angle.ToRadians();
			}

			public Side GetSide()
			{
				return side;
			}

			public override bool Equals(object? obj)
			{
				if (obj == null) { return false; }

                if (obj is Angle ObjA 
					&& side != Side.UNKNOWN 
					&& ObjA.side != Side.UNKNOWN)
                {
                    if (ObjA.side == this.side && ObjA.angle == this.angle)
                    {
                        return true;
                    }
                    //sides are opposite
                    if (ObjA.side != this.side && ObjA.angle.GetOppositeAngle() == angle)
                    {
                        return true;
                    }
                }
                return false;
			}

			public override int GetHashCode()
			{
				throw new NotImplementedException();
			}
		}


		public static double CrossProduct(Point a, Point b)
		{
			return a.X * b.Y - a.Y * b.X;
		}

		/// <summary>
		/// Calculates the cross product of two lines using their deltas
		/// </summary>
		public static double CrossProduct(Line a, Line b)
		{
			Point aDelta = a.GetDelta();
			Point bDelta = b.GetDelta();
			return aDelta.X * bDelta.Y - aDelta.Y * bDelta.X;
		}

        public static double DotProduct(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }
        
        public static double ToRadians(double degrees)
        {
            return degrees *  Math.PI / 180;
        }

		/// <summary>
		/// Calculates the dot product of two lines using their deltas
		/// </summary>
		public static double DotProduct(Line a, Line b)
		{
			Point aDelta = a.GetDelta();
			Point bDelta = b.GetDelta();
			return aDelta.X * bDelta.X + aDelta.Y * bDelta.Y;
		}

		// may not handle all cases yet, to be tested
		public static Angle GetAngle(Line a, Line b, Side targetSide = Side.INTERIOR, Orientation ori = Orientation.COUNTERCLOCKWISE)
		{
			double cross = CrossProduct(a, b);
			double dot = DotProduct(a, b);
			Side side = Side.UNKNOWN;

			double cos_theta = dot / (a.Length * b.Length);
			cos_theta = Math.Max(-1, Math.Min(1, cos_theta));

			double angle = Math.Acos(cos_theta) * (180 / Math.PI);

			if (cross >= 0)
			{
				side = Side.INTERIOR;
			}
			else if (cross < 0)
			{
				side = Side.EXTERIOR;
				angle = 360 - angle;
			}

			//initially calculates interior angle
			//return opposite if wanting exterior
			Degrees returnAngle = new(Math.Abs(180 - angle));

			//prefer 0 degrees interior over 360 for opposite facing parallel lines
			//since it can return 0 or 360 depending on orientation
			if ((returnAngle.Equals(360) || returnAngle.Equals(0)) && ori == Orientation.CLOCKWISE)
			{
				returnAngle = returnAngle.GetOppositeAngle();
			}

			if (side != Side.UNKNOWN && side != targetSide)
			{
				returnAngle = returnAngle.GetOppositeAngle();
				side = targetSide;
			}

			//default orientation is assumed to be counterclockwise
			//flip the angle if it is not
			if (ori == Orientation.CLOCKWISE)
			{
				returnAngle = returnAngle.GetOppositeAngle();
			}

			return new Angle(returnAngle, side);
		}

		public static bool IsPerpendicular(Line a, Line b)
		{
			Degrees angle = GetAngle(a, b).GetDegrees();
			return (angle.Equals(90) || angle.Equals(270));
		}

		public static bool IsParallel(Line a, Line b)
		{
			Degrees angle = GetAngle(a, b).GetDegrees();
			return (angle.Equals(0) || angle.Equals(180) || angle.Equals(360));
		}
	}
}