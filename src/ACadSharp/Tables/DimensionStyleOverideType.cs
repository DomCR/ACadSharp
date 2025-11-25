using ACadSharp.Attributes;

namespace ACadSharp.Tables;

using System.ComponentModel;

public enum DimensionStyleOverrideType
{
    /// <summary>
    /// Dimension text style.
    /// </summary>
    [DimOverrideXData(140, XDataValueKind.String, "DIMTXSTY")] 
    DimensionTextStyle,
    
    /// <summary>
    /// Dimension text height.
    /// </summary>
    [DimOverrideXData(140, XDataValueKind.Double, "DIMTXT")]
    DimensionTextHeight,

    /// <summary>
    /// Text vertical position relative to dimension line.
    /// </summary>
    [DimOverrideXData(77, XDataValueKind.DimensionTextVerticalAlignment, "DIMTAD")]
    TextVerticalPlacement,

    /// <summary>
    /// Text inside horizontal placement rule.
    /// </summary>
    [DimOverrideXData(73, XDataValueKind.Bool, "DIMTIH")]
    TextInsideHorizontal,

    /// <summary>
    /// Text outside horizontal placement rule.
    /// </summary>
    [DimOverrideXData(74, XDataValueKind.Bool, "DIMTOH")]
    TextOutsideHorizontal,

    /// <summary>
    /// Forces text inside extension lines when possible.
    /// </summary>
    [DimOverrideXData(174, XDataValueKind.Bool, "DIMTIX")]
    ForceTextInside,

    /// <summary>
    /// Forces extension lines if text must be moved outside.
    /// </summary>
    [DimOverrideXData(172, XDataValueKind.Short, "DIMTOFL")]
    ForceExtensionLinesOutside,

    /// <summary>
    /// Text vertical position for user-placed text.
    /// </summary>
    [DimOverrideXData(145, XDataValueKind.Double, "DIMTVP")]
    TextVerticalPosition,

    /// <summary>
    /// Text alignment/justification.
    /// </summary>
    [DimOverrideXData(280, XDataValueKind.DimensionTextHorizontalAlignment, "DIMJUST")]
    TextJustification,

    /// <summary>
    /// Formats prefix for dimension text.
    /// </summary>
    [DimOverrideXData(3, XDataValueKind.String, "DIMPOST")]
    DimensionTextPrefix,
    
    /// <summary>
    /// Formats prefix for dimension text.
    /// </summary>
    [DimOverrideXData(3, XDataValueKind.String, "DIMPOST")]
    PostFix,

    /// <summary>
    /// Formats prefix for alternate dimension text.
    /// </summary>
    [DimOverrideXData(4, XDataValueKind.String, "DIMAPOST")]
    AlternateDimensioningSuffix,


    // ---------------------------
    // COLORS
    // ---------------------------

    /// <summary>
    /// Color of dimension lines.
    /// </summary>
    [DimOverrideXData(176, XDataValueKind.Color, "DIMCLRD")]
    DimensionLineColor,

    /// <summary>
    /// Color of extension lines.
    /// </summary>
    [DimOverrideXData(177, XDataValueKind.Color, "DIMCLRE")]
    ExtensionLineColor,

    /// <summary>
    /// Color of dimension text.
    /// </summary>
    [DimOverrideXData(178, XDataValueKind.Color, "DIMCLRT")]
    DimensionTextColor,


    // ---------------------------
    // LINE WEIGHTS & TYPES
    // ---------------------------

    /// <summary>
    /// LineType for dimension lines.
    /// </summary>
    [DimOverrideXData(345, XDataValueKind.LineType, "DIMLTYPE")]
    DimensionLineLineType,

    /// <summary>
    /// LineType for first extension line.
    /// </summary>
    [DimOverrideXData(346, XDataValueKind.LineType, "DIMLTEX1")]
    ExtensionLine1LineType,

    /// <summary>
    /// LineType for second extension line.
    /// </summary>
    [DimOverrideXData(347, XDataValueKind.LineType, "DIMLTEX2")]
    ExtensionLine2LineType,

    /// <summary>
    /// Lineweight of dimension lines.
    /// </summary>
    [DimOverrideXData(371, XDataValueKind.LineWeightType, "DIMLWD")]
    DimensionLineWeight,

    /// <summary>
    /// Lineweight of extension lines.
    /// </summary>
    [DimOverrideXData(372, XDataValueKind.LineWeightType, "DIMLWE")]
    ExtensionLineWeight,


    // ---------------------------
    // DIMENSION / EXTENSION LINES
    // ---------------------------

