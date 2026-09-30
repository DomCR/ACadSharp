using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Entities;

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

	/// <inheritdoc/>
	public override DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.LoftedSurface,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntityLoftedSurface,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}
}
