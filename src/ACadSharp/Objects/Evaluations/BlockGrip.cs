using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations
{
	[DxfSubClass(DxfSubclassMarker.BlockGrip)]
	public abstract class BlockGrip : BlockElement
	{
		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ Location { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockGrip;

		[DxfCodeValue(280)]
		public short Value280 { get; set; }

		[DxfCodeValue(91)]
		public int Value91 { get; set; }

		[DxfCodeValue(92)]
		public int Value92 { get; set; }

		[DxfCodeValue(93)]
		public int Value93 { get; set; }
	}
}