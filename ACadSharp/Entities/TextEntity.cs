using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="TextEntity"/>
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityText"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Text"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityText)]
	[DxfSubClass(DxfSubclassMarker.Text)]
	public class TextEntity : Entity
	{
		public override ObjectType ObjectType => ObjectType.TEXT;

		public override string ObjectName => DxfFileToken.EntityText;

		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		/// <remarks>
		/// This property is read-only except for text whose Alignment property is set to acAlignmentLeft, acAlignmentAligned, or acAlignmentFit. To position text whose justification is other than left, aligned, or fit, use the TextAlignmentPoint property.
		/// </remarks>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Changes the height of the object.
		/// </summary>
		/// <value>
		/// This must be a positive, non-negative number.
		/// </value>
		[DxfCodeValue(40)]
		public double Height { get; set; } = 0.0;

		/// <summary>
		/// Specifies the text string for the entity.
		/// </summary>
		/// <value>
		/// The maximum length is 256 characters.
		/// </value>
		[DxfCodeValue(1)]
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(50)]
		public double Rotation { get; set; } = 0.0;

		/// <summary>
		/// Relative X scale factor—widt
		/// </summary>
		/// <remarks>
		/// This value is also adjusted when fit-type text is used (optional)
		/// </remarks>
		[DxfCodeValue(41)]
		public double WidthFactor { get; set; } = 1.0;

		/// <summary>
		/// Specifies the oblique angle of the object.
		/// </summary>
		/// <value>
		/// The angle in radians within the range of -85 to +85 degrees. A positive angle denotes a lean to the right; a negative value will have 2*PI added to it to convert it to its positive equivalent.
		/// </value>
		[DxfCodeValue(51)]
		public double ObliqueAngle { get; set; } = 0.0;

		/// <summary>
		/// Style of this text entity.
		/// </summary>
		[DxfCodeValue(7)]
		public TextStyle Style { get; set; } = TextStyle.Default;

		/// <summary>
		/// Mirror flags.
		/// </summary>
		[DxfCodeValue(71)]
		public TextMirrorFlag Mirror { get; set; } = TextMirrorFlag.None;

		/// <summary>
		/// Horizontal text justification type.
		/// </summary>
		[DxfCodeValue(72)]
		public TextHorizontalAlignment HorizontalAlignment { get; set; } = TextHorizontalAlignment.Left;

		/// <summary>
		/// A 3D WCS coordinate representing the alignment point of the object.
		/// </summary>
		/// <remarks>
		/// This property will be reset to 0, 0, 0 and will become read-only when the Alignment property is set to acAlignmentLeft. To position text whose justification is left, fit, or aligned, use the InsertionPoint property.
		/// </remarks>
		[DxfCodeValue(11, 21, 31)]
		public XYZ AlignmentPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Vertical text justification type.
		/// </summary>
		[DxfCodeValue(73)]
		public virtual TextVerticalAlignment VerticalAlignment { get; set; } = TextVerticalAlignment.Baseline;

		public TextEntity() : base() { }
	}
}
