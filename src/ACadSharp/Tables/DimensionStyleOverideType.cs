namespace ACadSharp.Tables;

using System.ComponentModel;

public enum DimensionStyleOverrideType
{
    // ---------------------------
    // TEXT & GENERAL APPEARANCE
    // ---------------------------

    ///
    /// <summary>
    /// Text style used for dimension text.
    /// DXF code: 340
    /// </summary>
    [Description("DIMTXSTY")]
    DimensionTextStyle,
    
    ///<summary>
    /// Dimension text height.
    /// DXF code: 140
    /// </summary>
    [Description("DIMTXT")]
    DimensionTextHeight,

    /// <summary>
    /// Text vertical position relative to dimension line.
    /// DXF code: 77
    /// </summary>
    [Description("DIMTAD")]
    TextVerticalPlacement,

    /// <summary>
    /// Text inside horizontal placement rule.
    /// DXF code: 73
    /// </summary>
    [Description("DIMTIH")]
    TextInsideHorizontal,

    /// <summary>
    /// Text outside horizontal placement rule.
    /// DXF code: 74
    /// </summary>
    [Description("DIMTOH")]
    TextOutsideHorizontal,

    /// <summary>
    /// Forces text inside extension lines when possible.
    /// DXF code: 174
    /// </summary>
    [Description("DIMTIX")]
    ForceTextInside,

    /// <summary>
    /// Forces extension lines if text must be moved outside.
    /// DXF code: 172
    /// </summary>
    [Description("DIMTOFL")]
    ForceExtensionLinesOutside,

    /// <summary>
    /// Text vertical position for user-placed text.
    /// DXF code: 145
    /// </summary>
    [Description("DIMTVP")]
    TextVerticalPosition,

    /// <summary>
    /// Text alignment/justification.
    /// DXF code: 280
    /// </summary>
    [Description("DIMJUST")]
    TextJustification,

    /// <summary>
    /// Formats prefix/suffix for dimension text.
    /// DXF code: 3
    /// </summary>
    [Description("DIMPOST")]
    DimensionTextFormat,

    /// <summary>
    /// Formats prefix/suffix for alternate dimension text.
    /// DXF code: 4
    /// </summary>
    [Description("DIMAPOST")]
    AltDimensionTextFormat,


    // ---------------------------
    // COLORS
    // ---------------------------

    /// <summary>
    /// Color of dimension lines.
    /// DXF code: 176
    /// </summary>
    [Description("DIMCLRD")]
    DimensionLineColor,

    /// <summary>
    /// Color of extension lines.
    /// DXF code: 177
    /// </summary>
    [Description("DIMCLRE")]
    ExtensionLineColor,

    /// <summary>
    /// Color of dimension text.
    /// DXF code: 178
    /// </summary>
    [Description("DIMCLRT")]
    DimensionTextColor,


    // ---------------------------
    // LINE WEIGHTS & TYPES
    // ---------------------------

    /// <summary>
    /// LineType for dimension lines.
    /// DXF code: 345
    /// </summary>
    [Description("DIMLTYPE")]
    DimensionLineLinetype,

    /// <summary>
    /// LineType for first extension line.
    /// DXF code: 346
    /// </summary>
    [Description("DIMLTEX1")]
    ExtensionLine1Linetype,

    /// <summary>
    /// LineType for second extension line.
    /// DXF code: 347
    /// </summary>
    [Description("DIMLTEX2")]
    ExtensionLine2Linetype,

    /// <summary>
    /// Lineweight of dimension lines.
    /// DXF code: 371
    /// </summary>
    [Description("DIMLWD")]
    DimensionLineWeight,

    /// <summary>
    /// Lineweight of extension lines.
    /// DXF code: 372
    /// </summary>
    [Description("DIMLWE")]
    ExtensionLineWeight,


    // ---------------------------
    // DIMENSION / EXTENSION LINES
    // ---------------------------

    /// <summary>
    /// Extension of dimension lines beyond the extension lines.
    /// DXF code: 46
    /// </summary>
    [Description("DIMDLE")]
    DimensionLineExtension,

