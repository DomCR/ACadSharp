using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="Surface"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntitySurface"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Surface"/>
/// </remarks>
[DxfName(DxfFileToken.EntitySurface)]
[DxfSubClass(DxfSubclassMarker.Surface)]
public class Surface : ModelerGeometry, IDxfClassDefined
{
	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntitySurface;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Surface;

	/// <summary>
	/// Number of U isolines.
	/// </summary>
	[DxfCodeValue(71)]
	public short UIsolines { get; set; }

	/// <summary>
	/// Number of V isolines.
	/// </summary>
	[DxfCodeValue(72)]
	public short VIsolines { get; set; }

	/// <inheritdoc/>
	public virtual DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.Surface,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntitySurface,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		};
	}
}
