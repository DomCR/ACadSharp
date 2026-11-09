using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Entities;

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

	/// <inheritdoc/>
	public override DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.NurbSurface,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntityNurbSurface,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		};
	}
}
