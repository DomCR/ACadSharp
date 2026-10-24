using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Entities;

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

	/// <inheritdoc/>
	public override DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.SweptSurface,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntitySweptSurface,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}
}
