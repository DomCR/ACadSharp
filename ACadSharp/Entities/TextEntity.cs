using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Fields for the subclass marker AcDbText.
	/// </summary>
	public abstract class TextEntity : Entity
	{
		//100	Subclass marker(AcDbText)
		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(DxfCode.Thickness)]
		public double Thickness { get; set; } = 0.0;
		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		/// <remarks>
		/// This property is read-only except for text whose Alignment property is set to acAlignmentLeft, acAlignmentAligned, or acAlignmentFit. To position text whose justification is other than left, aligned, or fit, use the TextAlignmentPoint property.
		/// </remarks>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;
		/// <summary>
		/// Changes the height of the object.
		/// </summary>
		/// <value>
		/// This must be a positive, non-negative number.
		/// </value>
		[DxfCodeValue(DxfCode.TxtSize)]
		public double Height { get; set; } = 0.0;
		/// <summary>
		/// Specifies the text string for the entity.
		/// </summary>
		/// <value>
		/// The maximum length is 256 characters.
		/// </value>
		[DxfCodeValue(DxfCode.Text)]
		public string Value { get; set; } = string.Empty;
		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfCode.Angle)]
		public double Rotation { get; set; } = 0.0;

		//41	Relative X scale factor—width(optional; default = 1)	//Needed?? adjust value
		//		This value is also adjusted when fit-type text is used
		[DxfCodeValue(DxfCode.XScaleFactor)]
		public double WidthFactor { get; set; } = 1.0;

		/// <summary>
		/// Specifies the oblique angle of the object.
		/// </summary>
		/// <value>
		/// The angle in radians within the range of -85 to +85 degrees. A positive angle denotes a lean to the right; a negative value will have 2*PI added to it to convert it to its positive equivalent.
		/// </value>
		[DxfCodeValue(DxfCode.ObliqueAngle)]
		public double ObliqueAngle { get; set; } = 0.0;
		/// <summary>
		/// Style of this text entity.
		/// </summary>
		[DxfCodeValue(DxfCode.TextStyleName)]
		public Style TextStyle { get; set; } = Style.Default;
		/// <summary>
		/// Mirror flags.
		/// </summary>
		[DxfCodeValue(DxfCode.TxtMirrorFlags)]
		public TextMirrorFlag Mirror { get; set; } = TextMirrorFlag.None;
		/// <summary>
		/// Horizontal text justification type.
		/// </summary>
		[DxfCodeValue(DxfCode.HorizontalTextAlignment)]
		public TextHorizontalAlignment HorizontalAlignment { get; set; } = TextHorizontalAlignment.Left;
		/// <summary>
		/// A 3D WCS coordinate representing the alignment point of the object.
		/// </summary>
		/// <remarks>
		/// This property will be reset to 0, 0, 0 and will become read-only when the Alignment property is set to acAlignmentLeft. To position text whose justification is left, fit, or aligned, use the InsertionPoint property.
		/// </remarks>
		[DxfCodeValue(DxfCode.XCoordinate1, DxfCode.YCoordinate1, DxfCode.ZCoordinate1)]
		public XYZ AlignmentPoint { get; set; } = XYZ.Zero;
		/// <summary>
		/// Vertical text justification type.
		/// </summary>
		[DxfCodeValue(DxfCode.VerticalTextAlignment)]
		public virtual TextVerticalAlignment VerticalAlignment { get; set; } = TextVerticalAlignment.Baseline;

		public TextEntity() : base() { }

		internal TextEntity(DxfEntityTemplate template) : base(template) { }
	}
}
