using ACadSharp.Attributes;
using ACadSharp.Classes;
using CSMath;
using System.Collections.Generic;

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
public partial class LoftedSurface : Surface
{
	/// <summary>
	/// Flag that indicates if the loft aligns the direction of the cross sections.
	/// </summary>
	[DxfCodeValue(292)]
	public bool AlignDirection { get; set; }

	/// <summary>
	/// Flag that indicates if the loft uses arc length parameterization.
	/// </summary>
	[DxfCodeValue(290)]
	public bool ArcLengthParameterization { get; set; }

	/// <summary>
	/// Flag that indicates if the loft creates closed surfaces.
	/// </summary>
	[DxfCodeValue(294)]
	public bool ClosedSurfaces { get; set; }

	/// <summary>
	/// Gets the cross sections that define the lofted surface.
	/// </summary>
	public List<Profile> CrossSections { get; private set; } = new();

	/// <summary>
	/// Draft angle at the end of the loft, in radians.
	/// </summary>
	[DxfCodeValue(42)]
	public double EndDraftAngle { get; set; }

	/// <summary>
	/// Draft magnitude at the end of the loft.
	/// </summary>
	[DxfCodeValue(44)]
	public double EndDraftMagnitude { get; set; }

	/// <summary>
	/// Gets the profiles that guide the lofted surface.
	/// </summary>
	public List<Profile> Guides { get; private set; } = new();

	/// <summary>
	/// Flag that indicates if the loft creates a solid.
	/// </summary>
	[DxfCodeValue(295)]
	public bool IsSolid { get; set; }

	/// <summary>
	/// Transform matrix of the loft entity.
	/// </summary>
	/// <remarks>
	/// Stored in the DXF as 16 consecutive values with code 40.
	/// </remarks>
	public Matrix4 LoftTransform { get; set; } = Matrix4.Identity;

	/// <summary>
	/// Flag that indicates if twisting is prevented.
	/// </summary>
	[DxfCodeValue(291)]
	public bool NoTwist { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityLoftedSurface;

	/// <summary>
	/// Gets or sets the profile used to define the path.
	/// </summary>
	public Profile Path { get; set; }

	/// <summary>
	/// Plane normal lofting type.
	/// </summary>
	[DxfCodeValue(70)]
	public LoftPlaneNormalType PlaneNormalLofting { get; set; }

	/// <summary>
	/// Flag that indicates if the loft creates ruled surfaces.
	/// </summary>
	[DxfCodeValue(296)]
	public bool RuledSurface { get; set; }

	/// <summary>
	/// Flag that indicates if the loft creates simple surfaces.
	/// </summary>
	[DxfCodeValue(293)]
	public bool SimpleSurfaces { get; set; }

	/// <summary>
	/// Draft angle at the start of the loft, in radians.
	/// </summary>
	[DxfCodeValue(41)]
	public double StartDraftAngle { get; set; }

	/// <summary>
	/// Draft magnitude at the start of the loft.
	/// </summary>
	[DxfCodeValue(43)]
	public double StartDraftMagnitude { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.LoftedSurface;

	/// <summary>
	/// Flag that indicates if the loft uses a virtual guide.
	/// </summary>
	[DxfCodeValue(297)]
	public bool VirtualGuide { get; set; }

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		LoftedSurface clone = (LoftedSurface)base.Clone();

		clone.CrossSections = new List<Profile>();
		foreach (Profile profile in this.CrossSections)
		{
			clone.CrossSections.Add(profile.Clone());
		}

		clone.Guides = new List<Profile>();
		foreach (Profile profile in this.Guides)
		{
			clone.Guides.Add(profile.Clone());
		}

		clone.Path = this.Path?.Clone();

		return clone;
	}

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