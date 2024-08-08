using System;
using System.Collections.Generic;

namespace CSMath
{
	public partial struct Matrix4
	{
		#region Move to transform
		/// <summary>
		/// Creates a translation matrix.
		/// </summary>
		/// <param name="position">The amount to translate in each axis.</param>
		/// <returns>The translation matrix.</returns>
		public static Matrix4 CreateTranslation(XYZ position)
		{
			return Matrix4.CreateTranslation(position.X, position.Y, position.Z);
		}

		/// <summary>
		/// Creates a translation matrix.
		/// </summary>
		/// <param name="xPosition">The amount to translate on the X-axis.</param>
		/// <param name="yPosition">The amount to translate on the Y-axis.</param>
		/// <param name="zPosition">The amount to translate on the Z-axis.</param>
		/// <returns>The translation matrix.</returns>
		public static Matrix4 CreateTranslation(double xPosition, double yPosition, double zPosition)
		{
			Matrix4 result;

			result.m00 = 1.0f;
			result.m01 = 0.0f;
			result.m02 = 0.0f;
			result.m03 = 0.0f;
			result.m10 = 0.0f;
			result.m11 = 1.0f;
			result.m12 = 0.0f;
			result.m13 = 0.0f;
			result.m20 = 0.0f;
			result.m21 = 0.0f;
			result.m22 = 1.0f;
			result.m23 = 0.0f;

			result.m30 = xPosition;
			result.m31 = yPosition;
			result.m32 = zPosition;
			result.m33 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a scaling matrix.
		/// </summary>
		/// <param name="scales">The vector containing the amount to scale by on each axis.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4 CreateScale(XYZ scales)
		{
			return CreateScale(scales, XYZ.Zero);
		}

		/// <summary>
		/// Creates a scaling matrix with a center point.
		/// </summary>
		/// <param name="scales">The vector containing the amount to scale by on each axis.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4 CreateScale(XYZ scales, XYZ centerPoint)
		{
			Matrix4 result;

			double tx = centerPoint.X * (1 - scales.X);
			double ty = centerPoint.Y * (1 - scales.Y);
			double tz = centerPoint.Z * (1 - scales.Z);

			result.m00 = scales.X;
			result.m01 = 0.0f;
			result.m02 = 0.0f;
			result.m03 = 0.0f;
			result.m10 = 0.0f;
			result.m11 = scales.Y;
			result.m12 = 0.0f;
			result.m13 = 0.0f;
			result.m20 = 0.0f;
			result.m21 = 0.0f;
			result.m22 = scales.Z;
			result.m23 = 0.0f;
			result.m30 = tx;
			result.m31 = ty;
			result.m32 = tz;
			result.m33 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a uniform scaling matrix that scales equally on each axis.
		/// </summary>
		/// <param name="scale">The uniform scaling factor.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4 CreateScale(double scale)
		{
			return CreateScale(scale, XYZ.Zero);
		}

		/// <summary>
		/// Creates a uniform scaling matrix that scales equally on each axis with a center point.
		/// </summary>
		/// <param name="scale">The uniform scaling factor.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4 CreateScale(double scale, XYZ centerPoint)
		{
			return CreateScale(new XYZ(scale), centerPoint);
		}

		/// <summary>
		/// Creates a rotation matrix.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Matrix4 CreateRotationMatrix(double x, double y, double z)
		{
			double cosx = Math.Cos(x);
			double cosy = Math.Cos(y);
			double cosz = Math.Cos(z);

			double sinx = Math.Sin(x);
			double siny = Math.Sin(y);
			double sinz = Math.Sin(z);

			//X rotation 
			Matrix4 rx = new Matrix4(
				1, 0, 0, 0,
				0, cosx, sinx, 0,
				0, -sinx, cosx, 0,
				0, 0, 0, 1);

			//Y rotation 
			Matrix4 ry = new Matrix4(
				cosy, 0, -siny, 0,
				0, 1, 0, 0,
				siny, 0, cosy, 0,
				0, 0, 0, 1);

			//Z rotation 
			Matrix4 rz = new Matrix4(
				cosz, -sinz, 0, 0,
				sinz, cosz, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1);

			return rx * ry * rz;
		}

		/// <summary>
		/// Creates a matrix that rotates around an arbitrary vector.
		/// </summary>
		/// <param name="axis">The axis to rotate around.</param>
		/// <param name="angle">The angle to rotate around the given axis, in radians.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4 CreateFromAxisAngle(XYZ axis, double angle)
		{
			// a: angle
			// x, y, z: unit vector for axis.
			//
			// Rotation matrix M can compute by using below equation.
			//
			//        T               T
			//  M = uu + (cos a)( I-uu ) + (sin a)S
			//
			// Where:
			//
			//  u = ( x, y, z )
			//
			//      [  0 -z  y ]
			//  S = [  z  0 -x ]
			//      [ -y  x  0 ]
			//
			//      [ 1 0 0 ]
			//  I = [ 0 1 0 ]
			//      [ 0 0 1 ]
			//
			//
			//     [  xx+cosa*(1-xx)   yx-cosa*yx-sina*z zx-cosa*xz+sina*y ]
			// M = [ xy-cosa*yx+sina*z    yy+cosa(1-yy)  yz-cosa*yz-sina*x ]
			//     [ zx-cosa*zx-sina*y zy-cosa*zy+sina*x   zz+cosa*(1-zz)  ]
			//
			double x = axis.X, y = axis.Y, z = axis.Z;
			double sa = (double)Math.Sin(angle), ca = (double)Math.Cos(angle);
			double xx = x * x, yy = y * y, zz = z * z;
			double xy = x * y, xz = x * z, yz = y * z;

			Matrix4 result = new Matrix4();

			result.m00 = xx + ca * (1.0f - xx);
			result.m01 = xy - ca * xy + sa * z;
			result.m02 = xz - ca * xz - sa * y;
			result.m03 = 0.0f;
			result.m10 = xy - ca * xy - sa * z;
			result.m11 = yy + ca * (1.0f - yy);
			result.m12 = yz - ca * yz + sa * x;
			result.m13 = 0.0f;
			result.m20 = xz - ca * xz + sa * y;
			result.m21 = yz - ca * yz - sa * x;
			result.m22 = zz + ca * (1.0f - zz);
			result.m23 = 0.0f;
			result.m30 = 0.0f;
			result.m31 = 0.0f;
			result.m32 = 0.0f;
			result.m33 = 1.0f;

			return result;
		}

		/// <summary>
		/// Builds a matrix that scales along the x-axis, y-axis, and z-axis.
		/// </summary>
		public static Matrix4 CreateScalingMatrix(double x, double y, double z)
		{
			return new Matrix4(
				x, 0.0, 0.0, 0.0,
				0.0, y, 0.0, 0.0,
				0.0, 0.0, z, 0.0,
				0.0, 0.0, 0.0, 1.0);
		} 
		#endregion

		/// <summary>
		/// Multiplies two matrices.
		/// </summary>
		/// <returns>A new instance containing the result.</returns>
		public static Matrix4 Multiply(Matrix4 a, Matrix4 b)
		{
			Matrix4 result = new Matrix4();

			var rows = a.GetRows();
			var cols = b.GetCols();

			result.m00 = rows[0].Dot(cols[0]);
			result.m10 = rows[0].Dot(cols[1]);
			result.m20 = rows[0].Dot(cols[2]);
			result.m30 = rows[0].Dot(cols[3]);
			result.m01 = rows[1].Dot(cols[0]);
			result.m11 = rows[1].Dot(cols[1]);
			result.m21 = rows[1].Dot(cols[2]);
			result.m31 = rows[1].Dot(cols[3]);
			result.m02 = rows[2].Dot(cols[0]);
			result.m12 = rows[2].Dot(cols[1]);
			result.m22 = rows[2].Dot(cols[2]);
			result.m32 = rows[2].Dot(cols[3]);
			result.m03 = rows[3].Dot(cols[0]);
			result.m13 = rows[3].Dot(cols[1]);
			result.m23 = rows[3].Dot(cols[2]);
			result.m33 = rows[3].Dot(cols[3]);

			return result;
		}

		/// <summary>
		/// Multiplies two matrices.
		/// </summary>
		/// <returns>A new instance containing the result.</returns>
		public static Matrix4 operator *(Matrix4 a, Matrix4 b)
		{
			return Matrix4.Multiply(a, b);
		}

		/// <summary>Multiply the matrix and a coordinate</summary>
		/// <param name="matrix"></param>
		/// <param name="value"></param>
		/// <returns>Result matrix</returns>
		public static XYZ operator *(Matrix4 matrix, XYZ value)
		{
			XYZM xyzm = new XYZM(value.X, value.Y, value.Z, 1);
			var rows = matrix.GetRows();

			XYZ result = new XYZ
			{
				X = rows[0].Dot(xyzm),
				Y = rows[1].Dot(xyzm),
				Z = rows[2].Dot(xyzm)
			};

			return result;
		}

		/// <summary>Multiply the matrix and XYZM</summary>
		/// <param name="matrix"></param>
		/// <param name="v"></param>
		/// <returns>Result matrix</returns>
		public static XYZM operator *(Matrix4 matrix, XYZM v)
		{
			return new XYZM(
				matrix.m00 * v.X + matrix.m10 * v.Y + matrix.m20 * v.Z + matrix.m30 * v.M,
				matrix.m01 * v.X + matrix.m11 * v.Y + matrix.m21 * v.Z + matrix.m31 * v.M,
				matrix.m02 * v.X + matrix.m12 * v.Y + matrix.m22 * v.Z + matrix.m32 * v.M,
				matrix.m03 * v.X + matrix.m13 * v.Y + matrix.m23 * v.Z + matrix.m33 * v.M);
		}
	}
}
