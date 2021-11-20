using ACadSharp.Attributes;
using ACadSharp.Tables;

namespace ACadSharp.Objects
{
	public partial class MLStyle
	{
		public class Element
		{
			/// <summary>
			/// Element offset
			/// </summary>
			[DxfCodeValue(49)]
			public double Offset { get; set; }

			/// <summary>
			/// Element color
			/// </summary>
			[DxfCodeValue(62)]
			public Color Color { get; set; } = Color.ByBlock;

			/// <summary>
			/// Element linetype
			/// </summary>
			[DxfCodeValue(6)]
			public LineType LineType { get; set; } = LineType.ByLayer;
		}
	}
}
