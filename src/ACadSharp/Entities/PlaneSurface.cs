using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="PlaneSurface"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityPlaneSurface"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.PlaneSurface"/>
/// </remarks>
[DxfName(DxfFileToken.EntityPlaneSurface)]
[DxfSubClass(DxfSubclassMarker.PlaneSurface)]
public class PlaneSurface : Surface
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityPlaneSurface;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.PlaneSurface;

	/// <inheritdoc/>
	public override DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.PlaneSurface,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntityPlaneSurface,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		};
	}
}
