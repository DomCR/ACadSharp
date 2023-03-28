using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
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

		/// <summary>
		/// Prompt text for this attribute.
		/// </summary>
		[DxfCodeValue(3)]
		public string Prompt { get; internal set; }

		public AttributeDefinition() : base() { }

		public AttributeDefinition(AttributeEntity entity) : this()
		{
			this.matchAttributeProperties(entity);
		}
	}
}
