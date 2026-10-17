using ACadSharp.Attributes;
using ACadSharp.Classes;
using CSMath;
using System.IO;

namespace ACadSharp.Entities;

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
	/// Gets the DXF class number.
	/// </summary>
	[DxfCodeValue(91)]
	public int ClassNumber { get { return this.DxfClass?.ClassNumber ?? 0; } }

	/// <inheritdoc/>
	/// <remarks>No <c>DxfCodeValue</c>: the DXF reader has no handler for code 311 on an entity.</remarks>
	public Stream Data { get; set; }

	/// <summary>
	/// Object drawing format when it becomes a proxy: <br/>
	/// Low word is AcDbDwgVersion. <br/>
	/// High word is MaintenanceReleaseVersion.
	/// </summary>
	[DxfCodeValue(95)]
	public int DrawingFormat { get { return (int)this.Version | (this.MaintenanceVersion << 16); } }

	/// <summary>
	/// Gets or sets the DXF class associated with the object.
	/// </summary>
	public DxfClass DxfClass { get; set; }

	/// <summary>
	/// Gets or sets the maintenance version number.
	/// </summary>
	[DxfCodeValue(97)]
	public int MaintenanceVersion { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityProxyEntity;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.ACAD_PROXY_ENTITY;

	/// <inheritdoc/>
	[DxfCodeValue(70)]
	public bool OriginalDataFormatDxf { get; set; } = false;

	/// <inheritdoc/>
	/// <remarks>
	/// Always 498
	/// </remarks>
	[DxfCodeValue(90)]
	public int ProxyClassId { get; } = 498;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.ProxyEntity;

	/// <inheritdoc/>
	[DxfCodeValue(71)]
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