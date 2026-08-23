using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKXYPARAMETER object, used in AutoCAD to control a
/// x y in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockXYParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockXYParameter"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockXYParameter)]
[DxfSubClass(DxfSubclassMarker.BlockXYParameter)]
public class BlockXYParameter : Block2PtParameter, IDxfClassDefined
{
	[DxfCodeValue(308)]
	public string DescriptionX { get; set; }

	[DxfCodeValue(307)]
	public string DescriptionY { get; set; }

	[DxfCodeValue(141)]
	public double LabelOffsetX { get; set; }

	[DxfCodeValue(140)]
	public double LabelOffsetY { get; set; }

	[DxfCodeValue(306)]
	public string LabelX { get; set; }

	[DxfCodeValue(305)]
	public string LabelY { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockXYParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockXYParameter;

	public ParameterValueSet ValueSetX { get; set; }

	public ParameterValueSet ValueSetY { get; set; }

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockXYParameter,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockXYParameter,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}