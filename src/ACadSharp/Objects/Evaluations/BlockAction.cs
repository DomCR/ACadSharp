using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockAction)]
public abstract class BlockAction : BlockElement
{
	/// <summary>
	/// Whatever this is?
	/// </summary>
	[DxfCodeValue(1010, 1020, 1030)]
	public XYZ ActionPoint { get; set; }

	/// <summary>
	/// Gets the list of <see cref="Entity"/> objects affected by this <see cref="BlockAction"/>.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 71)]
	[DxfCollectionCodeValue(DxfReferenceType.Handle, 330)]
	public List<Entity> Entities { get; } = new List<Entity>();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockAction;

	[DxfCodeValue(70)]
	public short Value70 { get; set; }
}