using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations
{
	[DxfSubClass(DxfSubclassMarker.Block2PtParameter)]
	public abstract class Block2PtParameter : BlockParameter
	{
		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ FirstPoint { get; set; }

		[DxfCodeValue(1011, 1021, 1031)]
		public XYZ SecondPoint { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Block2PtParameter;

		[DxfCodeValue(170)]
		public short Value170 { get; set; }

		[DxfCodeValue(171)]
		public short Value171 { get; set; }

		[DxfCodeValue(172)]
		public short Value172 { get; set; }

		[DxfCodeValue(173)]
		public short Value173 { get; set; }

		[DxfCodeValue(174)]
		public short Value174 { get; set; }

		[DxfCodeValue(177)]
		public short Value177 { get; set; }

		[DxfCodeValue(303)]
		public string Value303 { get; set; }

		[DxfCodeValue(304)]
		public string Value304 { get; set; }

		[DxfCodeValue(94)]
		public int Value94 { get; set; }

		[DxfCodeValue(95)]
		public int Value95 { get; set; }
	}
}