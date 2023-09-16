using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="AttributeEntity"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityAttribute"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Attribute"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityAttribute)]
	[DxfSubClass(DxfSubclassMarker.Attribute)]
	public class AttributeEntity : AttributeBase
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ATTRIB;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityAttribute;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Attribute;

		/// <summary>
		/// Default constructor
		/// </summary>
		public AttributeEntity() : base() { }

		public AttributeEntity(AttributeDefinition definition) : this()
		{
			this.matchAttributeProperties(definition);
		}
	}
}
