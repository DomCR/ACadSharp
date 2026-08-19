using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKPOLARPARAMETER object, used in AutoCAD to control a
/// polar parameter in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockPolarParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockPolarParameter"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockPolarParameter)]
[DxfSubClass(DxfSubclassMarker.BlockPolarParameter)]
public class BlockPolarParameter : Block2PtParameter, IDxfClassDefined
{
	/// <summary>
	/// Gets or sets the description of the angle.
	/// </summary>
	[DxfCodeValue(308)]
	public string AngleDescription { get; set; }

	/// <summary>
	/// Gets or sets the name of the angle.
	/// </summary>
	[DxfCodeValue(307)]
	public string AngleName { get; set; }

	/// <summary>
	/// Gets or sets the angle of the polar parameter.
	/// </summary>
	public ParameterValueSet AngleValueSet { get; set; }

	/// <summary>
	/// Gets or sets the description of the polar parameter.
	/// </summary>
	[DxfCodeValue(306)]
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the distance of the polar parameter.
	/// </summary>
	public ParameterValueSet DistanceValueSet { get; set; }

	/// <summary>
	/// Gets or sets the label text of the polar parameter.
	/// </summary>
	[DxfCodeValue(305)]
	public string Label { get; set; }

	/// <summary>
	/// Gets or sets the position of the label text of the polar parameter.
	/// </summary>
	[DxfCodeValue(140)]
	public double LabelOffset { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockPolarParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockPolarParameter;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockPolarParameter,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockPolarParameter,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}