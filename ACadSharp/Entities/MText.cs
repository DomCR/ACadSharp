using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="MText"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMText"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MText"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMText)]
	[DxfSubClass(DxfSubclassMarker.MText)]
	public partial class MText : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.MTEXT;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMText;

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		/// <remarks>
		/// This property is read-only except for text whose Alignment property is set to acAlignmentLeft, acAlignmentAligned, or acAlignmentFit. To position text whose justification is other than left, aligned, or fit, use the TextAlignmentPoint property.
		/// </remarks>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Changes the height of the object.
		/// </summary>
		/// <value>
		/// This must be a positive, non-negative number.
		/// </value>
		[DxfCodeValue(40)]
		public double Height { get; set; } = 0.0;

		/// <summary>
		/// Reference rectangle width
		/// </summary>
		[DxfCodeValue(41)]
		public double RectangleWitdth { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(46)]
		public double RectangleHeight { get; set; }

		/// <summary>
		/// Attachment point
		/// </summary>
		[DxfCodeValue(71)]
		public AttachmentPointType AttachmentPoint { get; set; }

		/// <summary>
		/// Drawing direction
		/// </summary>
		[DxfCodeValue(72)]
		public DrawingDirectionType DrawingDirection { get; set; }

		/// <summary>
		/// Specifies the text string for the entity.
		/// </summary>
		/// <value>
		/// The maximum length is 256 characters.
		/// </value>
		[DxfCodeValue(1)]
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// Additional text(always in 250-character chunks) (optional)
		/// </summary>
		[DxfCodeValue(3)]
		public string AdditionalText { get; set; } = string.Empty;

		/// <summary>
		/// Style of this text entity.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 7)]
		public TextStyle Style { get; set; } = TextStyle.Default;

		/// <summary>
		/// A 3D WCS coordinate representing the alignment point of the object.
		/// </summary>
		/// <remarks>
		/// This property will be reset to 0, 0, 0 and will become read-only when the Alignment property is set to acAlignmentLeft. To position text whose justification is left, fit, or aligned, use the InsertionPoint property.
		/// </remarks>
		[DxfCodeValue(11, 21, 31)]
		public XYZ AlignmentPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Horizontal width of the characters that make up the mtext entity.
		/// This value will always be equal to or less than the value of group code 41 
		/// </summary>
		/// <remarks>
		/// read-only, ignored if supplied
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Ignored, 42)]
		public double HorizontalWidth { get; set; }

		/// <summary>
		/// Horizontal width of the characters that make up the mtext entity.
		/// This value will always be equal to or less than the value of group code 41 
		/// </summary>
		/// <remarks>
		/// read-only, ignored if supplied
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Ignored, 43)]
		public double VerticalWidth { get; set; }

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(50)]
		public double Rotation { get; set; } = 0.0;

		/// <summary>
		/// Mtext line spacing style 
		/// </summary>
		[DxfCodeValue(73)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }

		/// <summary>
		/// Mtext line spacing factor 
		/// </summary>
		/// <remarks>
		/// Percentage of default (3-on-5) line spacing to be applied.Valid values range from 0.25 to 4.00
		/// </remarks>
		[DxfCodeValue(44)]
		public double LineSpacing { get; set; }

		/// <summary>
		/// Background fill setting
		/// </summary>
		[DxfCodeValue(90)]
		public BackgroundFillFlags BackgroundFillFlags { get; set; } = BackgroundFillFlags.None;

		/// <summary>
		/// Determines how much border there is around the text.
		/// </summary>
		[DxfCodeValue(45)]
		public double BackgroundScale { get; set; } = 1.5;

		/// <summary>
		/// Background fill color 
		/// </summary>
		/// <remarks>
		/// Color to use for background fill when group code 90 is 1.
		/// </remarks>
		[DxfCodeValue(63, 420, 430)]
		public Color BackgroundColor { get; set; }

		/// <summary>
		///
		/// </summary>
		[DxfCodeValue(441)]
		public Transparency BackgroundTransparency { get; set; }

		public bool IsAnnotative { get; set; }

		public TextColumn Column { get; set; } = new TextColumn();

		public MText() : base() { }
	}
}