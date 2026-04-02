using ACadSharp.Attributes;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="AttributeDefinition"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityAttributeDefinition"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.AttributeDefinition"/>
/// </remarks>
[DxfName(DxfFileToken.EntityAttributeDefinition)]
[DxfSubClass(DxfSubclassMarker.AttributeDefinition)]
public class AttributeDefinition : AttributeBase
{
	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.ATTDEF;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityAttributeDefinition;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AttributeDefinition;

	/// <summary>
	/// Prompt text for this attribute.
	/// </summary>
	[DxfCodeValue(3)]
	public string Prompt { get; set; } = string.Empty;

	/// <summary>
	/// Initializes a new instance of the AttributeDefinition class.
	/// </summary>
	public AttributeDefinition() : base() { }

	/// <summary>
	/// Initializes a new instance of the AttributeDefinition class using the specified attribute entity.
	/// </summary>
	/// <param name="entity">The attribute entity whose properties are used to initialize the new AttributeDefinition instance. Cannot be null.</param>
	public AttributeDefinition(AttributeEntity entity) : this()
	{
		this.matchAttributeProperties(entity);
	}
}