    /// <summary>
    /// Extension of dimension lines beyond the extension lines.
    /// </summary>
    [DimOverrideXData(46, XDataValueKind.Double, "DIMDLE")]
    DimensionLineExtension,

    /// <summary>
    /// Incremental spacing of dimension lines.
    /// </summary>
    [DimOverrideXData(43, XDataValueKind.Double, "DIMDLI")]
    DimensionLineIncrement,

    /// <summary>
    /// Extension line extension beyond the dimension line.
    /// </summary>
    [DimOverrideXData(44, XDataValueKind.Double, "DIMEXE")]
    ExtensionLineExtension,

    /// <summary>
    /// Offset distance of extension lines from the measured points.
    /// </summary>
    [DimOverrideXData(42, XDataValueKind.Double, "DIMEXO")]
    ExtensionLineOffset,

    /// <summary>
    /// Suppress first extension line.
    /// </summary>
    [DimOverrideXData(75, XDataValueKind.Bool, "DIMSE1")]
    SuppressExtensionLine1,

    /// <summary>
    /// Suppress second extension line.
    /// </summary>
    [DimOverrideXData(76, XDataValueKind.Bool, "DIMSE2")]
    SuppressExtensionLine2,

    /// <summary>
    /// Suppress first dimension extension.
    /// </summary>
    [DimOverrideXData(281, XDataValueKind.Bool, "DIMSD1")]
    SuppressDimensionExtension1,

    /// <summary>
    /// Suppress second dimension extension.
    /// </summary>
    [DimOverrideXData(282, XDataValueKind.Bool, "DIMSD2")]
    SuppressDimensionExtension2,

    /// <summary>
    /// Fixed-length extension line setting.
    /// </summary>
    [DimOverrideXData(49, XDataValueKind.Double, "DIMFXL")]
    ExtensionLineFixedLength,

    /// <summary>
    /// Enables fixed extension line length.
    /// </summary>
    [DimOverrideXData(290, XDataValueKind.Bool, "DIMFXLON")]
    ExtensionLineFixedLengthEnabled,


    // ---------------------------
    // ARROWS / SYMBOLS
    // ---------------------------

    /// <summary>
    /// Size of dimension arrows.
    /// </summary>
    [DimOverrideXData(41, XDataValueKind.Double, "DIMASZ")]
    ArrowSize,

    /// <summary>
    /// Tick size for architectural ticks.
    /// </summary>
    [DimOverrideXData(142, XDataValueKind.Double, "DIMTSZ")]
    TickSize,

    /// <summary>
    /// Center mark size for arc/circle dimensions.
    /// </summary>
    [DimOverrideXData(141, XDataValueKind.Double, "DIMCEN")]
    CenterMarkSize,

    /// <summary>
    /// Uses custom arrow blocks.
    /// </summary>
    [DimOverrideXData(173, XDataValueKind.Bool, "DIMSAH")]
    UseCustomArrowBlocks,

    /// <summary>
    /// Arrow block for both ends.
    /// </summary>
    [DimOverrideXData(342, XDataValueKind.BlockRecord, "DIMBLK")]
    ArrowBlock,

    /// <summary>
    /// Arrow block for first end.
    /// </summary>
    [DimOverrideXData(343, XDataValueKind.BlockRecord, "DIMBLK1")]
    ArrowBlock1,

    /// <summary>
    /// Arrow block for second end.
    /// </summary>
    [DimOverrideXData(344, XDataValueKind.BlockRecord, "DIMBLK2")]
    ArrowBlock2,

    /// <summary>
    /// Leader arrow block.
    /// </summary>
    [DimOverrideXData(341, XDataValueKind.BlockRecord, "DIMLDRBLK")]
    LeaderArrowBlock,

    /// <summary>
    /// Arc length dimension symbol placement.
    /// </summary>
    [DimOverrideXData(144, XDataValueKind.Double, "DIMARCSYM")]
    ArcLengthSymbolPlacement,


    // ---------------------------
    // UNITS & FORMATTING
    // ---------------------------

    /// <summary>
    /// Linear unit display mode.
    /// </summary>
    [DimOverrideXData(277, XDataValueKind.LinearUnitFormat, "DIMLUNIT")]
    LinearUnitFormat,

    /// <summary>
    /// Number of decimal places for linear dimensions.
    /// </summary>
    [DimOverrideXData(271, XDataValueKind.Short, "DIMDEC")]
    LinearDecimalPrecision,

    /// <summary>
    /// Decimal separator character.
    /// </summary>
    [DimOverrideXData(278, XDataValueKind.Char, "DIMDSEP")]
    DecimalSeparator,

