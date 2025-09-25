using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Region"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityRegion"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityRegion)]
	public class Region : ModelerGeometry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.REGION;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityRegion;
	}
}
