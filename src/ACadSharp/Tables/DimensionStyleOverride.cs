using System;
using System.Collections.Generic;
using ACadSharp.Types.Units;

namespace ACadSharp.Tables;

public sealed class DimensionStyleOverride
{
    public DimensionStyleOverrideType Type { get; }
    public object Value { get; }

    public DimensionStyleOverride(DimensionStyleOverrideType type, object value)
    {
        if (!ExpectedTypes.TryGetValue(type, out var expectedType))
            throw new ArgumentException($"No type mapping defined for {type}.");

        if (value == null)
            throw new ArgumentNullException(nameof(value), $"Value for {type} cannot be null.");

        var actualType = value.GetType();

        if (!expectedType.IsAssignableFrom(actualType))
        {
            throw new ArgumentException(
                $"Invalid value type for {type}. Expected {expectedType.Name}, got {actualType.Name}.");
        }

        Type = type;
        Value = value;
    }
    
    private static readonly Dictionary<DimensionStyleOverrideType, Type> ExpectedTypes =
        new()
        {
            // TEXT & APPEARANCE
            { DimensionStyleOverrideType.DimensionTextStyle, typeof(TextStyle) },
            { DimensionStyleOverrideType.DimensionTextHeight, typeof(double) },
            { DimensionStyleOverrideType.DimensionLineGap, typeof(double) },
            { DimensionStyleOverrideType.TextVerticalPlacement, typeof(DimensionTextVerticalAlignment) },
            { DimensionStyleOverrideType.TextInsideHorizontal, typeof(bool) },
            { DimensionStyleOverrideType.TextOutsideHorizontal, typeof(bool) },
            { DimensionStyleOverrideType.ForceTextInside, typeof(bool) },
            { DimensionStyleOverrideType.TextOutsideExtensions, typeof(bool) },
            { DimensionStyleOverrideType.TextVerticalPosition, typeof(double) },
            { DimensionStyleOverrideType.TextJustification, typeof(DimensionTextHorizontalAlignment) },
            { DimensionStyleOverrideType.PostFix, typeof(string) },
            { DimensionStyleOverrideType.AlternateDimensioningSuffix, typeof(string) },

            // COLORS
            { DimensionStyleOverrideType.DimensionLineColor, typeof(Color) },
            { DimensionStyleOverrideType.ExtensionLineColor, typeof(Color) },
            { DimensionStyleOverrideType.DimensionTextColor, typeof(Color) },

            // LINETYPES & WEIGHTS
            { DimensionStyleOverrideType.DimensionLineLineType, typeof(LineType) },
            { DimensionStyleOverrideType.ExtensionLine1LineType, typeof(LineType) },
            { DimensionStyleOverrideType.ExtensionLine2LineType, typeof(LineType) },
            { DimensionStyleOverrideType.DimensionLineWeight, typeof(LineWeightType) },
            { DimensionStyleOverrideType.ExtensionLineWeight, typeof(LineWeightType) },

            // DIM / EXT LINE VALUES
            { DimensionStyleOverrideType.DimensionLineExtension, typeof(double) },
            { DimensionStyleOverrideType.DimensionLineIncrement, typeof(double) },
            { DimensionStyleOverrideType.ExtensionLineExtension, typeof(double) },
            { DimensionStyleOverrideType.ExtensionLineOffset, typeof(double) },
            { DimensionStyleOverrideType.SuppressFirstDimensionLine, typeof(bool) },
            { DimensionStyleOverrideType.SuppressSecondDimensionLine, typeof(bool) },
            { DimensionStyleOverrideType.SuppressDimensionExtension1, typeof(bool) },
            { DimensionStyleOverrideType.SuppressDimensionExtension2, typeof(bool) },
            { DimensionStyleOverrideType.ExtensionLineFixedLength, typeof(double) },
            { DimensionStyleOverrideType.ExtensionLineFixedLengthEnabled, typeof(bool) },
            { DimensionStyleOverrideType.SuppressOutsideExtensions, typeof(bool) },

            // ARROWS
            { DimensionStyleOverrideType.ArrowSize, typeof(double) },
            { DimensionStyleOverrideType.TickSize, typeof(double) },
            { DimensionStyleOverrideType.CenterMarkSize, typeof(double) },
            { DimensionStyleOverrideType.UseCustomArrowBlocks, typeof(bool) },
            { DimensionStyleOverrideType.ArrowBlock, typeof(BlockRecord) },
            { DimensionStyleOverrideType.ArrowBlock1, typeof(BlockRecord) },
            { DimensionStyleOverrideType.ArrowBlock2, typeof(BlockRecord) },
            { DimensionStyleOverrideType.LeaderArrowBlock, typeof(BlockRecord) },
            { DimensionStyleOverrideType.ArcLengthSymbolPosition, typeof(ArcLengthSymbolPosition) },

            // UNITS
            { DimensionStyleOverrideType.LinearUnitFormat, typeof(LinearUnitFormat) },
            { DimensionStyleOverrideType.DimensionScaleFactor, typeof(Double) },
            { DimensionStyleOverrideType.LinearDecimalPrecision, typeof(short) },
            { DimensionStyleOverrideType.DecimalSeparator, typeof(Char) },
            { DimensionStyleOverrideType.AngularDecimalPlaces, typeof(short) },
            { DimensionStyleOverrideType.AngularUnitFormat, typeof(AngularUnitFormat) },
            { DimensionStyleOverrideType.ZeroSuppression, typeof(ZeroHandling) },
            { DimensionStyleOverrideType.AngularZeroHandling, typeof(ZeroHandling) },
            { DimensionStyleOverrideType.EnableAlternateUnits, typeof(bool) },
            { DimensionStyleOverrideType.AlternateUnitScaleFactor, typeof(double) },
            { DimensionStyleOverrideType.AlternateUnitRounding, typeof(double) },
            { DimensionStyleOverrideType.AlternateUnitToleranceDecimals, typeof(short) },
            { DimensionStyleOverrideType.AlternateUnitDecimalPlaces, typeof(short) },
            { DimensionStyleOverrideType.AlternateUnitToleranceZeroSuppression, typeof(ZeroHandling) },
            { DimensionStyleOverrideType.AlternateUnitFormat, typeof(LinearUnitFormat) },

            // FIT
            { DimensionStyleOverrideType.FitOptions, typeof(short) },
            { DimensionStyleOverrideType.LinearScaleFactor, typeof(double) },
            { DimensionStyleOverrideType.ToleranceTextScale, typeof(double) },
            { DimensionStyleOverrideType.TextMovementRule, typeof(TextMovement) },
            { DimensionStyleOverrideType.CursorUpdate, typeof(bool) },
            { DimensionStyleOverrideType.DimensionTextArrowFit, typeof(TextArrowFitType) },

            // TOLERANCES
            { DimensionStyleOverrideType.EnableTolerances, typeof(bool) },
            { DimensionStyleOverrideType.EnableLimitsTolerances, typeof(bool) },
            { DimensionStyleOverrideType.TolerancePlus, typeof(double) },
            { DimensionStyleOverrideType.ToleranceMinus, typeof(double) },
            { DimensionStyleOverrideType.ToleranceJustification, typeof(ToleranceAlignment) },
            { DimensionStyleOverrideType.ToleranceZeroSuppression, typeof(ZeroHandling) },
            { DimensionStyleOverrideType.ToleranceZeroSuppressionAlternate, typeof(ZeroHandling) },
            { DimensionStyleOverrideType.ToleranceDecimalPlaces, typeof(short) },

            // ADDITIONAL UNITS / TEXT OPTIONS
            { DimensionStyleOverrideType.Rounding, typeof(double) },
            { DimensionStyleOverrideType.FractionFormat, typeof(FractionFormat) },
            { DimensionStyleOverrideType.TextBackgroundFillMode, typeof(DimensionTextBackgroundFillMode) },
            { DimensionStyleOverrideType.TextDirection, typeof(TextDirection) },
            { DimensionStyleOverrideType.JoggedRadiusDimensionTransverseSegmentAngle, typeof(double) }
        };
}