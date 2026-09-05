using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="NurbSurface"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityNurbSurface"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.NurbSurface"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityNurbSurface)]
	[DxfSubClass(DxfSubclassMarker.NurbSurface)]
	public class NurbSurface : Surface
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityNurbSurface;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.NurbSurface;
	}
}
