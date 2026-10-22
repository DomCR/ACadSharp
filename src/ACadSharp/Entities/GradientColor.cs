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

		/// <summary>
		/// True color value as stored in the file (DXF code 421), -1 when absent.
		/// </summary>
		/// <remarks>
		/// The value is not always consistent with the ACI index of <see cref="Color"/>
		/// (exporters may compute their own RGB for a given ACI index), so the raw value is
		/// preserved to round-trip the file faithfully.
		/// </remarks>
		public int StoredColor { get; set; } = -1;

		public GradientColor Clone()
		{
			return (GradientColor)this.MemberwiseClone();
		}
	}
}
