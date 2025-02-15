using CSMath;
using System;

namespace ACadSharp
{
	[Obsolete("Use CSMath.MathHelper instead.")]
	public static class MathUtils
	{
		/// <summary>
		/// Factor for converting radians to degrees.
		/// </summary>
		public const double RadToDegFactor = (180 / Math.PI);

		/// <summary>
		/// Factor for converting degrees to radians.
		/// </summary>
		public const double DegToRadFactor = (Math.PI / 180);

		public const double Epsilon = 1e-12;

		public const double TwoPI = Math.PI * 2;

		/// <summary>
		/// Checks if a number is close to zero.
		/// </summary>
		/// <param name="number">Double precision number.</param>
		/// <returns>True if its close to one or false in any other case.</returns>
		public static bool IsZero(double number)
		{
			return IsZero(number, Epsilon);
		}

		/// <summary>
		/// Checks if a number is close to zero.
		/// </summary>
		/// <param name="number">Double precision number.</param>
		/// <param name="threshold">Tolerance.</param>
		/// <returns>True if its close to one or false in any other case.</returns>
		public static bool IsZero(double number, double threshold)
		{
			return number >= -threshold && number <= threshold;
		}

		public static double FixZero(double number)
		{
			return FixZero(number, Epsilon);
		}

		public static double FixZero(double number, double threshold)
		{
			return IsZero(number, threshold) ? 0 : number;
		}

		public static double RadToDeg(double value)
		{
			return value * RadToDegFactor;
		}

		public static double DegToRad(double value)
		{
			return value * DegToRadFactor;
		}

		public static XY GetCenter(XY start, XY end, double bulge)
		{
			return GetCenter(start, end, bulge, out _);
		}

		public static XY GetCenter(XY start, XY end, double bulge, out double radius)
		{
			double theta = 4 * Math.Atan(Math.Abs(bulge));
			double c = start.DistanceFrom(end) / 2.0;
			radius = c / Math.Sin(theta / 2.0);

			double gamma = (Math.PI - theta) / 2;
			double phi = (end - start).GetAngle() + Math.Sign(bulge) * gamma;
			return new XY(start.X + radius * CSMath.MathHelper.Cos(phi), start.Y + radius * CSMath.MathHelper.Sin(phi));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="bulge"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public static T GetCenter<T>(T start, T end, double bulge)
			where T : IVector, new()
		{
			//Needs a plane of reference in case is in 3D
			throw new NotImplementedException();
		}
	}
}
