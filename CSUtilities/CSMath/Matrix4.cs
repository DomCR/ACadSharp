using System;
using System.Collections.Generic;

namespace CSMath
{
	/// <summary>
	/// 4x4 Matrix
	/// </summary>
	/// <remarks>
	/// Matrix organization: <br/>
	/// |m00|m10|m20|m30| <br/>
	/// |m01|m11|m21|m31| <br/>
	/// |m02|m12|m22|m32| <br/>
	/// |m03|m13|m23|m33| <br/>
	/// </remarks>
	public partial struct Matrix4
	{
		/// <summary>
		/// 4-dimensional zero matrix.
		/// </summary>
		public static readonly Matrix4 Zero = new Matrix4(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

		/// <summary>
		/// 4-dimensional identity matrix.
		/// </summary>
		public static readonly Matrix4 Identity = new Matrix4(
			1.0, 0.0, 0.0, 0.0,
			0.0, 1.0, 0.0, 0.0,
			0.0, 0.0, 1.0, 0.0,
			0.0, 0.0, 0.0, 1.0);

		#region Public Fields
		/// <summary>
		/// Value at column 0, row 0 of the matrix.
		/// </summary>
		public double m00;
		/// <summary>
		/// Value at column 0, row 1 of the matrix.
		/// </summary>
		public double m01;
		/// <summary>
		/// Value at column 0, row 2 of the matrix.
		/// </summary>
		public double m02;
		/// <summary>
		/// Value at column 0, row 3 of the matrix.
		/// </summary>
		public double m03;

		/// <summary>
		/// Value at column 1, row 0 of the matrix.
		/// </summary>
		public double m10;
		/// <summary>
		/// Value at column 1, row 1 of the matrix.
		/// </summary>
		public double m11;
		/// <summary>
		/// Value at column 1, row 2 of the matrix.
		/// </summary>
		public double m12;
		/// <summary>
		/// Value at column 1, row 3 of the matrix.
		/// </summary>
		public double m13;

		/// <summary>
		/// Value at column 2, row 0 of the matrix.
		/// </summary>
		public double m20;
		/// <summary>
		/// Value at column 2, row 1 of the matrix.
		/// </summary>
		public double m21;
		/// <summary>
		/// Value at column 2, row 2 of the matrix.
		/// </summary>
		public double m22;
		/// <summary>
		/// Value at column 2, row 3 of the matrix.
		/// </summary>
		public double m23;

		/// <summary>
		/// Value at column 3, row 0 of the matrix.
		/// </summary>
		public double m30;
		/// <summary>
		/// Value at column 3, row 1 of the matrix.
		/// </summary>
		public double m31;
		/// <summary>
		/// Value at column 3, row 2 of the matrix.
		/// </summary>
		public double m32;
		/// <summary>
		/// Value at column 3, row 3 of the matrix.
		/// </summary>
		public double m33;
		#endregion Public Fields

		public Matrix4(
			double m00, double m10, double m20, double m30,
			double m01, double m11, double m21, double m31,
			double m02, double m12, double m22, double m32,
			double m03, double m13, double m23, double m33)
		{
			//Col 0
			this.m00 = m00;
			this.m01 = m01;
			this.m02 = m02;
			this.m03 = m03;

			//Col 1
			this.m10 = m10;
			this.m11 = m11;
			this.m12 = m12;
			this.m13 = m13;

			//Col 2
			this.m20 = m20;
			this.m21 = m21;
			this.m22 = m22;
			this.m23 = m23;

			//Col 3
			this.m30 = m30;
			this.m31 = m31;
			this.m32 = m32;
			this.m33 = m33;
		}

		public Matrix4(double[] elements) : this(
			elements[0], elements[1], elements[2], elements[3],
			elements[4], elements[5], elements[6], elements[7],
			elements[8], elements[9], elements[10], elements[11],
			elements[12], elements[13], elements[14], elements[15])
		{ }

		/// <summary>
		/// Creates a rotation matrix from the given Quaternion rotation value.
		/// </summary>
		/// <param name="quaternion">The source Quaternion.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4 CreateFromQuaternion(Quaternion quaternion)
		{
			Matrix4 result;

			double xx = quaternion.X * quaternion.X;
			double yy = quaternion.Y * quaternion.Y;
			double zz = quaternion.Z * quaternion.Z;

			double xy = quaternion.X * quaternion.Y;
			double wz = quaternion.Z * quaternion.W;
			double xz = quaternion.Z * quaternion.X;
			double wy = quaternion.Y * quaternion.W;
			double yz = quaternion.Y * quaternion.Z;
			double wx = quaternion.X * quaternion.W;

			result.m00 = 1.0f - 2.0f * (yy + zz);
			result.m01 = 2.0f * (xy + wz);
			result.m02 = 2.0f * (xz - wy);
			result.m03 = 0.0f;
			result.m10 = 2.0f * (xy - wz);
			result.m11 = 1.0f - 2.0f * (zz + xx);
			result.m12 = 2.0f * (yz + wx);
			result.m13 = 0.0f;
			result.m20 = 2.0f * (xz + wy);
			result.m21 = 2.0f * (yz - wx);
			result.m22 = 1.0f - 2.0f * (yy + xx);
			result.m23 = 0.0f;
			result.m30 = 0.0f;
			result.m31 = 0.0f;
			result.m32 = 0.0f;
			result.m33 = 1.0f;

			return result;
		}

		/// <summary>
		/// Gets the matrix rows
		/// </summary>
		/// <returns></returns>
		public List<XYZM> GetRows()
		{
			return new List<XYZM>
			{
				new XYZM(m00, m10, m20, m30),
				new XYZM(m01, m11, m21, m31),
				new XYZM(m02, m12, m22, m32),
				new XYZM(m03, m13, m23, m33)
			};
		}

		/// <summary>
		/// Gets the matrix columns
		/// </summary>
		/// <returns></returns>
		public List<XYZM> GetCols()
		{
			return new List<XYZM>
			{
				new XYZM(m00, m01, m02, m03),
				new XYZM(m10, m11, m12, m13),
				new XYZM(m20, m21, m22, m23),
				new XYZM(m30, m31, m32, m33)
			};
		}

		/// <summary>
		/// Calculates the determinant of the matrix.
		/// </summary>
		/// <returns>The determinant of the matrix.</returns>
		public double GetDeterminant()
		{
			// | a b c d |     | f g h |     | e g h |     | e f h |     | e f g |
			// | e f g h | = a | j k l | - b | i k l | + c | i j l | - d | i j k |
			// | i j k l |     | n o p |     | m o p |     | m n p |     | m n o |
			// | m n o p |
			//
			//   | f g h |
			// a | j k l | = a ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
			//   | n o p |
			//
			//   | e g h |     
			// b | i k l | = b ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
			//   | m o p |     
			//
			//   | e f h |
			// c | i j l | = c ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
			//   | m n p |
			//
			//   | e f g |
			// d | i j k | = d ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
			//   | m n o |

			double a = m00, b = m10, c = m20, d = m30;
			double e = m01, f = m11, g = m21, h = m31;
			double i = m02, j = m12, k = m22, l = m32;
			double m = m03, n = m13, o = m23, p = m33;

			double kp_lo = k * p - l * o;
			double jp_ln = j * p - l * n;
			double jo_kn = j * o - k * n;
			double ip_lm = i * p - l * m;
			double io_km = i * o - k * m;
			double in_jm = i * n - j * m;

			return a * (f * kp_lo - g * jp_ln + h * jo_kn) -
				   b * (e * kp_lo - g * ip_lm + h * io_km) +
				   c * (e * jp_ln - f * ip_lm + h * in_jm) -
				   d * (e * jo_kn - f * io_km + g * in_jm);
		}

		/// <summary>
		/// Attempts to calculate the inverse of the given matrix. If successful, result will contain the inverted matrix.
		/// </summary>
		/// <param name="matrix">The source matrix to invert.</param>
		/// <param name="result">If successful, contains the inverted matrix.</param>
		/// <returns>True if the source matrix could be inverted; False otherwise.</returns>
		public static bool Inverse(Matrix4 matrix, out Matrix4 result)
		{
			//                                       -1
			// If you have matrix M, inverse Matrix M   can compute
			//
			//     -1       1      
			//    M   = --------- A
			//            det(M)
			//
			// A is adjugate (adjoint) of M, where,
			//
			//      T
			// A = C
			//
			// C is Cofactor matrix of M, where,
			//           i + j
			// C   = (-1)      * det(M  )
			//  ij                    ij
			//
			//     [ a b c d ]
			// M = [ e f g h ]
			//     [ i j k l ]
			//     [ m n o p ]
			//
			// First Row
			//           2 | f g h |
			// C   = (-1)  | j k l | = + ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
			//  11         | n o p |
			//
			//           3 | e g h |
			// C   = (-1)  | i k l | = - ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
			//  12         | m o p |
			//
			//           4 | e f h |
			// C   = (-1)  | i j l | = + ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
			//  13         | m n p |
			//
			//           5 | e f g |
			// C   = (-1)  | i j k | = - ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
			//  14         | m n o |
			//
			// Second Row
			//           3 | b c d |
			// C   = (-1)  | j k l | = - ( b ( kp - lo ) - c ( jp - ln ) + d ( jo - kn ) )
			//  21         | n o p |
			//
			//           4 | a c d |
			// C   = (-1)  | i k l | = + ( a ( kp - lo ) - c ( ip - lm ) + d ( io - km ) )
			//  22         | m o p |
			//
			//           5 | a b d |
			// C   = (-1)  | i j l | = - ( a ( jp - ln ) - b ( ip - lm ) + d ( in - jm ) )
			//  23         | m n p |
			//
			//           6 | a b c |
			// C   = (-1)  | i j k | = + ( a ( jo - kn ) - b ( io - km ) + c ( in - jm ) )
			//  24         | m n o |
			//
			// Third Row
			//           4 | b c d |
			// C   = (-1)  | f g h | = + ( b ( gp - ho ) - c ( fp - hn ) + d ( fo - gn ) )
			//  31         | n o p |
			//
			//           5 | a c d |
			// C   = (-1)  | e g h | = - ( a ( gp - ho ) - c ( ep - hm ) + d ( eo - gm ) )
			//  32         | m o p |
			//
			//           6 | a b d |
			// C   = (-1)  | e f h | = + ( a ( fp - hn ) - b ( ep - hm ) + d ( en - fm ) )
			//  33         | m n p |
			//
			//           7 | a b c |
			// C   = (-1)  | e f g | = - ( a ( fo - gn ) - b ( eo - gm ) + c ( en - fm ) )
			//  34         | m n o |
			//
			// Fourth Row
			//           5 | b c d |
			// C   = (-1)  | f g h | = - ( b ( gl - hk ) - c ( fl - hj ) + d ( fk - gj ) )
			//  41         | j k l |
			//
			//           6 | a c d |
			// C   = (-1)  | e g h | = + ( a ( gl - hk ) - c ( el - hi ) + d ( ek - gi ) )
			//  42         | i k l |
			//
			//           7 | a b d |
			// C   = (-1)  | e f h | = - ( a ( fl - hj ) - b ( el - hi ) + d ( ej - fi ) )
			//  43         | i j l |
			//
			//           8 | a b c |
			// C   = (-1)  | e f g | = + ( a ( fk - gj ) - b ( ek - gi ) + c ( ej - fi ) )
			//  44         | i j k |
			//
			// Cost of operation
			// 53 adds, 104 muls, and 1 div.
			double a = matrix.m00, b = matrix.m01, c = matrix.m02, d = matrix.m03;
			double e = matrix.m10, f = matrix.m11, g = matrix.m12, h = matrix.m13;
			double i = matrix.m20, j = matrix.m21, k = matrix.m22, l = matrix.m23;
			double m = matrix.m30, n = matrix.m31, o = matrix.m32, p = matrix.m33;

			double kp_lo = k * p - l * o;
			double jp_ln = j * p - l * n;
			double jo_kn = j * o - k * n;
			double ip_lm = i * p - l * m;
			double io_km = i * o - k * m;
			double in_jm = i * n - j * m;

			double a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
			double a12 = -(e * kp_lo - g * ip_lm + h * io_km);
			double a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
			double a14 = -(e * jo_kn - f * io_km + g * in_jm);

			double det = a * a11 + b * a12 + c * a13 + d * a14;

			if (Math.Abs(det) < double.Epsilon)
			{
				result = new Matrix4(double.NaN, double.NaN, double.NaN, double.NaN,
									   double.NaN, double.NaN, double.NaN, double.NaN,
									   double.NaN, double.NaN, double.NaN, double.NaN,
									   double.NaN, double.NaN, double.NaN, double.NaN);
				return false;
			}

			double invDet = 1.0f / det;

			result.m00 = a11 * invDet;
			result.m10 = a12 * invDet;
			result.m20 = a13 * invDet;
			result.m30 = a14 * invDet;

			result.m01 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
			result.m11 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
			result.m21 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
			result.m31 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

			double gp_ho = g * p - h * o;
			double fp_hn = f * p - h * n;
			double fo_gn = f * o - g * n;
			double ep_hm = e * p - h * m;
			double eo_gm = e * o - g * m;
			double en_fm = e * n - f * m;

			result.m02 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
			result.m12 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
			result.m22 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
			result.m32 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

			double gl_hk = g * l - h * k;
			double fl_hj = f * l - h * j;
			double fk_gj = f * k - g * j;
			double el_hi = e * l - h * i;
			double ek_gi = e * k - g * i;
			double ej_fi = e * j - f * i;

			result.m03 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
			result.m13 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
			result.m23 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
			result.m33 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

			return true;
		}

		/// <summary>
		/// Transposes the rows and columns of this matrix.
		/// </summary>
		/// <returns>The transposed matrix.</returns>
		public Matrix4 Transpose()
		{
			Matrix4 result;

			result.m00 = this.m00;
			result.m01 = this.m10;
			result.m02 = this.m20;
			result.m03 = this.m30;
			result.m10 = this.m01;
			result.m11 = this.m11;
			result.m12 = this.m21;
			result.m13 = this.m31;
			result.m20 = this.m02;
			result.m21 = this.m12;
			result.m22 = this.m22;
			result.m23 = this.m32;
			result.m30 = this.m03;
			result.m31 = this.m13;
			result.m32 = this.m23;
			result.m33 = this.m33;

			return result;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return m00.GetHashCode() + m01.GetHashCode() + m02.GetHashCode() + m03.GetHashCode() +
				   m10.GetHashCode() + m11.GetHashCode() + m12.GetHashCode() + m13.GetHashCode() +
				   m20.GetHashCode() + m21.GetHashCode() + m22.GetHashCode() + m23.GetHashCode() +
				   m30.GetHashCode() + m31.GetHashCode() + m32.GetHashCode() + m33.GetHashCode();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is Matrix4 other))
				return false;

			return m00 == other.m00 && m11 == other.m11 && m22 == other.m22 && m33 == other.m33 &&
				m01 == other.m01 && m02 == other.m02 && m03 == other.m03 &&
				m10 == other.m10 && m12 == other.m12 && m13 == other.m13 &&
				m20 == other.m20 && m21 == other.m21 && m23 == other.m23 &&
				m30 == other.m30 && m31 == other.m31 && m32 == other.m32;
		}
	}
}
