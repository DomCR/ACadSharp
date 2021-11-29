using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{

	/// <summary>
	/// Represents the text direction (left-to-right or right-to-left).
	/// </summary>
	public enum TextDirection : byte
	{
		/// <summary>Display text left-to-right.</summary>
		LeftToRight,
		/// <summary>Display text right-to-left.</summary>
		RightToLeft,
	}

	/// <summary>
	/// Tolerance alignment.
	/// </summary>
	public enum ToleranceAlignment : byte
	{
		/// <summary>
		/// Aligns the tolerance text with the bottom of the main dimension text.
		/// </summary>
		Bottom = 0,

		/// <summary>
		/// Aligns the tolerance text with the middle of the main dimension text.
		/// </summary>
		Middle = 1,

		/// <summary>
		/// Aligns the tolerance text with the top of the main dimension text.
		/// </summary>
		Top = 2
	}

	/// <summary>
	/// Text movement rules.
	/// </summary>
	public enum TextMovement : short
	{
		/// <summary>
		/// Moves the dimension line with dimension text.
		/// </summary>
		MoveLineWithText,
		/// <summary>
		/// Adds a leader when dimension text is moved.
		/// </summary>
		AddLeaderWhenTextMoved,
		/// <summary>
		/// Allows text to be moved freely without a leader.
		/// </summary>
		FreeTextPosition,
	}

	/// <summary>
	/// Represents the fraction format when the linear unit format is set to architectural or fractional.
	/// </summary>
	public enum FractionFormat : short
	{
		/// <summary>
		/// Horizontal stacking.
		/// </summary>
		Horizontal,
		/// <summary>
		/// Diagonal stacking.
		/// </summary>
		Diagonal,
		/// <summary>
		/// Not stacked.
		/// </summary>
		None,
	}

	/// <summary>
	/// Controls the arc length symbol position in an arc length dimension.
	/// </summary>
	public enum ArcLengthSymbolPosition : short
	{
		/// <summary>
		/// Before the dimension text (default).
		/// </summary>
		BeforeDimensionText,
		/// <summary>
		/// Above the dimension text.
		/// </summary>
		AboveDimensionText,
		/// <summary>
		/// Don't display the arc length symbol.
		/// </summary>
		None,
	}

	/// <summary>
	/// Represents the dimension text background color.
	/// </summary>
	public enum DimensionTextBackgroundFillMode : short
	{
		/// <summary>
		/// No background color is used.
		/// </summary>
		NoBackground,
		/// <summary>
		/// In this mode the drawing background color is used.
		/// </summary>
		DrawingBackgroundColor,
		/// <summary>
		/// This mode is used as the dimension text background.
		/// </summary>
		DimensionTextBackgroundColor,
	}

	/// <summary>
	/// Represents supression of zeros in displaying decimal numbers.
	/// </summary>
	public enum ZeroHandling : byte
	{
		/// <summary>
		/// Suppress zero feet and exactly zero inches.
		/// </summary>
		SuppressZeroFeetAndInches = 0,
		/// <summary>
		/// Show zero feet and exactly zero inches.
		/// </summary>
		ShowZeroFeetAndInches = 1,
		/// <summary>
		/// Show zero feet and suppress zero inches.
		/// </summary>
		ShowZeroFeetSuppressZeroInches = 2,
		/// <summary>
		/// Suppress zero feet and show zero inches.
		/// </summary>
		SuppressZeroFeetShowZeroInches = 3,
		/// <summary>
		/// Suppress leading zeroes in decimal numbers.
		/// </summary>
		SuppressDecimalLeadingZeroes = 4,
		/// <summary>
		/// Suppress trailing zeroes in decimal numbers.
		/// </summary>
		SuppressDecimalTrailingZeroes = 8,
		/// <summary>
		/// Suppress both leading and trailing zeroes in decimal numbers
		/// </summary>
		SuppressDecimalLeadingAndTrailingZeroes = 12,
	}

	/// <summary>
	/// Controls the vertical placement of dimension text in relation to the dimension line.
	/// </summary>
	public enum DimensionTextHorizontalAlignment : byte
	{
		/// <summary>
		/// Centers the dimension text along the dimension line between the extension lines.
		/// </summary>
		Centered = 0,

		/// <summary>
		/// Left-justifies the text with the first extension line along the dimension line.
		/// </summary>
		Left = 1,

		/// <summary>
		/// Right-justifies the text with the second extension line along the dimension line.
		/// </summary>
		Right = 2,

		/// <summary>
		/// Positions the text over or along the first extension line.
		/// </summary>
		OverFirstExtLine = 3,

		/// <summary>
		/// Positions the text over or along the second extension line.
		/// </summary>
		OverSecondExtLine = 4
	}

	/// <summary>
	/// Controls the placement of dimension text.
	/// </summary>
	public enum DimensionTextVerticalAlignment
	{
		/// <summary>
		/// Centers the dimension text between the two parts of the dimension line.
		/// </summary>
		Centered = 0,

		/// <summary>
		/// Places the dimension text above the dimension line.
		/// </summary>
		Above = 1,

		/// <summary>
		/// Places the dimension text on the side of the dimension line farthest away from the first defining point.
		/// </summary>
		Outside = 2,

		/// <summary>
		/// Places the dimension text to conform to a Japanese Industrial Standards (JIS) representation.
		/// </summary>
		JIS = 3,

		/// <summary>
		/// Places the dimension text under the dimension line.
		/// </summary>
		Below = 4
	}

	public class DimensionStyle : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.DIMSTYLE;

		public override string ObjectName => DxfFileToken.TableDimstyle;

		public string PostFix { get; set; }
		public string AlternateDimensioningSuffix { get; set; }

		[DxfCodeValue(71)]
		public bool GenerateTolerances { get; set; }
		public bool LimitsGeneration { get; set; }
		public bool TextInsideHorizontal { get; set; }
		public bool TextOutsideHorizontal { get; set; }
		public bool SuppressFirstExtensionLine { get; set; }
		public bool SuppressSecondExtensionnLine { get; set; }
		public bool AlternateUnitDimensioning { get; set; }
		public bool TextOutsideExtensions { get; set; }
		public bool SeparateArrowBlocks { get; set; }
		public bool TextInsideExtensions { get; set; }
		public bool SuppressOutsideExtensions { get; set; }
		public short AlternateUnitDecimalPlaces { get; set; }
		public ZeroHandling ZeroHandling { get; set; }
		public bool SuppressFirstDimensionLine { get; set; }
		public bool SuppressSecondDimensionLine { get; set; }
		public ToleranceAlignment ToleranceAlignment { get; set; }
		public DimensionTextHorizontalAlignment TextHorizontalAlignment { get; set; }
		public char DimensionFit { get; set; }
		public bool CursorUpdate { get; set; }
		public ZeroHandling ToleranceZeroHandling { get; set; }
		public ZeroHandling AlternateUnitZeroHandling { get; set; }
		public short AngularDimensionDecimalPlaces { get; set; }
		public ZeroHandling AlternateUnitToleranceZeroHandling { get; set; }
		public DimensionTextVerticalAlignment TextVerticalAlignment { get; set; }
		public short DimensionUnit { get; set; }
		public AngularUnitFormat AngularUnit { get; set; }
		public short DecimalPlaces { get; set; }
		public short ToleranceDecimalPlaces { get; set; }
		public LinearUnitFormat AlternateUnitFormat { get; set; }
		public short AlternateUnitToleranceDecimalPlaces { get; set; }
		public double ScaleFactor { get; set; }
		public double ArrowSize { get; set; }
		public double ExtensionLineOffset { get; set; }
		public double DimensionLineIncrement { get; set; }
		public double ExtensionLineExtension { get; set; }
		public double Rounding { get; set; }
		public double DimensionLineExtension { get; set; }
		public double PlusTolerance { get; set; }
		public double MinusTolerance { get; set; }
		public double FixedExtensionLineLength { get; set; }
		public double JoggedRadiusDimensionTransverseSegmentAngle { get; set; }
		public DimensionTextBackgroundFillMode TextBackgroundFillMode { get; set; }
		public Color TextBackgroundColor { get; set; }
		public bool SuppressSecondExtensionLine { get; set; }
		public ZeroHandling AngularZeroHandling { get; set; }
		public ArcLengthSymbolPosition ArcLengthSymbolPosition { get; set; }
		public double TextHeight { get; set; }
		public double CenterMarkSize { get; set; }
		public double TickSize { get; set; }
		public double AlternateUnitScaleFactor { get; set; }
		public double LinearScaleFactor { get; set; }
		public double TextVerticalPosition { get; set; }
		public double ToleranceScaleFactor { get; set; }
		public double DimensionLineGap { get; set; }
		public double AlternateUnitRounding { get; set; }
		public Color DimensionLineColor { get; set; }
		public Color ExtensionLineColor { get; set; }
		public Color TextColor { get; set; }
		public FractionFormat FractionFormat { get; set; }
		public LinearUnitFormat LinearUnitFormat { get; set; }
		public char DecimalSeparator { get; set; }
		public TextMovement TextMovement { get; set; }
		public bool IsExtensionLineLengthFixed { get; set; }
		public TextDirection TextDirection { get; set; }
		public double AltMzf { get; set; }
		public double Mzf { get; set; }
		public short DimensionLineWeight { get; set; }
		public short ExtensionLineWeight { get; set; }
		public string AltMzs { get; set; }
		public string Mzs { get; set; }
		public short DimensionTextArrowFit { get; set; }

		public DimensionStyle() : base() { }
		internal DimensionStyle(DxfEntryTemplate template) : base(template) { }
	}
}
