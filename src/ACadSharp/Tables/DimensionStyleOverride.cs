using System;
using System.Collections.Generic;

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
            { DimensionStyleOverrideType.TextVerticalPlacement, typeof(DimensionTextVerticalAlignment) },
            { DimensionStyleOverrideType.TextInsideHorizontal, typeof(bool) },
            { DimensionStyleOverrideType.TextOutsideHorizontal, typeof(bool) },
            { DimensionStyleOverrideType.ForceTextInside, typeof(bool) },
            { DimensionStyleOverrideType.ForceExtensionLinesOutside, typeof(bool) },
            { DimensionStyleOverrideType.TextVerticalPosition, typeof(double) },
            { DimensionStyleOverrideType.TextJustification, typeof(short) },
            { DimensionStyleOverrideType.DimensionTextFormat, typeof(string) },
            { DimensionStyleOverrideType.AltDimensionTextFormat, typeof(string) },

            // COLORS
            { DimensionStyleOverrideType.DimensionLineColor, typeof(Color) },
            { DimensionStyleOverrideType.ExtensionLineColor, typeof(Color) },
            { DimensionStyleOverrideType.DimensionTextColor, typeof(Color) },

            // LINETYPES & WEIGHTS
            { DimensionStyleOverrideType.DimensionLineLinetype, typeof(LineType) },
            { DimensionStyleOverrideType.ExtensionLine1Linetype, typeof(LineType) },
            { DimensionStyleOverrideType.ExtensionLine2Linetype, typeof(LineType) },
            { DimensionStyleOverrideType.DimensionLineWeight, typeof(short) },
            { DimensionStyleOverrideType.ExtensionLineWeight, typeof(short) },

            // DIM / EXT LINE VALUES
            { DimensionStyleOverrideType.DimensionLineExtension, typeof(double) },
            { DimensionStyleOverrideType.DimensionLineIncrement, typeof(double) },
            { DimensionStyleOverrideType.ExtensionLineExtension, typeof(double) },
            { DimensionStyleOverrideType.ExtensionLineOffset, typeof(double) },
            { DimensionStyleOverrideType.SuppressExtensionLine1, typeof(bool) },
            { DimensionStyleOverrideType.SuppressExtensionLine2, typeof(bool) },
            { DimensionStyleOverrideType.SuppressDimensionExtension1, typeof(bool) },
            { DimensionStyleOverrideType.SuppressDimensionExtension2, typeof(bool) },
            { DimensionStyleOverrideType.ExtensionLineFixedLength, typeof(double) },
            { DimensionStyleOverrideType.ExtensionLineFixedLengthEnabled, typeof(bool) },

            // ARROWS
            { DimensionStyleOverrideType.ArrowSize, typeof(double) },
            { DimensionStyleOverrideType.TickSize, typeof(double) },
            { DimensionStyleOverrideType.CenterMarkSize, typeof(double) },
            { DimensionStyleOverrideType.UseCustomArrowBlocks, typeof(bool) },
            { DimensionStyleOverrideType.ArrowBlock, typeof(string) },
            { DimensionStyleOverrideType.ArrowBlock1, typeof(string) },
            { DimensionStyleOverrideType.ArrowBlock2, typeof(string) },
            { DimensionStyleOverrideType.LeaderArrowBlock, typeof(string) },
            { DimensionStyleOverrideType.ArcLengthSymbolPlacement, typeof(short) },

            // UNITS
            { DimensionStyleOverrideType.LinearUnitFormat, typeof(short) },
            { DimensionStyleOverrideType.LinearDecimalPrecision, typeof(short) },
            { DimensionStyleOverrideType.DecimalSeparator, typeof(short) },
            { DimensionStyleOverrideType.AngularDecimalPrecision, typeof(short) },
            { DimensionStyleOverrideType.AngularUnitFormat, typeof(short) },
            { DimensionStyleOverrideType.ZeroSuppression, typeof(short) },
            { DimensionStyleOverrideType.AltUnitZeroSuppressionFactor, typeof(short) },
            { DimensionStyleOverrideType.EnableAlternateUnits, typeof(bool) },
            { DimensionStyleOverrideType.AlternateUnitScaleFactor, typeof(double) },
            { DimensionStyleOverrideType.AlternateUnitRounding, typeof(double) },
            { DimensionStyleOverrideType.AlternateUnitToleranceDecimals, typeof(short) },
            { DimensionStyleOverrideType.AlternateUnitDecimalPlaces, typeof(short) },
            { DimensionStyleOverrideType.AlternateUnitToleranceZeroSuppression, typeof(short) },

            // FIT
            { DimensionStyleOverrideType.FitOptions, typeof(short) },
            { DimensionStyleOverrideType.LinearScaleFactor, typeof(double) },
            { DimensionStyleOverrideType.ToleranceTextScale, typeof(double) },
            { DimensionStyleOverrideType.TextMovementRule, typeof(short) },
            { DimensionStyleOverrideType.UserPositionedText, typeof(bool) },

            // TOLERANCES
            { DimensionStyleOverrideType.EnableTolerances, typeof(bool) },
            { DimensionStyleOverrideType.EnableLimitsTolerances, typeof(bool) },
            { DimensionStyleOverrideType.TolerancePlus, typeof(double) },
            { DimensionStyleOverrideType.ToleranceMinus, typeof(double) },
            { DimensionStyleOverrideType.ToleranceJustification, typeof(short) },
            { DimensionStyleOverrideType.ToleranceZeroSuppression, typeof(short) },
            { DimensionStyleOverrideType.ToleranceZeroSuppressionAlternate, typeof(short) }
        };
}