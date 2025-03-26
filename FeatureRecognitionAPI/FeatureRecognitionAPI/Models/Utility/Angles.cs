using static FeatureRecognitionAPI.Models.Utility.MDCMath;

// This file is used for calculating the angle between lines and on what side they lay
namespace FeatureRecognitionAPI.Models.Utility
{
	/// <summary>
	/// Angles contains classes and functions for calculating angles
	/// Not to be used directly, use Angle class, Side and Orientation enums, etc
	/// </summary>
	public static class Angles
	{
		private const double AngleTolDeg = 0.001;
		private const double AngleTolRad = AngleTolDeg * Math.PI / 180;

		/// <summary>
		/// The side an angle lies in relation to the rest of the shape
		/// </summary>
		public enum Side
		{
			Interior,
			Exterior,
			Unknown
		}

		/// <summary>
		/// The general orientation of a polygon
		/// This is the direction its lines are drawn
		/// Ideally all lines of a polygon should have a consistent direction to them
		/// </summary>
		public enum Orientation
		{
			Counter,
			Clockwise,
			Unknown
		}

		public class Degrees(double value)
		{
			public readonly double Value = value;

			public Degrees GetOppositeAngle()
			{
				return new Degrees(360 - Value);
			}

			public Radians ToRadians()
			{
				return new Radians(Value * Math.PI / 180);
			}
			
			#region Overrides

			public override bool Equals(object? obj)
			{
				if (obj == null) return false;
				
				if (obj is Degrees objD)
				{
					return DoubleEquals(objD, Value);
				}
				return false;
			}

			public static bool operator ==(Degrees a, Degrees b)
			{
				return a.Equals(b);
			}
			
			public static bool operator !=(Degrees a, Degrees b)
			{
				return !a.Equals(b);
			}
			
			public static implicit operator double(Degrees d) => d.Value;
			
			public override int GetHashCode()
			{
				return HashCode.Combine(Value);
			}

			#endregion
		}

		public class Radians(double value)
		{
			public readonly double Value = value;

			public Radians GetOppositeAngle()
			{
				return new Radians((2 * Math.PI) - Value);
			}

			public Degrees ToDegrees()
			{
				return new Degrees(Value * 180 / Math.PI);
			}
			
			#region Overrides

			public override bool Equals(object? obj)
			{
				if (obj == null) return false;
				
				if (obj is Degrees objR)
				{
					return DoubleEquals(objR, Value, AngleTolRad);
				}
				return false;
			}

			public static bool operator ==(Radians a, Radians b)
			{
				return a.Equals(b);
			}
			
			public static bool operator !=(Radians a, Radians b)
			{
				return !a.Equals(b);
			}
			
			public static implicit operator double(Radians r) => r.Value;
			
			public override int GetHashCode()
			{
				return HashCode.Combine(Value);
			}
			
			#endregion
		}

		/// <summary>
		/// The angle and side that angle is on between two lines
		/// </summary>
		public class Angle
		{
			private readonly Degrees _angle;
			private readonly Side _side;

			public Angle(Degrees angle, Side side)
			{
				_angle = angle;
				_side = side;
			}

			public Angle(Radians angle, Side side)
			{
				_angle = angle.ToDegrees();
				_side = side;
			}

			public Degrees GetDegrees()
			{
				return _angle;
			}

			public Radians GetRadians()
			{
				return _angle.ToRadians();
			}

			public Side GetSide()
			{
				return _side;
			}

			#region Overrides
			public override bool Equals(object? obj)
			{
				if (obj == null) { return false; }

                if (obj is Angle objA 
					&& _side != Side.Unknown 
					&& objA._side != Side.Unknown)
                {
                    if (objA._side == this._side && objA._angle == this._angle)
                    {
                        return true;
                    }
                    //sides are opposite
                    if (objA._side != this._side && objA._angle.GetOppositeAngle() == _angle)
                    {
                        return true;
                    }
                }
                return false;
			}
			
			public static bool operator ==(Angle a, Angle b)
			{
				return a.Equals(b);
			}
			
			public static bool operator !=(Angle a, Angle b)
			{
				return !a.Equals(b);
			}

			public override int GetHashCode()
			{
				throw new NotImplementedException();
			}
			#endregion
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

        public static double RadToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }
        
        public static double DegToRadians(double degrees)
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

        /// <summary>
        /// Calculates the dot product of two points treated as vectors
        /// </summary>
        public static double DotProduct(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        // may not handle all cases yet, to be tested
        public static Angle GetAngle(Line a, Line b, Side targetSide = Side.Interior, Orientation ori = Orientation.Counter)
		{
			if (DoubleEquals(a.Length, 0) || DoubleEquals(b.Length, 0))
			{
				return new Angle(new Degrees(0), Side.Unknown);
			}

			double cross = CrossProduct(a, b);
			double dot = DotProduct(a, b);
			Side side = Side.Unknown;

			double cosTheta = dot / (a.Length * b.Length);
			cosTheta = Math.Max(-1, Math.Min(1, cosTheta));

			double angle = Math.Acos(cosTheta) * (180 / Math.PI);

			if (cross >= 0)
			{
				side = Side.Interior;
			}
			else if (cross < 0)
			{
				side = Side.Exterior;
				angle = 360 - angle;
			}

			//initially calculates interior angle
			//return opposite if wanting exterior
			Degrees returnAngle = new(Math.Abs(180 - angle));

			//prefer 0 degrees interior over 360 for opposite facing parallel lines
			//since it can return 0 or 360 depending on orientation
			if ((DoubleEquals(returnAngle, 360) 
			     || DoubleEquals(returnAngle, 0)) 
			    && ori == Orientation.Clockwise)
			{
				returnAngle = returnAngle.GetOppositeAngle();
			}

			if (side != Side.Unknown && side != targetSide)
			{
				returnAngle = returnAngle.GetOppositeAngle();
				side = targetSide;
			}

			//default orientation is assumed to be counterclockwise
			//flip the angle if it is not
			if (ori == Orientation.Clockwise)
			{
				returnAngle = returnAngle.GetOppositeAngle();
			}

			return new Angle(returnAngle, side);
		}

		public static bool IsPerpendicular(Line a, Line b)
		{
			Degrees angle = GetAngle(a, b).GetDegrees();
			return DoubleEquals((angle + 90) % 180, 0);
		}

		public static bool IsParallel(Line a, Line b)
		{
			Degrees angle = GetAngle(a, b).GetDegrees();
			return DoubleEquals(angle % 180, 0);
		}
	}
}