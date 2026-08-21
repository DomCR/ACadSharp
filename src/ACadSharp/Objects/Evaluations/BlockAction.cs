using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockAction)]
public abstract class BlockAction : BlockElement
{
	/// <summary>
	/// Gets the list of <see cref="Entity"/> objects affected by this <see cref="BlockAction"/>.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 71)]
	[DxfCollectionCodeValue(DxfReferenceType.Handle, 330)]
	public List<Entity> Entities { get; } = new List<Entity>();

	/// <summary>
	/// Every object the action refers to, in the order the file gives them. The file does not
	/// restrict this list to entities: an action refers to the parameters, grips and other actions
	/// it is wired to, and a production drawing had 385 such references in 118 actions - all of
	/// them dropped on write while only <see cref="Entities"/> was written. Objects that are
	/// entities appear in both lists; the writers write this list plus any entity added only to
	/// <see cref="Entities"/>.
	/// </summary>
	public List<CadObject> Elements { get; } = new List<CadObject>();

	/// <summary>
	/// The references the writers put in the file: <see cref="Elements"/> in file order, followed
	/// by any <see cref="Entities"/> member that is not already among them.
	/// </summary>
	public IEnumerable<CadObject> GetReferencedObjects()
	{
		foreach (CadObject element in this.Elements)
		{
			yield return element;
		}

		foreach (Entity entity in this.Entities)
		{
			if (!this.Elements.Contains(entity))
			{
				yield return entity;
			}
		}
	}

	/// <summary>
	/// Gets or sets the position of the action label.
	/// </summary>
	[DxfCodeValue(1010, 1020, 1030)]
	public XYZ LabelPosition { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockAction;

	[DxfCodeValue(DxfReferenceType.Count, 70)]
	[DxfCollectionCodeValue(91)]
	public List<int> ParametersIds { get; } = new();
}