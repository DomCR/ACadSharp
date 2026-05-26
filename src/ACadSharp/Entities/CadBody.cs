using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="CadBody"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityBody"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityBody)]
	public class CadBody : ModelerGeometry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BODY;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityBody;
	}
}
