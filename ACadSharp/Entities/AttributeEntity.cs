using ACadSharp.Attributes;
using ACadSharp.IO.Templates;

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

		/// <summary>
		/// Default constructor
		/// </summary>
		public AttributeEntity() : base() { }
	}
}