    /// <summary>
    /// Angular precision.
    /// </summary>
    [DimOverrideXData(270, XDataValueKind.Short, "DIMADEC")]
    AngularDecimalPrecision,

    /// <summary>
    /// Angular unit display format.
    /// </summary>
    [DimOverrideXData(179, XDataValueKind.Short, "DIMAUNIT")]
    AngularUnitFormat,

    /// <summary>
    /// Zero suppression for primary units.
    /// </summary>
    [DimOverrideXData(78, XDataValueKind.ZeroHandling, "DIMZIN")]
    ZeroSuppression,

    /// <summary>
    /// Zero suppression for alternative units.
    /// </summary>
    [DimOverrideXData(175, XDataValueKind.Bool, "DIMALTMZF")]
    AltUnitZeroSuppressionFactor,

    /// <summary>
    /// Enables alternate units.
    /// </summary>
    [DimOverrideXData(170, XDataValueKind.Bool, "DIMALT")]
    EnableAlternateUnits,

    /// <summary>
    /// Scale factor for alternate units.
    /// </summary>
    [DimOverrideXData(143, XDataValueKind.Double, "DIMALTF")]
    AlternateUnitScaleFactor,

    /// <summary>
    /// Alternate unit rounding value.
    /// </summary>
    [DimOverrideXData(148, XDataValueKind.Double, "DIMALTRND")]
    AlternateUnitRounding,

    /// <summary>
    /// Alternate unit tolerance precision.
    /// </summary>
    [DimOverrideXData(274, XDataValueKind.Short, "DIMALTTD")]
    AlternateUnitToleranceDecimals,

    /// <summary>
    /// Number of decimal places for alternate units.
    /// </summary>
    [DimOverrideXData(171, XDataValueKind.Short, "DIMALTD")]
    AlternateUnitDecimalPlaces,

    /// <summary>
    /// Zero suppression for alt unit tolerances.
    /// </summary>
    [DimOverrideXData(286, XDataValueKind.ZeroHandling, "DIMALTTZ")]
    AlternateUnitToleranceZeroSuppression,


    // ---------------------------
    // FIT / SCALING / AUTO-PLACEMENT
    // ---------------------------

    /// <summary>
    /// Controls text and arrow fit rules.
    /// </summary>
    [DimOverrideXData(287, XDataValueKind.Short, "DIMFIT")]
    FitOptions,

    /// <summary>
    /// Linear scale factor for dimensions.
    /// </summary>
    [DimOverrideXData(144, XDataValueKind.Double, "DIMLFAC")]
    LinearScaleFactor,

    /// <summary>
    /// Tolerance text height scale factor.
    /// </summary>
    [DimOverrideXData(146, XDataValueKind.Double, "DIMTFAC")]
    ToleranceTextScale,

    /// <summary>
    /// Controls text and arrow movement rules.
    /// </summary>
    [DimOverrideXData(279, XDataValueKind.TextMovement, "DIMTMOVE")]
    TextMovementRule,

    /// <summary>
    /// User-positioned text flag.
    /// </summary>
    [DimOverrideXData(288, XDataValueKind.Short, "DIMUPT")]
    UserPositionedText,


    // ---------------------------
    //– TOLERANCING
    // ---------------------------

    /// <summary>
    /// Enables dimension tolerances.
    /// </summary>
    [DimOverrideXData(71, XDataValueKind.Bool, "DIMTOL")]
    EnableTolerances,

    /// <summary>
    /// Enables limits-style tolerances.
    /// </summary>
    [DimOverrideXData(72, XDataValueKind.Bool, "DIMLTOL")]
    EnableLimitsTolerances,

    /// <summary>
    /// Tolerance plus value.
    /// </summary>
    [DimOverrideXData(47, XDataValueKind.Double, "DIMTP")]
    TolerancePlus,

    /// <summary>
    /// Tolerance minus value.
    /// </summary>
    [DimOverrideXData(48, XDataValueKind.Double, "DIMTM")]
    ToleranceMinus,

    /// <summary>
    /// Tolerance justification.
    /// </summary>
    [DimOverrideXData(283, XDataValueKind.ToleranceAlignment, "DIMTOLJ")]
    ToleranceJustification,

    /// <summary>
    /// Zero suppression for tolerances.
    /// </summary>
    [DimOverrideXData(284, XDataValueKind.ZeroHandling, "DIMTZIN")]
    ToleranceZeroSuppression,

    /// <summary>
    /// Zero suppression for alt-unit tolerances.
    /// </summary>
    [DimOverrideXData(285, XDataValueKind.ZeroHandling, "DIMTOLZ")]
    ToleranceZeroSuppressionAlternate
}