namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents supression of zeros in displaying decimal numbers.
	/// </summary>
	public enum ZeroHandling : byte
	{
		/// <summary>
		/// Suppress zero feet and exactly zero inches.
		/// </summary>
		SuppressZeroFeetAndInches = 0,
		/// <summary>
		/// Show zero feet and exactly zero inches.
		/// </summary>
		ShowZeroFeetAndInches = 1,
		/// <summary>
		/// Show zero feet and suppress zero inches.
		/// </summary>
		ShowZeroFeetSuppressZeroInches = 2,
		/// <summary>
		/// Suppress zero feet and show zero inches.
		/// </summary>
		SuppressZeroFeetShowZeroInches = 3,
		/// <summary>
		/// Suppress leading zeroes in decimal numbers.
		/// </summary>
		SuppressDecimalLeadingZeroes = 4,
		/// <summary>
		/// Suppress trailing zeroes in decimal numbers.
		/// </summary>
		SuppressDecimalTrailingZeroes = 8,
		/// <summary>
		/// Suppress both leading and trailing zeroes in decimal numbers
		/// </summary>
		SuppressDecimalLeadingAndTrailingZeroes = 12,
	}
}
