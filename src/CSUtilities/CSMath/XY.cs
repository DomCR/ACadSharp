using System;

namespace CSMath
{
	public partial struct XY : IVector, IEquatable<XY>
	{
		public readonly static XY Zero = new XY(0, 0);
		public readonly static XY AxisX = new XY(1, 0);
		public readonly static XY AxisY = new XY(0, 1);

		/// <summary>
		/// Specifies the X-value of the vector component
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Specifies the Y-value of the vector component
		/// </summary>
		public double Y { get; set; }

		/// <inheritdoc/>
		public uint Dimension { get { return 2; } }

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
		public XY(double x, double y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Constructs a vector whose components are all the single specified value.
		/// </summary>
		/// <param name="value">The element to fill the vector with.</param>
		public XY(double value) : this(value, value) { }

		/// <summary>
		/// Get the angle
		/// </summary>
		/// <returns>Angle in radians</returns>
		public double GetAngle()
		{
			return Math.Atan2(Y, X);
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (!(obj is XY other))
				return false;

			return this.Equals(other);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal with in a specific precison.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="digits">number of decimals</param>
		/// <returns></returns>
		public bool Equals(XY other, int digits)
		{
			return other.IsEqual(this, digits);
		}

		/// <inheritdoc/>
		public bool Equals(XY other)
		{
			return this.X == other.X && this.Y == other.Y;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{X},{Y}";
		}
	}
}
