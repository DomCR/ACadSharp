using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockActionBasePt)]
public abstract class BlockActionBasePt : BlockAction
{
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockActionBasePt;

	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ Value1011 { get; set; }

	[DxfCodeValue(1012, 1022, 1032)]
	public XYZ Value1012 { get; set; }

	[DxfCodeValue(280)]
	public bool Value280 { get; set; }

	[DxfCodeValue(301)]
	public string Value301 { get; set; }

	[DxfCodeValue(302)]
	public string Value302 { get; set; }

	[DxfCodeValue(92)]
	public int Value92 { get; set; }

	[DxfCodeValue(93)]
	public int Value93 { get; set; }
}