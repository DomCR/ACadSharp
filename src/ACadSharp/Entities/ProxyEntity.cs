using ACadSharp.Attributes;
using ACadSharp.Classes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="ProxyEntity"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityProxyEntity"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.ProxyEntity"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityProxyEntity)]
	[DxfSubClass(DxfSubclassMarker.ProxyEntity)]
	public class ProxyEntity : Entity, IProxy
	{
		/// <summary>
		/// Application dxf class ID.
		/// </summary>
		[DxfCodeValue(91)]
		public int ClassId { get { return this.DxfClass.ClassNumber; } }

		/// <summary>
		/// Object drawing format when it becomes a proxy: <br/>
		/// Low word is AcDbDwgVersion. <br/>
		/// High word is MaintenanceReleaseVersion.
		/// </summary>
		[DxfCodeValue(95)]
		public int DrawingFormat { get { return (int)this.Version | (this.MaintenanceVersion << 16); } }

		public DxfClass DxfClass { get; set; }

		public int MaintenanceVersion { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityProxyEntity;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ACAD_PROXY_ENTITY;

		/// <inheritdoc/>
		[DxfCodeValue(70)]
		public bool OriginalDataFormatDxf { get; set; }

		/// <inheritdoc/>
		/// <remarks>
		/// Always 498
		/// </remarks>
		[DxfCodeValue(90)]
		public int ProxyClassId { get; } = 498;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ProxyEntity;

		//92 Size of graphics data in bytes
		//310 Binary graphics data(multiple entries can appear) (optional)

		//93 Size of entity data in bits
		//310 Binary entity data(multiple entries can appear) (optional)

		//330 or 340
		//or 350 or 360
		//An object ID(multiple entries can appear) (optional)

		//94 0 (indicates end of object ID section)
		/// <inheritdoc/>
		public ACadVersion Version { get; set; }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}