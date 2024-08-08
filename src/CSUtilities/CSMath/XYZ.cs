using System;

namespace CSMath
{
	public partial struct XYZ : IVector, IEquatable<XYZ>
	{
		public readonly static XYZ Zero = new XYZ(0, 0, 0);
		public readonly static XYZ AxisX = new XYZ(1, 0, 0);
		public readonly static XYZ AxisY = new XYZ(0, 1, 0);
		public readonly static XYZ AxisZ = new XYZ(0, 0, 1);

		/// <summary>
		/// Specifies the X-value of the vector component
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Specifies the Y-value of the vector component
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Specifies the Z-value of the vector component
		/// </summary>
		public double Z { get; set; }

		/// <inheritdoc/>
		public uint Dimension { get { return 3; } }

		/// <inheritdoc/>
		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					default:
						throw new IndexOutOfRangeException($"The index must be between 0 and {this.Dimension}.");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					case 2:
						Z = value;
						break;
					default:
						throw new IndexOutOfRangeException($"The index must be between 0 and {this.Dimension}.");
				}
			}
		}

		/// <summary>
		/// Constructor with the coordinate components
		/// </summary>
		/// <param name="x">Value of the X-coordinate</param>
		/// <param name="y">Value of the Y-coordinate</param>
		/// <param name="z">Value of the Z-coordinate</param>
		public XYZ(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Constructs a vector whose elements are all the single specified value.
		/// </summary>
		/// <param name="value">The element to fill the vector with.</param>
		public XYZ(double value) : this(value, value, value) { }

		[Obsolete("Deprecated")]
		public XYZ(double[] components) : this(components[0], components[1], components[2]) { }

		[Obsolete("Deprecated")]
		public static XYZ CreateFrom(double[] arr)
		{
			return CreateFrom(arr, 0);
		}

		[Obsolete("Deprecated")]
		public static XYZ CreateFrom(double[] arr, int offset)
		{
			double[] values = new double[3];
			for (int i = offset; i < arr.Length && i < values.Length + offset; i++)
			{
				values[i] = (double)arr[i];
			}

			return new XYZ(values);
		}

		/// <summary>
		/// Computes the cross product of two coordinates.
		/// </summary>
		/// <param name="xyz1">The first coordinate.</param>
		/// <param name="xyz2">The second coordinate.</param>
		/// <returns>The cross product.</returns>
		public static XYZ Cross(XYZ xyz1, XYZ xyz2)
		{
			return new XYZ(
				xyz1.Y * xyz2.Z - xyz1.Z * xyz2.Y,
				xyz1.Z * xyz2.X - xyz1.X * xyz2.Z,
				xyz1.X * xyz2.Y - xyz1.Y * xyz2.X);
		}

		public static XYZ FindNormal(XYZ point1, XYZ point2, XYZ point3)
		{
			XYZ a = point2.Subtract(point1);
			XYZ b = point3.Subtract(point1);

			// N = Cross(a, b)
			XYZ n = XYZ.Cross(a, b);
			XYZ normal = n.Normalize();

			return normal;
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (!(obj is XYZ other))
				return false;

			return this.Equals(other);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal with in a specific precison.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="digits">number of decimals</param>
		/// <returns></returns>
		public bool Equals(XYZ other, int digits)
		{
			return other.IsEqual(this, digits);
		}

		/// <inheritdoc/>
		public bool Equals(XYZ other)
		{
			return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{X},{Y},{Z}";
		}
	}
}
