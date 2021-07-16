using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>Represents a background fill flags.</summary>
	[Flags]
	public enum BackgroundFillFlags : byte
	{
		/// <summary>None.</summary>
		None = 0,
		/// <summary>
		/// Use the background color.
		/// </summary>
		UseBackgroundFillColor = 1,
		/// <summary>
		/// Use the drawing window color.
		/// </summary>
		UseDrawingWindowColor = 2,
		/// <summary>
		/// Adds a text frame.
		/// Introduced in AutoCAD 2018.
		/// </summary>
		TextFrame = 16, // 0x10
	}

	/// <summary>
	/// Line spacing style for Multiline text and Dimensions.
	/// </summary>
	public enum LineSpacingStyleType : short
	{
		/// <summary>None.</summary>
		None,
		/// <summary>Taller characters will override.</summary>
		AtLeast,
		/// <summary>Taller characters will not override.</summary>
		Exact,
	}

	/// <summary>
	/// Attachment point for Multiline text.
	/// </summary>
	public enum AttachmentPointType : short
	{
		/// <summary>Top left.</summary>
		TopLeft = 1,
		/// <summary>Top center.</summary>
		TopCenter = 2,
		/// <summary>Top right.</summary>
		TopRight = 3,
		/// <summary>Middle left.</summary>
		MiddleLeft = 4,
		/// <summary>Middle center.</summary>
		MiddleCenter = 5,
		/// <summary>Middle right.</summary>
		MiddleRight = 6,
		/// <summary>Bottom left.</summary>
		BottomLeft = 7,
		/// <summary>Bottom center.</summary>
		BottomCenter = 8,
		/// <summary>Bottom right.</summary>
		BottomRight = 9,
	}

	/// <summary>
	/// Multiline text drawing direction.
	/// </summary>
	public enum DrawingDirectionType : short
	{
		/// <summary>Left to right.</summary>
		LeftToRight = 1,
		/// <summary>Right to left.</summary>
		RightToLeft = 2,
		/// <summary>Top to bottom.</summary>
		TopToBottom = 3,
		/// <summary>Bottom to top.</summary>
		BottomToTop = 4,
		/// <summary>By Style.</summary>
		ByStyle = 5,
	}

	/// <summary>
	/// Represents the type of columns.
	/// </summary>
	public enum ColumnType : short
	{
		NoColumns,
		StaticColumns,
		DynamicColumns,
	}

	public class MText : Entity
	{
		public override ObjectType ObjectType => ObjectType.MTEXT;
		public override string ObjectName => DxfFileToken.EntityMText;

		//100	Subclass marker(AcDbMText)

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
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.TxtStyleXScale)]
		public double RectangleWitdth { get; set; }
		[DxfCodeValue(DxfCode.TxtStyleYScale)]
		public double RectangleHeight { get; set; }
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.TxtStyleFlags)]
		public AttachmentPointType AttachmentPoint { get; set; }
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.HorizontalTextAlignment)]
		public DrawingDirectionType DrawingDirection { get; set; }
		/// <summary>
		/// Specifies the text string for the entity.
		/// </summary>
		/// <value>
		/// The maximum length is 256 characters.
		/// </value>
		[DxfCodeValue(DxfCode.Text)]
		public string Value { get; set; } = string.Empty;

		//3	Additional text(always in 250-character chunks) (optional)

		/// <summary>
		/// Style of this text entity.
		/// </summary>
		[DxfCodeValue(DxfCode.TextStyleName)]
		public Style TextStyle { get; set; } = Style.Default;
		/// <summary>
		/// A 3D WCS coordinate representing the alignment point of the object.
		/// </summary>
		/// <remarks>
		/// This property will be reset to 0, 0, 0 and will become read-only when the Alignment property is set to acAlignmentLeft. To position text whose justification is left, fit, or aligned, use the InsertionPoint property.
		/// </remarks>
		[DxfCodeValue(DxfCode.XCoordinate1, DxfCode.YCoordinate1, DxfCode.ZCoordinate1)]
		public XYZ AlignmentPoint { get; set; } = XYZ.Zero;

		//42 Horizontal width of the characters that make up the mtext entity.This value will always be equal to or less than the value of group code 41 (read-only, ignored if supplied)

		//43 Vertical height of the mtext entity(read-only, ignored if supplied)

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfCode.Angle)]
		public double Rotation { get; set; } = 0.0;
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.VerticalTextAlignment)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Percentage of default (3-on-5) line spacing to be applied.Valid values range from 0.25 to 4.00
		/// </remarks>
		[DxfCodeValue(DxfCode.ShapeXOffset)]
		public double LineSpacing { get; set; }
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.Int32)]
		public BackgroundFillFlags BackgroundFillFlags { get; set; }
		/// <summary>
		/// Determines how much border there is around the text.
		/// </summary>
		[DxfCodeValue(DxfCode.BackgroundScale)]
		public double BackgroundScale { get; set; } = 1.5;
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Color to use for background fill when group code 90 is 1.
		/// </remarks>
		[DxfCodeValue(DxfCode.BackgroundColor)]
		public Color BackgroundColor { get; set; }
		//420 - 429	Background color(if RGB color)
		//430 - 439	Background color(if color name)
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.BackgroundTransparency)]
		public Transparency BackgroundTransparency { get; set; }

		#region Create a class column data ?
		/// <summary>
		/// Text column type.
		/// </summary>
		[DxfCodeValue(75)]
		public ColumnType ColumnType { get; set; }

		/// <summary>
		/// Number of columns.
		/// </summary>
		[DxfCodeValue(76)]
		public int ColumnCount { get { return this.ColumnHeights.Count; } }

		/// <summary>
		/// Gets or sets a value indicating whether flow is reversed.
		/// </summary>
		[DxfCodeValue(78)]
		public bool ColumnFlowReversed { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether height is automatic.
		/// </summary>
		[DxfCodeValue(79)]
		public bool ColumnAutoHeight { get; set; }

		/// <summary>
		/// Width of the column.
		/// </summary>
		[DxfCodeValue(48)]
		public double ColumnWidth { get; set; }

		/// <summary>
		/// Column gutter.
		/// </summary>
		[DxfCodeValue(49)]
		public double ColumnGutter { get; set; }

		/// <summary>
		/// Column heights.
		/// </summary>
		[DxfCodeValue(50)]
		public List<double> ColumnHeights { get; } = new List<double>();
		#endregion

		public bool IsAnnotative { get; set; }

		public MText() : base() { }

		internal MText(DxfEntityTemplate template) : base(template) { }
	}
}
