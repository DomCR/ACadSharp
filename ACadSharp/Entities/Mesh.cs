using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Mesh"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPoint"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Mesh"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPoint)]
	[DxfSubClass(DxfSubclassMarker.Mesh)]
	public class Mesh : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPoint;
	}
}
