using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations
{
	[DxfSubClass(DxfSubclassMarker.Block1PtParameter)]
	public abstract class Block1PtParameter : BlockParameter
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Block1PtParameter;

		/// <summary>
		/// Location for parameter to be placed in the block.
		/// </summary>
		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ Location { get; set; }

		[DxfCodeValue(93)]
		internal long Value93 { get; set; }

		[DxfCodeValue(170)]
		internal short Value170 { get; set; }

		[DxfCodeValue(171)]
		internal short Value171 { get; set; }
	}
}
