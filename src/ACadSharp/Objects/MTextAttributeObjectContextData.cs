using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Represents the context data for the multiline text of an <see cref="AttributeEntity"/> in a specific annotation scale.
/// </summary>
[DxfName(DxfFileToken.MTextAttributeObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AnnotScaleObjectContextData)]
public class MTextAttributeObjectContextData : AnnotScaleObjectContextData, IDxfClassDefined
{
	/// <summary>
	/// Alignment point of the text.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ AlignmentPoint { get; set; } = XYZ.AxisX;

	/// <summary>
	/// Attachment point that defines how the text is aligned relative to the <see cref="InsertPoint"/>.
	/// </summary>
	[DxfCodeValue(70)]
	public AttachmentPointType AttachmentPoint { get; set; } = AttachmentPointType.TopLeft;

	/// <summary>
	/// Insert point of the text.
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ InsertPoint { get; set; } = XYZ.Zero;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.MTextAttributeObjectContextData;

	/// <summary>
	/// The rotation angle of the text, in radians.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
	public double Rotation { get; set; }

	/// <summary>
	/// Unknown flag stored in the DXF code 290.
	/// </summary>
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