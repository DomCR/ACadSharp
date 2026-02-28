using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects;

[DxfName(DxfFileToken.MTextAttributeObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AnnotScaleObjectContextData)]
public class MTextAttributeObjectContextData : AnnotScaleObjectContextData
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
}