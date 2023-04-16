using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	public class GradientColor
	{
		/// <summary>
		/// Gradient value
		/// </summary>
		/// <value>
		/// The value must be in the range 0-1
		/// </value>
		[DxfCodeValue(463)]
		public double Value { get; set; }

		/// <summary>
		/// Color for this gradient
		/// </summary>
		[DxfCodeValue(421)]
		public Color Color { get; set; }
	}
}