    /// <summary>
    /// Incremental spacing of dimension lines.
    /// DXF code: 43
    /// </summary>
    [Description("DIMDLI")]
    DimensionLineIncrement,

    /// <summary>
    /// Extension line extension beyond the dimension line.
    /// DXF code: 44
    /// </summary>
    [Description("DIMEXE")]
    ExtensionLineExtension,

    /// <summary>
    /// Offset distance of extension lines from the measured points.
    /// DXF code: 42
    /// </summary>
    [Description("DIMEXO")]
    ExtensionLineOffset,

    /// <summary>
    /// Suppress first extension line.
    /// DXF code: 75
    /// </summary>
    [Description("DIMSE1")]
    SuppressExtensionLine1,

    /// <summary>
    /// Suppress second extension line.
    /// DXF code: 76
    /// </summary>
    [Description("DIMSE2")]
    SuppressExtensionLine2,

    /// <summary>
    /// Suppress first dimension extension.
    /// DXF code: 281
    /// </summary>
    [Description("DIMSD1")]
    SuppressDimensionExtension1,

    /// <summary>
    /// Suppress second dimension extension.
    /// DXF code: 282
    /// </summary>
    [Description("DIMSD2")]
    SuppressDimensionExtension2,

    /// <summary>
    /// Fixed-length extension line setting.
    /// DXF code: 49
    /// </summary>
    [Description("DIMFXL")]
    ExtensionLineFixedLength,

    /// <summary>
    /// Enables fixed extension line length.
    /// DXF code: 290
    /// </summary>
    [Description("DIMFXLON")]
    ExtensionLineFixedLengthEnabled,


    // ---------------------------
    // ARROWS / SYMBOLS
    // ---------------------------

    /// <summary>
    /// Size of dimension arrows.
    /// DXF code: 41
    /// </summary>
    [Description("DIMASZ")]
    ArrowSize,

    /// <summary>
    /// Tick size for architectural ticks.
    /// DXF code:
    /// </summary>
    [Description("DIMTSZ")]
    TickSize,

    /// <summary>
    /// Center mark size for arc/circle dimensions.
    /// DXF code: 141
    /// </summary>
    [Description("DIMCEN")]
    CenterMarkSize,

    /// <summary>
    /// Uses custom arrow blocks.
    /// DXF code: 173
    /// </summary>
    [Description("DIMSAH")]
    UseCustomArrowBlocks,

    /// <summary>
    /// Arrow block for both ends.
    /// DXF code: 342
    /// </summary>
    [Description("DIMBLK")]
    ArrowBlock,

    /// <summary>
    /// Arrow block for first end.
    /// DXF code: 343
    /// </summary>
    [Description("DIMBLK1")]
    ArrowBlock1,

    /// <summary>
    /// Arrow block for second end.
    /// DXF code: 344
    /// </summary>
    [Description("DIMBLK2")]
    ArrowBlock2,

    /// <summary>
    /// Leader arrow block.
    /// DXF code:
    /// </summary>
    [Description("DIMLDRBLK")]
    LeaderArrowBlock,

    /// <summary>
    /// Arc length dimension symbol placement.
    /// DXF code: 341
    /// </summary>
    [Description("DIMARCSYM")]
    ArcLengthSymbolPlacement,


    // ---------------------------
    // UNITS & FORMATTING
    // ---------------------------

    /// <summary>
    /// Linear unit display mode.
    /// DXF code: 277
    /// </summary>
    [Description("DIMLUNIT")]
    LinearUnitFormat,

    /// <summary>
    /// Number of decimal places for linear dimensions.
    /// DXF code: 271
    /// </summary>
    [Description("DIMDEC")]
    LinearDecimalPrecision,

    /// <summary>
    /// Decimal separator character.
    /// DXF code: 278
    /// </summary>
    [Description("DIMDSEP")]
    DecimalSeparator,

