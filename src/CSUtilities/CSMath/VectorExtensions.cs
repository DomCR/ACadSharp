using System;
using System.Collections.Generic;

namespace CSMath
{
	public static class VectorExtensions
	{
		/// <summary>
		/// Angle between two <see cref="IVector"/>.
		/// </summary>
		/// <param name="v">The first <see cref="IVector" />.</param>
		/// <param name="u">The second <see cref="IVector" />.</param>
		public static double AngleFrom<T>(this T v, T u)
			where T : IVector, new()
		{
			if (v.IsZero() || u.IsZero())
				throw new InvalidOperationException("Cannot calculate the angle between two vectors, if one is zero.");

			return Math.Acos(v.Dot(u) / (v.GetLength() * u.GetLength()));
		}

		/// <summary>
		/// Returns true if the magnitude of the <see cref="IVector"/> is zero.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <returns></returns>
		public static bool IsZero<T>(this T v)
			where T : IVector
		{
			return v.GetLength() == 0;
		}

		/// <summary>
		/// Distance between two <see cref="IVector"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <param name="u"></param>
		/// <returns></returns>
		public static double DistanceFrom<T>(this T v, T u)
			where T : IVector, new()
		{
			return Subtract(v, u).GetLength();
		}

		/// <summary>
		/// Copy the component values from a source using the smallest dimension of both parameters
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <param name="source"></param>
		/// <returns>A copy of the <see cref="IVector"/> with the copied components</returns>
		public static T CopyValues<T>(this T v, IVector source)
			where T : IVector, new()
		{
			for (int i = 0; i < Math.Min(v.Dimension, source.Dimension); i++)
			{
				v[i] = source[i];
			}

			return v;
		}

		/// <summary>
		/// Converts an <see cref="IVector" /> into an equivalent <see cref="IVector" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <returns></returns>
		public static T Convert<T>(this IVector v)
			where T : IVector, new()
		{
			T result = new T();

			for (int i = 0; i < Math.Min(result.Dimension, v.Dimension); i++)
			{
				result[i] = v[i];
			}

			return result;
		}

		/// <summary>
		/// Returns the length of the vector.
		/// </summary>
		/// <returns>The vector's length.</returns>
		public static double GetLength<T>(this T vector)
			where T : IVector
		{
			double length = 0;

			for (int i = 0; i < vector.Dimension; i++)
			{
				length += Math.Pow(vector[i], 2);
			}

			return Math.Sqrt(length);
		}

		/// <summary>
		/// Returns a vector with the same direction as the given vector, but with a length of 1.
		/// </summary>
		/// <param name="vector">The vector to normalize.</param>
		/// <returns>The normalized vector.</returns>
		public static T Normalize<T>(this T vector)
			where T : IVector, new()
		{
			double length = vector.GetLength();
			T result = new T();

			for (int i = 0; i < result.Dimension; i++)
			{
				result[i] = vector[i] / length;
			}

			return result;
		}

