using System;

namespace CSMath
{
	public interface IVector
	{
		/// <summary>
		/// Get the dimension of the <see cref="IVector"/>.
		/// </summary>
		uint Dimension { get; }

		/// <summary>
		/// Value of the coordinate at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns>The value of the coordinate at the specified index.</returns>
		public double this[int index] { get; set; }
	}
}
