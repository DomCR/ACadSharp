using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Entities;

/// <summary>
/// Represents an <see cref="ExtrudedSurface"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityExtrudedSurface"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.ExtrudedSurface"/>
/// </remarks>
[DxfName(DxfFileToken.EntityExtrudedSurface)]
[DxfSubClass(DxfSubclassMarker.ExtrudedSurface)]
public class ExtrudedSurface : Surface
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityExtrudedSurface;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.ExtrudedSurface;

	/// <inheritdoc/>
	public override DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.ExtrudedSurface,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntityExtrudedSurface,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}
}
