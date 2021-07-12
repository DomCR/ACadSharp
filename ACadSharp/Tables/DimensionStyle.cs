using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class DimensionStyle : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.DIMSTYLE;

		public string PostFix { get; internal set; }
		public string AlternateDimensioningSuffix { get; internal set; }
		public bool GenerateTolerances { get; internal set; }
		public bool LimitsGeneration { get; internal set; }
		public bool TextInsideHorizontal { get; internal set; }
		public bool TextOutsideHorizontal { get; internal set; }
		public bool SuppressFirstExtensionLine { get; internal set; }
		public bool SuppressSecondExtensionnLine { get; internal set; }
		public bool AlternateUnitDimensioning { get; internal set; }
		public bool TextOutsideExtensions { get; internal set; }
		public bool SeparateArrowBlocks { get; internal set; }
		public bool TextInsideExtensions { get; internal set; }
		public bool SuppressOutsideExtensions { get; internal set; }
		public char AlternateUnitDecimalPlaces { get; internal set; }
		public char ZeroHandling { get; internal set; }
		public bool SuppressFirstDimensionLine { get; internal set; }
		public bool SuppressSecondDimensionLine { get; internal set; }
		public char ToleranceAlignment { get; internal set; }
		public char TextHorizontalAlignment { get; internal set; }
		public char DimensionFit { get; internal set; }
		public bool CursorUpdate { get; internal set; }
		public char ToleranceZeroHandling { get; internal set; }
		public char AlternateUnitZeroHandling { get; internal set; }
		public short AngularDimensionDecimalPlaces { get; internal set; }
		public char AlternateUnitToleranceZeroHandling { get; internal set; }
		public char TextVerticalAlignment { get; internal set; }
		public short DimensionUnit { get; internal set; }
		public short DecimalPlaces { get; internal set; }
		public short ToleranceDecimalPlaces { get; internal set; }
		public short AlternateUnitFormat { get; internal set; }
		public short AlternateUnitToleranceDecimalPlaces { get; internal set; }
		public double ScaleFactor { get; internal set; }
		public double ArrowSize { get; internal set; }
		public double ExtensionLineOffset { get; internal set; }
		public double DimensionLineIncrement { get; internal set; }
		public double ExtensionLineExtension { get; internal set; }
		public double Rounding { get; internal set; }
		public double DimensionLineExtension { get; internal set; }
		public double PlusTolerance { get; internal set; }
		public double MinusTolerance { get; internal set; }
		public double FixedExtensionLineLength { get; internal set; }
		public double JoggedRadiusDimensionTransverseSegmentAngle { get; internal set; }
		public short TextBackgroundFillMode { get; internal set; }
		public Color TextBackgroundColor { get; internal set; }
		public bool SuppressSecondExtensionLine { get; internal set; }
		public short AngularZeroHandling { get; internal set; }
		public short ArcLengthSymbolPosition { get; internal set; }
		public double TextHeight { get; internal set; }
		public double CenterMarkSize { get; internal set; }
		public double TickSize { get; internal set; }
		public double AlternateUnitScaleFactor { get; internal set; }
		public double LinearScaleFactor { get; internal set; }
		public double TextVerticalPosition { get; internal set; }
		public double ToleranceScaleFactor { get; internal set; }
		public double DimensionLineGap { get; internal set; }
		public double AlternateUnitRounding { get; internal set; }
		public Color DimensionLineColor { get; internal set; }
		public Color ExtensionLineColor { get; internal set; }
		public Color TextColor { get; internal set; }
		public short FractionFormat { get; internal set; }
		public short LinearUnitFormat { get; internal set; }
		public char DecimalSeparator { get; internal set; }
		public short TextMovement { get; internal set; }
		public bool IsExtensionLineLengthFixed { get; internal set; }
		public bool TextDirection { get; internal set; }
		public double AltMzf { get; internal set; }
		public double Mzf { get; internal set; }
		public short DimensionLineWeight { get; internal set; }
		public short ExtensionLineWeight { get; internal set; }
		public string AltMzs { get; internal set; }
		public string Mzs { get; internal set; }
		public short DimensionTextArrowFit { get; internal set; }

		public DimensionStyle() : base() { }
		internal DimensionStyle(DxfEntryTemplate template) : base(template) { }
	}
}
