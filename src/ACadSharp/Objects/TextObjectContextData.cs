using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Represents the context data of an annotative <see cref="TextEntity"/> for one annotation scale: where the text sits and
/// how it is turned and aligned at that scale.
/// </summary>
/// <remarks>
/// An annotative text keeps one of these per annotation scale in its extension dictionary
/// (<see cref="AnnotativeData.ContextDataManagerName"/> / <see cref="AnnotativeData.AnnotationScalesName"/>), see
/// <see cref="AnnotativeData"/>. The text's height at a scale is not stored here: it is the text's annotative (paper)
/// height times the scale.
/// </remarks>
[DxfName(DxfFileToken.TextObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AnnotScaleObjectContextData)]
public class TextObjectContextData : AnnotScaleObjectContextData, IDxfClassDefined
{
	/// <summary>
	/// Alignment point of the text at this scale (the text's second alignment point, in OCS).
	/// </summary>
	[DxfCodeValue(11, 21)]
	public XY AlignmentPoint { get; set; } = XY.Zero;

	/// <summary>
	/// Horizontal alignment of the text at this scale.
	/// </summary>
	[DxfCodeValue(70)]
	public TextHorizontalAlignment HorizontalAlignment { get; set; } = TextHorizontalAlignment.Left;

	/// <summary>
	/// Insertion point of the text at this scale (in OCS).
	/// </summary>
	[DxfCodeValue(10, 20)]
	public XY InsertPoint { get; set; } = XY.Zero;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TextObjectContextData;

	/// <summary>
	/// The rotation angle of the text at this scale, in radians.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
	public double Rotation { get; set; }

	/// <summary>
	/// Creates the context data of <paramref name="text"/> at <paramref name="scale"/>: its insertion and alignment points,
	/// rotation and horizontal alignment as they are.
	/// </summary>
	public static TextObjectContextData Create(TextEntity text, Scale scale)
	{
		return new TextObjectContextData
		{
			Scale = scale,
			InsertPoint = (XY)text.InsertPoint,
			AlignmentPoint = (XY)text.AlignmentPoint,
			Rotation = text.Rotation,
			HorizontalAlignment = text.HorizontalAlignment,
		};
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.TextObjectContextData,
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.TextObjectContextData,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}
