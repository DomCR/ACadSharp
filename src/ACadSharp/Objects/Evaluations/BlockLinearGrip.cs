using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKLINEARGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockLinearGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockLinearGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockLinearGrip)]
[DxfSubClass(DxfSubclassMarker.BlockLinearGrip)]
public class BlockLinearGrip : BlockGrip, IDxfClassDefined
{
	/// <summary>
	/// Gets or sets the distance in the X direction.
	/// </summary>
	[DxfCodeValue(140)]
	public double DistanceX { get; set; }

	/// <summary>
	/// Gets or sets the distance in the Y direction.
	/// </summary>
	[DxfCodeValue(141)]
	public double DistanceY { get; set; }

	/// <summary>
	/// Gets or sets the distance in the Z direction.
	/// </summary>
	[DxfCodeValue(142)]
	public double DistanceZ { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLinearGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLinearGrip;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockLinearGrip,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockLinearGrip,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}