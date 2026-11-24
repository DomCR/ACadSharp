using ACadSharp.Attributes;
using ACadSharp.Classes;
using CSMath;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="RevolvedSurface"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityRevolvedSurface"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.RevolvedSurface"/>
/// </remarks>
[DxfName(DxfFileToken.EntityRevolvedSurface)]
[DxfSubClass(DxfSubclassMarker.RevolvedSurface)]
public class RevolvedSurface : Surface
{
	/// <summary>
	/// Point on the revolution axis.
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ AxisPoint { get; set; }

	/// <summary>
	/// Direction of the revolution axis.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ AxisVector { get; set; }

	/// <summary>
	/// Binary data of the revolved entity.
	/// </summary>
	[DxfCodeValue(310)]
	public byte[] BinaryData { get; set; }

	/// <summary>
	/// Class ID of the revolved entity.
	/// </summary>
	[DxfCodeValue(90)]
	public int? ClassId { get; set; }

	/// <summary>
	/// Flag that indicates if the revolution is closed to the axis.
	/// </summary>
	[DxfCodeValue(291)]
	public bool CloseToAxis { get; set; }

	/// <summary>
	/// Draft angle, in radians.
	/// </summary>
	[DxfCodeValue(43)]
	public double DraftAngle { get; set; }

	/// <summary>
	/// Draft end distance.
	/// </summary>
	[DxfCodeValue(45)]
	public double DraftEndDistance { get; set; }

	/// <summary>
	/// Draft start distance.
	/// </summary>
	[DxfCodeValue(44)]
	public double DraftStartDistance { get; set; }

	/// <summary>
	/// Flag that indicates if the revolution creates a solid.
	/// </summary>
	[DxfCodeValue(290)]
	public bool IsSolid { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityRevolvedSurface;

	/// <summary>
	/// Revolve angle, in radians.
	/// </summary>
	[DxfCodeValue(40)]
	public double RevolveAngle { get; set; }

	/// <summary>
	/// Transform matrix of the revolved entity.
	/// </summary>
	public Matrix4 RevolveTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Start angle, in radians.
	/// </summary>
	[DxfCodeValue(41)]
	public double StartAngle { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.RevolvedSurface;

	/// <summary>
	/// Twist angle, in radians.
	/// </summary>
	[DxfCodeValue(46)]
	public double TwistAngle { get; set; }

	/// <inheritdoc/>
	public override DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.RevolvedSurface,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntityRevolvedSurface,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}
}