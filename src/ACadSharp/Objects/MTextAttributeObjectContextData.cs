using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects;

[DxfName(DxfFileToken.MTextAttributeObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AnnotScaleObjectContextData)]
public class MTextAttributeObjectContextData : AnnotScaleObjectContextData, IDxfClassDefined
{
	[DxfCodeValue(11, 21, 31)]
	public XYZ AlignmentPoint { get; set; } = XYZ.AxisX;

	[DxfCodeValue(70)]
	public AttachmentPointType AttachmentPoint { get; set; } = AttachmentPointType.TopLeft;

	[DxfCodeValue(10, 20, 30)]
	public XYZ InsertPoint { get; set; } = XYZ.Zero;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.MTextAttributeObjectContextData;

	[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
	public double Rotation { get; set; }

	[DxfCodeValue(290)]
	public bool Value290 { get; set; }

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.MTextAttributeObjectContextData,
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.MTextAttributeObjectContextData,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}