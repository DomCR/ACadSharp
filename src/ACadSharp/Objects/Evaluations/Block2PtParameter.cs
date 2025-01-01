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
	}
}
