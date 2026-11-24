using ACadSharp.Attributes;
using ACadSharp.Classes;
using CSMath;

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
	/// <summary>
	/// Align angle, in radians.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 49)]
	public double AlignAngle { get; set; }

	/// <summary>
	/// Flag that indicates if the sweep entity is aligned at the start of the path.
	/// </summary>
	[DxfCodeValue(292)]
	public bool AlignStart { get; set; }

	/// <summary>
	/// Flag that indicates if the sweep entity banks (rotates) along a 3D path.
	/// </summary>
	[DxfCodeValue(293)]
	public bool Bank { get; set; }

	/// <summary>
	/// Flag that indicates if the base point has been set.
	/// </summary>
	[DxfCodeValue(294)]
	public bool BasePointSet { get; set; }

	/// <summary>
	/// Binary data of the extruded entity.
	/// </summary>
	[DxfCodeValue(310)]
	public byte[] BinaryData { get; set; }

	/// <summary>
	/// Class ID.
	/// </summary>
	[DxfCodeValue(90)]
	public int? ClassId { get; set; }

	/// <summary>
	/// Sweep vector that defines the direction and length of the extrusion.
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ Direction { get; set; }

	/// <summary>
	/// Draft angle, in radians.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 42)]
	public double DraftAngle { get; set; }

	/// <summary>
	/// Draft end distance.
	/// </summary>
	[DxfCodeValue(44)]
	public double DraftEndDistance { get; set; }

	/// <summary>
	/// Draft start distance.
	/// </summary>
	[DxfCodeValue(43)]
	public double DraftStartDistance { get; set; }

	/// <summary>
	/// Transform matrix of the extruded entity.
	/// </summary>
	public Matrix4 ExtrusionTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Flag that indicates if the extrusion creates a solid.
	/// </summary>
	[DxfCodeValue(290)]
	public bool IsSolid { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityExtrudedSurface;

	/// <summary>
	/// Transform matrix of the path entity.
	/// </summary>
	public Matrix4 PathEntityTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Flag that indicates if the path entity transform has been computed.
	/// </summary>
	[DxfCodeValue(296)]
	public bool PathEntityTransformComputed { get; set; }

	/// <summary>
	/// Reference vector for controlling twist.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ ReferenceVector { get; set; }

	/// <summary>
	/// Scale factor applied along the extrusion.
	/// </summary>
	[DxfCodeValue(48)]
	public double ScaleFactor { get; set; } = 1.0;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.ExtrudedSurface;

	/// <summary>
	/// Sweep alignment option.
	/// </summary>
	[DxfCodeValue(70)]
	public SweepAlignment SweepAlignment { get; set; }

	/// <summary>
	/// Transform matrix of the sweep entity.
	/// </summary>
	public Matrix4 SweepEntityTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Flag that indicates if the sweep entity transform has been computed.
	/// </summary>
	[DxfCodeValue(295)]
	public bool SweepEntityTransformComputed { get; set; }

	/// <summary>
	/// Twist angle, in radians.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 45)]
	public double TwistAngle { get; set; }

	/// <summary>
	/// Undocumented value.
	/// </summary>
	[DxfCodeValue(71)]
	public short Unknown71 { get; set; }

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