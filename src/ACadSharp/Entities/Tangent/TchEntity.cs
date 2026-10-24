using ACadSharp.Classes;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent;

/// <summary>
/// Base class for every entity that comes from a Tianzheng drawing.
/// </summary>
public abstract class TchEntity : Entity
{
	/// <summary>
	/// Gets the class this entity was read from.
	/// </summary>
	/// <remarks>
	/// The entity type says how the object was read; this says which Tianzheng class it is.
	/// Two objects of different classes share a type when they are read the same way, which is
	/// why the class is kept rather than encoded in the type.
	/// </remarks>
	public DxfClass DxfClass { get; }

	/// <inheritdoc/>
	public override string ObjectName => this.DxfClass?.DxfName ?? TchUnknownClassName;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNDEFINED;

	/// <inheritdoc/>
	public override string SubclassMarker => this.DxfClass?.CppClassName ?? DxfSubclassMarker.Entity;

	/// <summary>
	/// Represents the class name used when the TCH class is unknown.
	/// </summary>
	public const string TchUnknownClassName = "TCH_UNKNOWN";

	internal TchEntity(DxfClass dxfClass)
	{
		this.DxfClass = dxfClass;
	}

	/// <summary>
	/// Converts the current instance to a sequence of standard <see cref="Entity"/> objects.
	/// </summary>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Entity"/> instances in standard form.</returns>
	public abstract IEnumerable<Entity> ToStandardEntities();
}