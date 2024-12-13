using System;

namespace CSMath
{
	public partial struct XYZM : IVector, IEquatable<XYZM>
	{
		public readonly static XYZM Zero = new XYZM(0, 0, 0, 0);
		public readonly static XYZM AxisX = new XYZM(1, 0, 0, 0);
		public readonly static XYZM AxisY = new XYZM(0, 1, 0, 0);
		public readonly static XYZM AxisZ = new XYZM(0, 0, 1, 0);
		public readonly static XYZM AxisM = new XYZM(0, 0, 0, 1);

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

		/// <summary>
		/// Specifies the M-value of the vector component
		/// </summary>
		public double M { get; set; }

		/// <inheritdoc/>
		public uint Dimension { get { return 4; } }

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
					case 3:
						return M;
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
					case 3:
						M = value;
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
		/// <param name="m">Value of the M-coordinate</param>
		public XYZM(double x, double y, double z, double m)
		{
			X = x;
			Y = y;
			Z = z;
			M = m;
		}

		/// <summary>
		/// Constructs a vector whose elements are all the single specified value.
		/// </summary>
		/// <param name="value">The element to fill the vector with.</param>
		public XYZM(double value) : this(value, value, value, value) { }

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (!(obj is XYZM other))
				return false;

			return this.Equals(other);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal with in a specific precison.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="digits">number of decimals</param>
		/// <returns></returns>
		public bool Equals(XYZM other, int digits)
		{
			return other.IsEqual(this, digits);
		}

		/// <inheritdoc/>
		public bool Equals(XYZM other)
		{
			return this.X == other.X && this.Y == other.Y && this.Z == other.Z && this.M == other.M;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.M.GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{X},{Y},{Z},{M}";
		}
	}
}
