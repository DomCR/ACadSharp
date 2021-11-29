using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
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
		[DxfCodeValue(40)]
		public double Height { get; set; } = 0.0;

		/// <summary>
		///
		/// </summary>
		[DxfCodeValue(DxfCode.TxtStyleXScale)]
		public double RectangleWitdth { get; set; }

		[DxfCodeValue(46)]
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
		public TextStyle Style { get; set; } = TextStyle.Default;

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

		#endregion Create a class column data ?

		public bool IsAnnotative { get; set; }

		public MText() : base()
		{
		}

		internal MText(DxfEntityTemplate template) : base(template)
		{
		}
	}
}