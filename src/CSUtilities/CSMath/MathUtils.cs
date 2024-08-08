using System;

namespace CSMath
{
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

		/// <summary>
		/// Default tolerance
		/// </summary>
		public const double Epsilon = 1e-12;

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

		/// <summary>
		/// Convert a value from radian to degree
		/// </summary>
		/// <param name="degree">Value in radians</param>
		/// <returns>The radian value</returns>
		public static double RadToDeg(double value)
		{
			return value * RadToDegFactor;
		}

		/// <summary>
		/// Convert a value from degree to radian
		/// </summary>
		/// <param name="degree">Value in degrees</param>
		/// <returns>The radian value</returns>
		public static double DegToRad(double value)
		{
			return value * DegToRadFactor;
		}

		/// <summary>
		/// Returns the sine of specific angle in radians adjusting the value to 0 using <see cref="Epsilon"/> as tolerance.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double Sin(double value)
		{
			double result = Math.Sin(value);
			return IsZero(result) ? 0 : result;
		}

		/// <summary>
		/// Returns the cosine of specific angle in radians adjusting the value to 0 using <see cref="Epsilon"/> as tolerance.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double Cos(double value)
		{
			double result = Math.Cos(value);
			return IsZero(result) ? 0 : result;
		}
	}
}
