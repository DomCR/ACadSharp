using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	[DxfSubClass(DxfSubclassMarker.BlockParameter)]
	public abstract class BlockParameter : BlockElement
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockParameter;

		[DxfCodeValue(280)]
		internal bool Value280 { get; set; }

		[DxfCodeValue(281)]
		internal bool Value281 { get; set; }
	}
}
