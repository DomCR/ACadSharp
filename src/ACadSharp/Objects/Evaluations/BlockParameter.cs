using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockParameter)]
public abstract class BlockParameter : BlockElement
{
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockParameter;

	[DxfCodeValue(280)]
	public bool ShowProperties { get; set; } = true;

	[DxfCodeValue(281)]
	public bool ChainActions { get; set; } = false;
}