		/// <summary>
		/// Returns the dot product of two vectors.
		/// </summary>
		/// <param name="left">The first vector.</param>
		/// <param name="right">The second vector.</param>
		/// <returns>The dot product.</returns>
		public static double Dot<T>(this T left, T right)
			where T : IVector
		{
			double result = 0;
			for (int i = 0; i < left.Dimension; ++i)
			{
				result += left[i] * right[i];
			}

			return result;
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="IVector" /> is normalized, or not.
		/// </summary>
		public static bool IsNormalized<T>(this T v)
			where T : IVector
		{
			return v.GetLength() == 1;
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given vectors are parallel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="left">The first vector.</param>
		/// <param name="right">The second vector.</param>
		/// <returns></returns>
		public static bool IsParallel<T>(this T left, T right)
			where T : IVector
		{
			if (left.IsZero() || right.IsZero())
				return false;

			double firstResult = 0;
			for (int i = 0; i < left.Dimension; ++i)
				if (i == 0)
				{
					firstResult = right[i] / left[i];
				}
				else
				{
					double curr = right[i] / left[i];
					if (!curr.Equals(firstResult))
					{
						return false;
					}
				}

			return true;
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given vectors are perpendicular.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="left">The first vector.</param>
		/// <param name="right">The second vector.</param>
		/// <returns></returns>
		public static bool IsPerpendicular<T>(this T left, T right)
			where T : IVector
		{
			return Dot<T>(left, right) == 0;
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given vectors are equal.
		/// </summary>
		/// <param name="left">The first vector to compare.</param>
		/// <param name="right">The second vector to compare.</param>
		/// <returns>True if the vectors are equal; False otherwise.</returns>
		public static bool IsEqual<T>(this T left, T right)
			where T : IVector
		{
			for (int i = 0; i < left.Dimension; i++)
			{
				if (left[i] != right[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given vectors are equal.
		/// </summary>
		/// <param name="left">The first vector to compare.</param>
		/// <param name="right">The second vector to compare.</param>
		/// <param name="ndecimals">Number of decimals digits to be set as precision.</param>
		/// <returns>True if the vectors are equal; False otherwise.</returns>
		public static bool IsEqual<T>(this T left, T right, int ndecimals)
			where T : IVector
		{
			for (int i = 0; i < left.Dimension; i++)
			{
				if (Math.Round(left[i], ndecimals) != Math.Round(right[i], ndecimals))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Adds two vectors together.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The summed vector.</returns>
		public static T Add<T>(this T left, T right)
			where T : IVector, new()
		{
			return applyFunctionByComponentIndex(left, right, (o, x) => o + x);
		}

		/// <summary>
		/// Subtracts the second vector from the first.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The difference vector.</returns>
		public static T Subtract<T>(this T left, T right)
			where T : IVector, new()
		{
			return applyFunctionByComponentIndex(left, right, (o, x) => o - x);
		}

		/// <summary>
		/// Multiplies two vectors together.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The product vector.</returns>
		public static T Multiply<T>(this T left, T right)
			where T : IVector, new()
		{
			return applyFunctionByComponentIndex(left, right, (o, x) => o * x);
		}

		/// <summary>
		/// Multiplies a vector with an scalar.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The product vector.</returns>
		public static T Multiply<T>(this T left, double scalar)
			where T : IVector, new()
		{
			return applyFunctionByScalar(left, scalar, (o, x) => o * x);
		}

		/// <summary>
		/// Divides the first vector by the second.
		/// </summary>
		/// <param name="left">The first source vector.</param>
		/// <param name="right">The second source vector.</param>
		/// <returns>The vector resulting from the division.</returns>
		public static T Divide<T>(this T left, T right)
			where T : IVector, new()
		{
			return applyFunctionByComponentIndex(left, right, (o, x) => o / x);
		}

		/// <summary>
		/// Divides a vector with an scalar.
		/// </summary>
		public static T Divide<T>(this T left, double scalar)
			where T : IVector, new()
		{
			return applyFunctionByScalar(left, scalar, (o, x) => o / x);
		}

		/// <summary>
		/// Round the vector components
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static T Round<T>(this T vector)
			where T : IVector, new()
		{
			T result = new T();

			for (int i = 0; i < result.Dimension; i++)
			{
				result[i] = Math.Round(vector[i]);
			}

			return result;
		}

		/// <summary>
		/// Round the vector components
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="vector"></param>
		/// <param name="digits">The number of fractional digits in the return value</param>
		/// <returns></returns>
		public static T Round<T>(this T vector, int digits)
			where T : IVector, new()
		{
			T result = new T();

			for (int i = 0; i < result.Dimension; i++)
			{
				result[i] = Math.Round(vector[i], digits);
			}

			return result;
		}

		/// <summary>
		/// Get an enumerable with the components of the <see cref="IVector"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <returns></returns>
		public static IEnumerable<double> ToEnumerable<T>(this T v)
			where T : IVector
		{
			for (int i = 0; i < v.Dimension; i++)
			{
				yield return v[i];
			}
		}

		// Applies a function in all the components of a vector by order
		private static T applyFunctionByComponentIndex<T>(this T left, T right, Func<double, double, double> op)
			where T : IVector, new()
		{
			T result = new T();

			for (int i = 0; i < left.Dimension; i++)
			{
				result[i] = op(left[i], right[i]);
			}

			return result;
		}

		private static T applyFunctionByScalar<T>(this T v, double scalar, Func<double, double, double> op)
			where T : IVector, new()
		{
			T result = new T();

			for (int i = 0; i < v.Dimension; i++)
			{
				result[i] = op(v[i], scalar);
			}

			return result;
		}
	}
}
