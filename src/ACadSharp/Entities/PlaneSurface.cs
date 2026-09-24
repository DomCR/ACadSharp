using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
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
	}
}
