using System;

namespace CSMath
{
	/// <summary>
	/// Four dimensional vector which is used to efficiently rotate an object about the (x,y,z) vector by the angle theta, where w = cos(theta/2).
	/// </summary>
	public struct Quaternion : IEquatable<Quaternion>
	{
		/// <summary>
		/// Specifies the X-value of the vector component of the Quaternion.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Specifies the Y-value of the vector component of the Quaternion.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Specifies the Z-value of the vector component of the Quaternion.
		/// </summary>
		public double Z { get; set; }

		/// <summary>
		/// Specifies the rotation component of the Quaternion.
		/// </summary>
		public double W { get; set; }

		/// <summary>
		/// Returns a Quaternion representing no rotation. 
		/// </summary>
		public static Quaternion Identity
		{
			get { return new Quaternion(0, 0, 0, 1); }
		}

		/// <summary>
		/// Constructs a Quaternion from the given components.
		/// </summary>
		/// <param name="x">The X component of the Quaternion.</param>
		/// <param name="y">The Y component of the Quaternion.</param>
		/// <param name="z">The Z component of the Quaternion.</param>
		/// <param name="w">The W component of the Quaternion.</param>
		public Quaternion(double x, double y, double z, double w)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}

		/// <summary>
		/// Constructs a Quaternion from the given vector and rotation parts.
		/// </summary>
		/// <param name="vectorPart">The vector part of the Quaternion.</param>
		/// <param name="scalarPart">The rotation part of the Quaternion.</param>
		public Quaternion(XYZ vectorPart, double scalarPart)
		{
			X = vectorPart.X;
			Y = vectorPart.Y;
			Z = vectorPart.Z;
			W = scalarPart;
		}

		/// <summary>
		/// Creates a new Quaternion from the given yaw, pitch, and roll, in radians.
		/// </summary>
		/// <param name="xyz">X as pitch, Y as yaw and Z as roll</param>
		/// <returns></returns>
		public static Quaternion CreateFromYawPitchRoll(XYZ xyz)
		{
			return CreateFromYawPitchRoll(xyz.X, xyz.Y, xyz.Z);
		}

		/// <summary>
		/// Creates a new Quaternion from the given yaw, pitch, and roll, in radians.
		/// </summary>
		/// <param name="pitch">The pitch angle, around the X-axis.</param>
		/// <param name="yaw">The yaw angle, around the Y-axis.</param>
		/// <param name="roll">The roll angle, around the Z-axis.</param>
		/// <remarks>
		/// The values must be in radians
		/// </remarks>
		/// <returns></returns>
		public static Quaternion CreateFromYawPitchRoll(double pitch, double yaw, double roll)
		{
			double sr, cr, sp, cp, sy, cy;

			double halfPitch = pitch * 0.5f;
			sp = (double)Math.Sin(halfPitch);
			cp = (double)Math.Cos(halfPitch);

			double halfYaw = yaw * 0.5f;
			sy = (double)Math.Sin(halfYaw);
			cy = (double)Math.Cos(halfYaw);

			double halfRoll = roll * 0.5f;
			sr = (double)Math.Sin(halfRoll);
			cr = (double)Math.Cos(halfRoll);

			Quaternion result = new Quaternion
			{
				X = cy * sp * cr + sy * cp * sr,
				Y = sy * cp * cr - cy * sp * sr,
				Z = cy * cp * sr - sy * sp * cr,
				W = cy * cp * cr + sy * sp * sr
			};

			return result;
		}

		/// <summary>
		/// Creates a Quaternion from the given rotation matrix.
		/// </summary>
		/// <param name="matrix">The rotation matrix.</param>
		/// <returns>The created Quaternion.</returns>
		public static Quaternion CreateFromRotationMatrix(Matrix4 matrix)
		{
			double trace = matrix.m00 + matrix.m11 + matrix.m22;

			Quaternion q = new Quaternion();

			if (trace > 0.0f)
			{
				float s = (float)Math.Sqrt(trace + 1.0f);
				q.W = s * 0.5f;
				s = 0.5f / s;
				q.X = (matrix.m12 - matrix.m21) * s;
				q.Y = (matrix.m20 - matrix.m02) * s;
				q.Z = (matrix.m01 - matrix.m10) * s;
			}
			else
			{
				if (matrix.m00 >= matrix.m11 && matrix.m00 >= matrix.m22)
				{
					float s = (float)Math.Sqrt(1.0f + matrix.m00 - matrix.m11 - matrix.m22);
					float invS = 0.5f / s;
					q.X = 0.5f * s;
					q.Y = (matrix.m01 + matrix.m10) * invS;
					q.Z = (matrix.m02 + matrix.m20) * invS;
					q.W = (matrix.m12 - matrix.m21) * invS;
				}
				else if (matrix.m11 > matrix.m22)
				{
					float s = (float)Math.Sqrt(1.0f + matrix.m11 - matrix.m00 - matrix.m22);
					float invS = 0.5f / s;
					q.X = (matrix.m10 + matrix.m01) * invS;
					q.Y = 0.5f * s;
					q.Z = (matrix.m21 + matrix.m12) * invS;
					q.W = (matrix.m20 - matrix.m02) * invS;
				}
				else
				{
					float s = (float)Math.Sqrt(1.0f + matrix.m22 - matrix.m00 - matrix.m11);
					float invS = 0.5f / s;
					q.X = (matrix.m20 + matrix.m02) * invS;
					q.Y = (matrix.m21 + matrix.m12) * invS;
					q.Z = 0.5f * s;
					q.W = (matrix.m01 - matrix.m10) * invS;
				}
			}

			return q;
		}

		/// <summary>
		/// Create a rotation matrix
		/// </summary>
		/// <returns></returns>
		public Matrix4 ToMatrix()
		{
			return Matrix4.CreateFromQuaternion(this);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{X},{Y},{Z},{W}";
		}

		/// <inheritdoc/>
		public bool Equals(Quaternion other)
		{
			return (X == other.X &&
					Y == other.Y &&
					Z == other.Z &&
					W == other.W);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="ndecimals">Number of decimals digits to be set as precision.</param>
		/// <returns></returns>
		public bool Equals(Quaternion other, int ndecimals)
		{
			return (Math.Round(X, ndecimals) == Math.Round(other.X, ndecimals) &&
					Math.Round(Y, ndecimals) == Math.Round(other.Y, ndecimals) &&
					Math.Round(Z, ndecimals) == Math.Round(other.Z, ndecimals) &&
					Math.Round(W, ndecimals) == Math.Round(other.W, ndecimals));
		}
	}
}
