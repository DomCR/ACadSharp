using ACadSharp.Attributes;
using ACadSharp.Classes;
using CSMath;

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
	/// Binary data of the sweep entity.
	/// </summary>
	[DxfCodeValue(310)]
	public byte[] BinaryData { get; set; }

	/// <summary>
	/// Class ID of the sweep entity.
	/// </summary>
	[DxfCodeValue(90)]
	public int? ClassId { get; set; }

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
	/// Flag that indicates if the sweep creates a solid.
	/// </summary>
	[DxfCodeValue(290)]
	public bool IsSolid { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntitySweptSurface;

	/// <summary>
	/// Binary data of the path entity.
	/// </summary>
	/// <remarks>
	/// Second code 310 block, after <see cref="PathClassId"/>.
	/// </remarks>
	public byte[] PathBinaryData { get; set; }

	/// <summary>
	/// Class ID of the path entity.
	/// </summary>
	[DxfCodeValue(91)]
	public int? PathClassId { get; set; }

	/// <summary>
	/// Transform matrix of the path entity (sweep options).
	/// </summary>
	public Matrix4 PathEntityTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Flag that indicates if the path entity transform has been computed.
	/// </summary>
	[DxfCodeValue(296)]
	public bool PathEntityTransformComputed { get; set; }

	/// <summary>
	/// Transform matrix of the path entity.
	/// </summary>
	public Matrix4 PathTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Reference vector for controlling twist.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ ReferenceVector { get; set; }

	/// <summary>
	/// Scale factor applied along the sweep.
	/// </summary>
	[DxfCodeValue(48)]
	public double ScaleFactor { get; set; } = 1.0;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.SweptSurface;

	/// <summary>
	/// Sweep alignment option.
	/// </summary>
	[DxfCodeValue(70)]
	public SweepAlignment SweepAlignment { get; set; }

	/// <summary>
	/// Transform matrix of the sweep entity (sweep options).
	/// </summary>
	public Matrix4 SweepEntityTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Flag that indicates if the sweep entity transform has been computed.
	/// </summary>
	[DxfCodeValue(295)]
	public bool SweepEntityTransformComputed { get; set; }

	/// <summary>
	/// Transform matrix of the sweep entity.
	/// </summary>
	public Matrix4 SweepTransform { get; set; } = Matrix4.Identity;

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