using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="LoftedSurface"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityLoftedSurface"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.LoftedSurface"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityLoftedSurface)]
	[DxfSubClass(DxfSubclassMarker.LoftedSurface)]
	public class LoftedSurface : Surface
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityLoftedSurface;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LoftedSurface;
	}
}
