using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKALIGNMENTPARAMETER object, used in AutoCAD to control the
/// alignment in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockAlignmentParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockAlignmentParameter"/>
/// </remarks>

[DxfName(DxfFileToken.ObjectBlockAlignmentParameter)]
[DxfSubClass(DxfSubclassMarker.BlockAlignmentParameter)]
public class BlockAlignmentParameter : Block2PtParameter, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockAlignmentParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockAlignmentParameter;

	/// <summary>
	/// Gets or sets the perpendicular flag.
	/// </summary>
	[DxfCodeValue(280)]
	public bool IsPerpendicular { get; internal set; }

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockAlignmentParameter,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockAlignmentParameter,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}