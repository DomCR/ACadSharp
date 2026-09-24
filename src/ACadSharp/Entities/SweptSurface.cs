using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="SweptSurface"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntitySweptSurface"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.SweptSurface"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntitySweptSurface)]
	[DxfSubClass(DxfSubclassMarker.SweptSurface)]
	public class SweptSurface : Surface
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntitySweptSurface;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.SweptSurface;
	}
}
