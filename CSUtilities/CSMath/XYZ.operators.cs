using System;

namespace CSMath
{
	public partial struct XYZ : IVector, IEquatable<XYZ>
	{
		/// <summary>
		/// Adds two vectors together.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The summed vector.</returns>
		public static XYZ operator +(XYZ left, XYZ right)
		{
			return left.Add(right);
		}

		/// <summary>
		/// Subtracts the second vector from the first.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The difference vector.</returns>
		public static XYZ operator -(XYZ left, XYZ right)
		{
			return left.Subtract(right);
		}

		/// <summary>
		/// Multiplies two vectors together.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The product vector.</returns>
		public static XYZ operator *(XYZ left, XYZ right)
		{
			return left.Multiply(right);
		}

		/// <summary>
		/// Multiplies a vector by the given scalar.
		/// </summary>
		/// <param name="left">The source vector.</param>
		/// <param name="scalar">The scalar value.</param>
		/// <returns>The scaled vector.</returns>
		public static XYZ operator *(XYZ left, double scalar)
		{
			return left * new XYZ(scalar);
		}

		/// <summary>
		/// Multiplies a vector by the given scalar.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="vector">The source vector.</param>
		/// <returns>The scaled vector.</returns>
		public static XYZ operator *(double scalar, XYZ vector)
		{
			return new XYZ(scalar) * vector;
		}

		/// <summary>
		/// Divides the first vector by the second.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The vector resulting from the division.</returns>
		public static XYZ operator /(XYZ left, XYZ right)
		{
			return left.Divide(right);
		}

		/// <summary>
		/// Divides the vector by the given scalar.
		/// </summary>
		/// <param name="xyz">The source vector.</param>
		/// <param name="value">The scalar value.</param>
		/// <returns>The result of the division.</returns>
		public static XYZ operator /(XYZ xyz, float value)
		{
			float invDiv = 1.0f / value;

			return new XYZ(xyz.X * invDiv,
							xyz.Y * invDiv,
							xyz.Z * invDiv);
		}

		/// <summary>
		/// Divides the vector by the given scalar.
		/// </summary>
		/// <param name="xyz">The source vector.</param>
		/// <param name="value">The scalar value.</param>
		/// <returns>The result of the division.</returns>
		public static XYZ operator /(XYZ xyz, double value)
		{
			double invDiv = 1.0f / value;

			return new XYZ(xyz.X * invDiv,
							xyz.Y * invDiv,
							xyz.Z * invDiv);
		}

		/// <summary>
		/// Negates a given vector.
		/// </summary>
		/// <param name="value">The source vector.</param>
		/// <returns>The negated vector.</returns>
		public static XYZ operator -(XYZ value)
		{
			return Zero.Subtract(value);
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given vectors are equal.
		/// </summary>
		/// <param name="left">The first vector to compare.</param>
		/// <param name="right">The second vector to compare.</param>
		/// <returns>True if the vectors are equal; False otherwise.</returns>
		public static bool operator ==(XYZ left, XYZ right)
		{
			return (left.X == right.X &&
					left.Y == right.Y &&
					left.Z == right.Z);
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given vectors are not equal.
		/// </summary>
		/// <param name="left">The first vector to compare.</param>
		/// <param name="right">The second vector to compare.</param>
		/// <returns>True if the vectors are not equal; False if they are equal.</returns>
		public static bool operator !=(XYZ left, XYZ right)
		{
			return (left.X != right.X ||
					left.Y != right.Y ||
					left.Z != right.Z);
		}

		public static explicit operator XYZ(XY xy)
		{
			return new XYZ(xy.X, xy.Y, 0);
		}
	}
}