    /// <summary>
    /// Angular precision.
    /// DXF code:
    /// </summary>
    [Description("DIMADEC")]
    AngularDecimalPrecision,

    /// <summary>
    /// Angular unit display format.
    /// DXF code: 179
    /// </summary>
    [Description("DIMAUNIT")]
    AngularUnitFormat,

    /// <summary>
    /// Zero suppression for primary units.
    /// DXF code: 78
    /// </summary>
    [Description("DIMZIN")]
    ZeroSuppression,

    /// <summary>
    /// Zero suppression for alternative units.
    /// DXF code: ???
    /// </summary>
    [Description("DIMALTMZF")]
    AltUnitZeroSuppressionFactor,

    /// <summary>
    /// Enables alternate units.
    /// DXF code: 170
    /// </summary>
    [Description("DIMALT")]
    EnableAlternateUnits,

    /// <summary>
    /// Scale factor for alternate units.
    /// DXF code: 143
    /// </summary>
    [Description("DIMALTF")]
    AlternateUnitScaleFactor,

    /// <summary>
    /// Alternate unit rounding value.
    /// DXF code: 148
    /// </summary>
    [Description("DIMALTRND")]
    AlternateUnitRounding,

    /// <summary>
    /// Alternate unit decimal precision.
    /// DXF code: 274
    /// </summary>
    [Description("DIMALTTD")]
    AlternateUnitToleranceDecimals,

    /// <summary>
    /// Number of decimal places for alternate units.
    /// DXF code: 171
    /// </summary>
    [Description("DIMALTD")]
    AlternateUnitDecimalPlaces,

    /// <summary>
    /// Zero suppression for alt unit tolerances.
    /// DXF code: 286
    /// </summary>
    [Description("DIMALTTZ")]
    AlternateUnitToleranceZeroSuppression,


    // ---------------------------
    // FIT / SCALING / AUTO-PLACEMENT
    // ---------------------------

    /// <summary>
    /// Controls how text and arrows are placed when space is tight.
    /// DXF code: ???
    /// </summary>
    [Description("DIMFIT")]
    FitOptions,

    /// <summary>
    /// Linear scale factor for dimensions.
    /// DXF code: 144
    /// </summary>
    [Description("DIMLFAC")]
    LinearScaleFactor,

    /// <summary>
    /// Text scale factor for tolerances.
    /// DXF code: 146
    /// </summary>
    [Description("DIMTFAC")]
    ToleranceTextScale,

    /// <summary>
    /// Controls text and arrow movement rules.
    /// DXF code: 279
    /// </summary>
    [Description("DIMTMOVE")]
    TextMovementRule,

    /// <summary>
    /// User-positioned text flag.
    /// DXF code: 288
    /// </summary>
    [Description("DIMUPT")]
    UserPositionedText,


    // ---------------------------
    // TOLERANCING
    // ---------------------------

    /// <summary>
    /// Enables dimension tolerances.
    /// DXF code: 71
    /// </summary>
    [Description("DIMTOL")]
    EnableTolerances,

    /// <summary>
    /// Enables limits-style tolerances.
    /// DXF code:
    /// </summary>
    [Description("DIMLTOL")]
    EnableLimitsTolerances,

    /// <summary>
    /// Tolerance plus value.
    /// DXF code: 47
    /// </summary>
    [Description("DIMTP")]
    TolerancePlus,

    /// <summary>
    /// Tolerance minus value.
    /// DXF code: 48
    /// </summary>
    [Description("DIMTM")]
    ToleranceMinus,

    /// <summary>
    /// Tolerance justification.
    /// DXF code: 283
    /// </summary>
    [Description("DIMTOLJ")]
    ToleranceJustification,

    /// <summary>
    /// Zero suppression for tolerances.
    /// DXF code: 284
    /// </summary>
    [Description("DIMTZIN")]
    ToleranceZeroSuppression,

    /// <summary>
    /// Zero suppression for tolerances in alt units.
    /// DXF code: ???
    /// </summary>
    [Description("DIMTOLZ")]
    ToleranceZeroSuppressionAlternate
}