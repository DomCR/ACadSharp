using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations
{
	[DxfSubClass(DxfSubclassMarker.Block2PtParameter)]
	public abstract class Block2PtParameter : BlockParameter
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Block2PtParameter;

		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ FirstPoint { get; set; }

		[DxfCodeValue(1011, 1021, 1031)]
		public XYZ SecondPoint { get; set; }

		//	Appears on DXF
		//170	BS	4
		//91	BL	31
		//91	BL	0
		//91	BL	0
		//91	BL	0

		//171	BS	0
		//172	BS	0
		//173	BS	0
		//174	BS	0
		//177	BS	0
	}
}