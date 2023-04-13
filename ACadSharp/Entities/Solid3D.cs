using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Solid"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.Entity3DSolid"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.ModelerGeometry"/>
	/// </remarks>
	[DxfName(DxfFileToken.Entity3DSolid)]
	[DxfSubClass(DxfSubclassMarker.ModelerGeometry)]
	public class Solid3D : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SOLID3D;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.Entity3DSolid;

		public override object Clone()
		{
			throw new System.NotImplementedException();
		}
	}
}
