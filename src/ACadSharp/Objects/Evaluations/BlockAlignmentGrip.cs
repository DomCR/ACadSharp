using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKALIGNMENTGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockAlignmentGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockAlignmentGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockAlignmentGrip)]
[DxfSubClass(DxfSubclassMarker.BlockAlignmentGrip)]
public class BlockAlignmentGrip : BlockGrip, IDxfClassDefined
{
	/// <summary>
	/// Gets or sets the alignment in the X direction.
	/// </summary>
	[DxfCodeValue(140)]
	public double AlignmentX { get; set; }

	/// <summary>
	/// Gets or sets the alignment in the Y direction.
	/// </summary>
	[DxfCodeValue(141)]
	public double AlignmentY { get; set; }

	/// <summary>
	/// Gets or sets the alignment in the Z direction.
	/// </summary>
	[DxfCodeValue(142)]
	public double AlignmentZ { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockAlignmentGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockAlignmentGrip;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockAlignmentGrip,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockAlignmentGrip,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}