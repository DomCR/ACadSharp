using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using System;

//	TODO should the described coupling of properties be implemented in this class,
//		 e.g., GenerateTolerances and LimitsGeneration?

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="DimensionStyle"/> table entry.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableDimstyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DimensionStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableDimstyle)]
	[DxfSubClass(DxfSubclassMarker.DimensionStyle)]
	public class DimensionStyle : TableEntry
	{
		public const string DefaultName = "Standard";

		public static DimensionStyle Default { get { return new DimensionStyle(DefaultName); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMSTYLE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableDimstyle;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DimensionStyle;

		/// <summary>
		/// Specifies a text prefix or suffix (or both) to the dimension measurement
		/// (see DIMPOST System Variable).
		/// </summary>
		/// <remarks><para>
		/// For example, to establish a suffix for millimeters, set <i>PostFix</i> to mm;
		/// a distance of 19.2 units would be displayed as 19.2 mm.
		/// </para><para>
		/// If tolerances are turned on, the suffix is applied to the tolerances
		/// as well as to the main dimension.
		/// </para><para>
		/// Use &lt;&gt; to indicate placement of the text in relation to the dimension value.
		/// For example, enter &lt;&gt; mm to display a 5.0 millimeter radial dimension as "5.0mm".
		/// If you entered mm &lt;&gt;, the dimension would be displayed as "mm 5.0". Use the &lt;&gt;
		/// mechanism for angular dimensions.
		/// </para>
		/// </remarks>
		[DxfCodeValue(3)]
		public string PostFix { get; set; } = "<>";

		/// <summary>
		/// Specifies a text prefix or suffix (or both) to the alternate dimension
		/// measurement for all types of dimensions except angular
		/// (see DIMAPOST System Variable).
		/// </summary>
		/// <remarks><para>
		/// For instance, if the current units are Architectural, <see cref="AlternateUnitDimensioning"/>
		/// is on (true), <see cref="AlternateUnitScaleFactor"/> is 25.4 (the number of millimeters per inch),
		/// <see cref="AlternateUnitDecimalPlaces"/> is 2, and <i>AlternateDimensioningSuffix</i> is set to "mm",
		/// a distance of 10 units would be displayed as 10"[254.00mm].
		/// </para><para>
		/// To turn off an established prefix or suffix (or both), set it to a single period (.).
		/// </para>
		/// </remarks>
		[DxfCodeValue(4)]
		public string AlternateDimensioningSuffix { get; set; } = "[]";

		/// <summary>
		/// Appends tolerances to dimension text
		/// (see DIMTOL System Variable).
		/// </summary>
		/// <remarks>
		/// Setting <i>GenerateTolerances</i> to on (true) turns <see cref="LimitsGeneration"/> off (false).
		/// </remarks>
		[DxfCodeValue(71)]
		public bool GenerateTolerances { get; set; }

		/// <summary>
		/// Generates dimension limits as the default text
		/// (see DIMLIM System Variable).
		/// </summary>
		/// <remarks>
		/// Setting <i>LimitsGeneration</i> to on (true) turns <see cref="GenerateTolerances"/> off (false).
		/// </remarks>
		[DxfCodeValue(72)]
		public bool LimitsGeneration { get; set; }

		/// <summary>
		/// Controls the position of dimension text inside the extension lines for all
		/// dimension types except Ordinate.
		/// (see DIMTIH System Variable).
		/// </summary>
		/// <value><b>true</b> if the text is to be drawn horizontally;
		/// <b>false </b> if the text is to be aligned with the dimension line.</value>
		[DxfCodeValue(73)]
		public bool TextInsideHorizontal { get; set; }

		/// <summary>
		/// Controls the position of dimension text outside the extension lines
		/// (see DIMTOH System Variable).
		/// </summary>
		/// <value><b>true</b> if the text is to be drawn horizontally;
		/// <b>false </b> if the text is to be aligned with the dimension line.</value>
		[DxfCodeValue(74)]
		public bool TextOutsideHorizontal { get; set; }

		/// <summary>
		/// Suppresses display of the first extension line
		/// (see DIMSE1 System Variable).
		/// </summary>
		/// <value><b>true</b> if the first extension line is to be suppressed; otherwise <b>false</b>.
		/// </value>
		[DxfCodeValue(75)]
		public bool SuppressFirstExtensionLine { get; set; }

		/// <summary>
		/// Suppresses display of the second extension line
		/// (see DIMSE2 System Variable).
		/// </summary>
		/// <value><b>true</b> if the second extension line is to be suppressed; otherwise <b>false</b>.
		/// </value>
		[DxfCodeValue(76)]
		public bool SuppressSecondExtensionLine { get; set; }

		/// <summary>
		/// Controls the vertical position of text in relation to the dimension line
		/// (see DIMTAD System Variable).
		/// </summary>
		[DxfCodeValue(77)]
		public DimensionTextVerticalAlignment TextVerticalAlignment { get; set; }

		/// <summary>
		/// Controls the suppression of zeros in the primary unit value
		/// (see DIMZIN System Variable).
		/// </summary>
		[DxfCodeValue(78)]
		public ZeroHandling ZeroHandling { get; set; }

		/// <summary>Controls the display of alternate units in dimensions
		/// (see DIMALT System Variable).
		/// </summary>
		/// <value>
		/// <b>true</b> enables alternate units; <b>false</b> disables alternate units.
		/// </value>
		[DxfCodeValue(170)]
		public bool AlternateUnitDimensioning { get; set; }

		/// <summary>
		/// Controls the number of decimal places in alternate units
		/// (see DIMALTD System Variable).
		/// </summary>
		/// <remarks>
		/// If <see cref="AlternateUnitDimensioning"/> is turned on (true), <i>AlternateUnitDecimalPlaces</i>
		/// specifies the number of digits displayed to the right of the decimal point in the alternate
		/// measurement.
		/// </remarks>
		[DxfCodeValue(171)]
		public short AlternateUnitDecimalPlaces { get; set; }

		/// <summary>
		/// Controls whether a dimension line is drawn between the extension lines even when the text
		/// is placed outside.
		/// (see DIMTOFL System Variable).
		/// </summary>
		/// <remarks>
		/// For radius and diameter dimensions, a dimension line is drawn inside the circle or arc when the text,
		/// arrowheads, and leader are placed outside.
		/// </remarks>
		/// <value>
		/// <para>
		/// <b>true</b>: Draws dimension lines between the measured points even when arrowheads are placed
		/// outside the measured points
		/// </para><para>
		/// <b>false</b>: Does not draw dimension lines between the measured points when arrowheads are placed
		/// outside the measured points
		/// </para>
		/// </value>
		[DxfCodeValue(172)]
		public bool TextOutsideExtensions { get; set; }

		/// <summary>
		/// Controls the display of dimension line arrowhead blocks
		/// (see DIMSAH System Variable).
		/// </summary>
		/// <value>
		/// <b>true</b> if arrowhead blocks set by <see cref="DimArrow1"/> and <see cref="DimArrow2"/>
		/// shall be used;
		/// <b>false</b> if arrowhead block set by <see cref="ArrowBlock"/> shall be used.
		/// </value>
		[DxfCodeValue(173)]
		public bool SeparateArrowBlocks { get; set; }

		/// <summary>
		/// Draws text between extension lines
		/// (see DIMTIX System Variable).
		/// </summary>
		/// <value><para>
		/// <b>false</b>: For linear and angular dimensions, dimension text is placed
		/// inside the extension lines if there is sufficient room.
		/// </para><para>
		/// <b>true</b>: Draws dimension text between the extension lines even if it
		/// would ordinarily be placed outside those lines.
		/// For radius and diameter dimensions, TextInsideExtensions on (true) always forces
		/// the dimension text outside the circle or arc.
		/// </para>
		/// </value>
		[DxfCodeValue(174)]
		public bool TextInsideExtensions { get; set; }

		/// <summary>
		/// Suppresses arrowheads if not enough space is available inside the extension lines
		/// (see DIMSOXD System Variable).
		/// </summary>
		/// <value>
		/// <b>true</b> if arrowheads are to be suppressed; otherwise, <b>false</b>.
		/// </value>
		[DxfCodeValue(175)]
		public bool SuppressOutsideExtensions { get; set; }

		/// <summary>
		/// Controls the number of precision places displayed in angular dimensions
		/// (see DIMADEC System Variable).
		/// </summary>
		/// <value><para>
		/// <b>-1</b>: Angular dimensions display the number of decimal places specified by
		/// the <see cref="DecimalPlaces"/> property
		/// </para><para>
		/// <b>0-8</b>: Specifies the number of decimal places displayed in angular dimensions
		/// (independent of the value of the <see cref="DecimalPlaces"/> property).
		/// </para>
		/// </value>
		[DxfCodeValue(179)]
		public short AngularDimensionDecimalPlaces { get; set; }

		/// <summary>
		/// Controls the horizontal positioning of dimension text
		/// (see DIMJUST System Variable).
		/// </summary>
		[DxfCodeValue(280)]
		public DimensionTextHorizontalAlignment TextHorizontalAlignment { get; set; }

		/// <summary>
		/// Controls suppression of the first dimension line and arrowhead
		/// (see DIMSD1 System Variable).
		/// </summary>
		/// <value>
		/// <b>true</b> if the first dimension line is to be suppressed; otherwise, <b>false</b>.
		/// </value>
		[DxfCodeValue(281)]
		public bool SuppressFirstDimensionLine { get; set; }

		/// <summary>
		/// Controls suppression of the second dimension line and arrowhead
		/// (see DIMSD2 System Variable).
		/// </summary>
		/// <value>
		/// <b>true</b> if the second dimension line is to be suppressed; otherwise, <b>false</b>.
		/// </value>
		[DxfCodeValue(282)]
		public bool SuppressSecondDimensionLine { get; set; }

		/// <summary>
		/// Gets or sets the vertical justification for tolerance values relative to the nominal dimension text.
		/// (see DIMTOLJ System Variable).
		/// </summary>
		[DxfCodeValue(283)]
		public ToleranceAlignment ToleranceAlignment { get; set; }

		/// <summary>
		/// Controls the suppression of zeros in tolerance values
		/// (see DIMTZIN System Variable).
		/// </summary>
		/// <remarks>
		/// Value 0-3 affect feet-and-inch dimensions only.
		/// </remarks>
		[DxfCodeValue(284)]
		public ZeroHandling ToleranceZeroHandling { get; set; }

		/// <summary>
		/// Controls the suppression of zeros for alternate unit dimension values
		/// (see DIMALTZ System Variable).
		/// </summary>
		[DxfCodeValue(285)]
		public ZeroHandling AlternateUnitZeroHandling { get; set; }

		/// <summary>
		/// Determines how dimension text and arrows are arranged when space is not sufficient to
		/// place both within the extension lines.
		/// (see DIMFIT System Variable).
		/// </summary>
		[DxfCodeValue(287)]
		public char DimensionFit { get; set; }

		/// <summary>
		/// Controls options for user-positioned text
		/// (see DIMUPT System Variable).
		/// </summary>
		/// <value>
		/// <para>
		/// <b>false</b>: Cursor controls only the dimension line location.
		/// </para><para>
		/// <b>true</b>: Cursor controls both the text position and the dimension line location,
		/// </para>
		/// </value>
		[DxfCodeValue(288)]
		public bool CursorUpdate { get; set; }

		/// <summary>
		/// Controls suppression of zeros in tolerance values
		/// (see DIMALTTZ System Variable).
		/// </summary>
		[DxfCodeValue(286)]
		public ZeroHandling AlternateUnitToleranceZeroHandling { get; set; }

		/// <summary>
		/// DIMUNIT (obsolete, now use DIMLUNIT AND DIMFRAC)
		/// </summary>
		[DxfCodeValue(270)]
		public short DimensionUnit { get; set; }

		/// <summary>
		/// Gets or sets the units format for angular dimensions
		/// (see DIMAUNIT System Variable).
		/// </summary>
		[DxfCodeValue(275)]
		public AngularUnitFormat AngularUnit { get; set; }

		/// <summary>
		/// Gets or sets the number of decimal places displayed for the primary
		/// units of a dimension
		/// (see DIMDEC System Variable).
		/// </summary>
		[DxfCodeValue(271)]
		public short DecimalPlaces { get; set; }

		/// <summary>
		/// Gets or sets the number of decimal places to display in tolerance values
		/// for the primary units in a dimension
		/// (see DIMTDEC System Variable).
		/// </summary>
		[DxfCodeValue(272)]
		public short ToleranceDecimalPlaces { get; set; }

		/// <summary>
		/// Gets or sets the units format for alternate units of all dimension substyles
		/// except Angular
		/// (see DIMALTU System Variable).
		/// </summary>
		[DxfCodeValue(273)]
		public LinearUnitFormat AlternateUnitFormat { get; set; } = LinearUnitFormat.Decimal;

		/// <summary>
		/// Gets or sets the number of decimal places for the tolerance values in the alternate
		/// units of a dimension
		/// (see DIMALTTD System Variable).
		/// </summary>
		[DxfCodeValue(274)]
		public short AlternateUnitToleranceDecimalPlaces { get; set; }

		/// <summary>
		/// Gets or sets the overall scale factor applied to dimensioning variables that specify
		/// sizes, distances, or offsets
		/// (see DIMSCALE System Variable).
		/// </summary>
		/// <remarks>
		/// <para>
		/// This ScaleFactor does not affect measured lengths, coordinates, or angles.
		/// </para><para>
		/// Use <i>ScaleFactor</i> to control the overall scale of dimensions. However, if the current
		/// dimension style is annotative, <i>ScaleFactor</i> is automatically set to zero and the
		/// dimension scale is controlled by the CANNOSCALE system variable. <i>ScaleFactor</i> cannot
		/// be set to a non-zero value when using annotative dimensions.
		/// </para><para>
		/// Also affects the leader objects with the LEADER command.
		/// </para><para>
		/// Use MLEADERSCALE to scale multileader objects created with the MLEADER command.
		/// </para>
		/// </remarks>
		/// <value>
		/// <para>
		/// <b>0.0</b>
		/// </para><para>
		/// A reasonable default value is computed based on the scaling between the current
		/// model space viewport and paper space. If you are in paper space or model space
		/// and not using the paper space feature, the scale factor is 1.0.
		/// </para><para>
		/// <b>&gt;0</b>
		/// </para><para>
		/// A scale factor is computed that leads text sizes, arrowhead sizes, and other scaled
		/// distances to plot at their face values.
		/// </para>
		/// </value>
		[DxfCodeValue(40)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// Controls the size of dimension line and leader line arrowheads. Also controls the
		/// size of hook lines
		/// (see DIMASZ System Variable).
		/// </summary>
		[DxfCodeValue(41)]
		public double ArrowSize
		{
			get { return this._arrowSize; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "The ArrowSize must be equals or greater than zero.");
				}
				this._arrowSize = value;
			}
		}

		/// <summary>
		/// Specifies how far extension lines are offset from origin points
		/// (see DIMEXO System Variable).
		/// </summary>
		/// <remarks>
		/// With fixed-length extension lines, this value determines the minimum offset.
		/// </remarks>
		[DxfCodeValue(42)]
		public double ExtensionLineOffset { get; set; }

		/// <summary>
		/// Controls the spacing of the dimension lines in baseline dimensions
		/// (see DIMDLI System Variable).
		/// </summary>
		/// <remarks>
		/// Each dimension line is offset from the previous one by this amount, if necessary,
		/// to avoid drawing over it. Changes made with <i>DimensionLineIncrement</i> are
		/// not applied to existing dimensions.
		/// </remarks>
		[DxfCodeValue(43)]
		public double DimensionLineIncrement { get; set; }

		/// <summary>
		/// Specifies how far to extend the extension line beyond the dimension line
		/// (see DIMEXE System Variable).
		/// </summary>
		[DxfCodeValue(44)]
		public double ExtensionLineExtension { get; set; }

		/// <summary>
		/// Rounds all dimensioning distances to the specified value
		/// (see DIMRND System Variable).
		/// </summary>
		/// <remarks>
		/// <para>
		/// For instance, if <i>Rounding</i> is set to 0.25, all distances round
		/// to the nearest 0.25 unit. If you set the value to 1.0, all distances round to the
		/// nearest integer.
		/// </para><para>
		/// Note that the number of digits edited after the decimal point depends on the
		/// precision set by <see cref="DecimalPlaces"/>.
		/// </para><para>
		/// This  does not apply to angular dimensions.
		/// </para>
		/// </remarks>
		[DxfCodeValue(45)]
		public double Rounding { get; set; }

		/// <summary>
		/// Sets the distance the dimension line extends beyond the extension line when
		/// oblique strokes are drawn instead of arrowheads.
		/// (see DIMDLE System Variable).
		/// </summary>
		[DxfCodeValue(46)]
		public double DimensionLineExtension { get; set; }

		/// <summary>
		/// Gets or sets the maximum (or upper) tolerance limit for dimension text when
		/// <see cref="GenerateTolerances"/> or <see cref="LimitsGeneration"/> is on (true)
		/// (see DIMTP System Variable).
		/// </summary>
		[DxfCodeValue(47)]
		public double PlusTolerance { get; set; }

		/// <summary>
		/// Gets or sets the minimum (or lower) tolerance limit for dimension text when
		/// <see cref="GenerateTolerances"/> or <see cref="LimitsGeneration"/> is on (true).
		/// (see DIMTM System Variable).
		/// </summary>
		/// <remarks>
		/// <para>
		/// <i>MinusTolerance</i> accepts signed values. If <see cref="GenerateTolerances"/> is on
		/// and <see cref="PlusTolerance"/> and <i>MinusTolerance</i> are set to the same value,
		/// a tolerance value is drawn.
		/// </para><para>
		/// If <i>MinusTolerance</i> and <see cref="PlusTolerance"/> values differ, the upper
		/// tolerance is drawn above the lower, and a plus sign is added to the <see cref="PlusTolerance"/>
		/// value if it is positive.
		/// </para><para>
		/// For <i>MinusTolerance</i>, the program uses the negative of the value you enter
		/// (adding a minus sign if you specify a positive number and a plus sign if you specify a negative
		/// number).
		/// </para>
		/// </remarks>
		[DxfCodeValue(48)]
		public double MinusTolerance { get; set; }

		/// <summary>
		/// Sets the total length of the extension lines starting from the dimension line
		/// toward the dimension origin
		/// (see DIMFXL System Variable).
		/// </summary>
		[DxfCodeValue(49)]
		public double FixedExtensionLineLength { get; set; }

		/// <summary>
		/// (see DIMJOGANG System Variable).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double JoggedRadiusDimensionTransverseSegmentAngle { get; set; }

		/// <summary>
		/// Controls the background of dimension text
		/// (see DIMTFILL System Variable).
		/// </summary>
		[DxfCodeValue(69)]
		public DimensionTextBackgroundFillMode TextBackgroundFillMode { get; set; }

		/// <summary>
		/// Determines the angle of the transverse segment of the dimension line
		/// in a jogged radius dimension.
		/// (see DIMTFILLCLR System Variable).
		/// </summary>
		//[DxfCodeValue(70)]	//Not present in the dxf documentation
		public Color TextBackgroundColor { get; set; }

		/// <summary>
		/// Suppresses zeros for angular dimensions
		/// (see DIMAZIN System Variable).
		/// </summary>
		[DxfCodeValue(79)]
		public ZeroHandling AngularZeroHandling { get; set; }

		/// <summary>
		/// Controls display of the arc symbol in an arc length dimension
		/// (see DIMARCSYM System Variable).
		/// </summary>
		[DxfCodeValue(90)]
		public ArcLengthSymbolPosition ArcLengthSymbolPosition { get; set; }

		/// <summary>
		/// Specifies the height of dimension text, unless the current text style has a fixed height
		/// (see DIMTXT System Variable).
		/// </summary>
		[DxfCodeValue(140)]
		public double TextHeight { get; set; } = 0.18;

		/// <summary>
		/// Controls drawing of circle or arc center marks and centerlines by the
		/// DIMCENTER, DIMDIAMETER, and DIMRADIUS commands
		/// (see DIMCEN System Variable).
		/// </summary>
		/// <remarks>
		/// For DIMDIAMETER and DIMRADIUS, the center mark is drawn only if you place
		/// the dimension line outside the circle or arc.
		/// </remarks>
		/// <value>
		/// <para>
		/// <b>0</b>
		/// </para><para>
		/// No center marks or lines are drawn
		/// </para><para>
		/// <b>&lt;0</b>
		/// </para><para>
		/// Centerlines are drawn
		/// </para><para>
		/// <b>&gt;0</b>
		/// </para><para>
		/// Center marks are drawn
		/// </para>
		/// </value>
		[DxfCodeValue(141)]
		public double CenterMarkSize { get; set; }

		/// <summary>
		/// Specifies the size of oblique strokes drawn instead of arrowheads for linear, radius,
		/// and diameter dimensioning
		/// (see DIMTSZ System Variable).
		/// </summary>
		/// <value>
		/// <para>
		/// <b>0</b>
		/// </para><para>
		/// Draws arrowheads.
		/// </para><para>
		/// <b>&gt;0</b>
		/// </para><para>
		/// Draws oblique strokes instead of arrowheads.
		/// The size of the oblique strokes is determined by this value multiplied by the value
		/// of <see cref="ScaleFactor"/>.
		/// </para>
		/// </value>
		[DxfCodeValue(142)]
		public double TickSize { get; set; }

		/// <summary>
		/// Controls the multiplier for alternate units
		/// (see DIMALTF System Variable).
		/// </summary>
		/// <remarks>
		/// If <see cref="AlternateUnitDimensioning"/> is turned on (true), the value of this
		/// property (AlternateUnitScaleFactor) multiplies linear dimensions by a factor to produce
		/// a value in an alternate system of measurement. The initial value represents the number
		/// of millimeters in an inch.
		/// </remarks>
		[DxfCodeValue(143)]
		public double AlternateUnitScaleFactor { get; set; } = 25.4;

		/// <summary>
		/// Sets a scale factor for linear dimension measurements
		/// (see DIMLFAC System Variable).
		/// </summary>
		/// <remarks>
		/// <para>
		/// All linear dimension distances, including radii, diameters, and coordinates, are multiplied
		/// by this <i>LinearScaleFactor</i> before being converted to dimension text.
		/// Positive values of <i>LinearScaleFactor</i> are applied to dimensions in both model space and
		/// paper space; negative values are applied to paper space only.
		/// </para><para>
		/// <i>LinearScaleFactor</i> applies primarily to nonassociative dimensions (DIMASSOC set 0 or 1).
		/// For nonassociative dimensions in paper space, <i>LinearScaleFactor</i> must be set individually
		/// for each layout viewport to accommodate viewport scaling.
		/// </para><para>
		/// <i>LinearScaleFactor</i> has no effect on angular dimensions, and is not applied to the values held in
		/// <see cref="Rounding"/>, <see cref="MinusTolerance"/>, or <see cref="PlusTolerance"/>.
		/// </para>
		/// </remarks>
		[DxfCodeValue(144)]
		public double LinearScaleFactor { get; set; } = 1.0;

		/// <summary>
		/// Controls the vertical position of dimension text above or below the dimension line
		/// (see DIMTVP System Variable).
		/// </summary>
		/// <remarks>
		/// The <i>TextVerticalPosition</i> value is used when <see cref="TextVerticalAlignment"/>
		/// is off. The magnitude of the vertical offset of text is the product of the text height
		/// and <i>TextVerticalPosition</i>. Setting <i>TextVerticalPosition</i> to 1.0 is equivalent
		/// to setting <see cref="TextVerticalAlignment"/> to on. The dimension line splits to
		/// accommodate the text only if the absolute value of <i>TextVerticalPosition</i> is less than 0.7.
		/// </remarks>
		[DxfCodeValue(145)]
		public double TextVerticalPosition { get; set; }

		/// <summary>
		/// Specifies a scale factor for the text height of fractions and tolerance values relative
		/// to the dimension text height, as set by <see cref="TextHeight"/>
		/// (see DIMTFAC System Variable).
		/// </summary>
		/// <remarks>
		/// For example, if <i>ToleranceScaleFactor</i> is set to 1.0, the text height of fractions and
		/// tolerances is the same height as the dimension text. If <i>ToleranceScaleFactor</i> is set
		/// to 0.7500, the text height of fractions and tolerances is three-quarters the size of
		/// dimension text.
		/// </remarks>
		[DxfCodeValue(146)]
		public double ToleranceScaleFactor { get; set; } = 1.0;

		/// <summary>
		/// Gets or sets the distance around the dimension text when the dimension line breaks to
		/// accommodate dimension text
		/// (see DIMGAP System Variable).
		/// </summary>
		/// <remarks>
		/// <para>
		/// Also sets the gap between annotation and a hook line created with the LEADER command.
		/// If you enter a negative value, <i>DimensionLineGap</i> places a box around the dimension text.
		/// </para><para>
		/// The value of <i>DimensionLineGap</i> is also used as the minimum length of each segment
		/// of the dimension line. To locate the components of a linear dimension within the extension
		/// lines, enough space must be available for both arrowheads (2 x <see cref="ArrowSize"/>),
		/// both dimension line segments (2 x <i>DimensionLineGap</i>), a gap on either side of the
		/// dimension text (another 2 x <i>DimensionLineGap</i>), and the length of the dimension text,
		/// which depends on its size and number of decimal places displayed.
		/// </para>
		/// </remarks>
		[DxfCodeValue(147)]
		public double DimensionLineGap { get; set; }

		/// <summary>
		/// Rounds off the alternate dimension units
		/// (see DIMALTRND System Variable).
		/// </summary>
		[DxfCodeValue(148)]
		public double AlternateUnitRounding { get; set; }

		/// <summary>
		/// Gets or sets colors to dimension lines, arrowheads, and dimension leader lines
		/// (see DIMCLRD System Variable).
		/// </summary>
		/// <remarks>
		/// Also controls the color of leader lines created with the LEADER command.
		/// </remarks>
		[DxfCodeValue(176)]
		public Color DimensionLineColor { get; set; }

		/// <summary>
		/// Gets or sets colors to extension lines, center marks, and centerlines
		/// (see DIMCLRE System Variable).
		/// </summary>
		[DxfCodeValue(177)]
		public Color ExtensionLineColor { get; set; }

		/// <summary>
		/// Assigns colors to dimension text
		/// (see DIMCLRT System Variable).
		/// </summary>
		/// <remarks>
		/// The color can be any valid color number.
		/// </remarks>
		[DxfCodeValue(178)]
		public Color TextColor { get; set; }

		/// <summary>
		/// Gets or sets the fraction format when <see cref="LinearUnitFormat"/> is set to 4 (Architectural) or 5 (Fractional).
		/// (see DIMFRAC System Variable).
		/// </summary>
		[DxfCodeValue(276)]
		public FractionFormat FractionFormat { get; set; }

		/// <summary>
		/// Gets or sets units for all dimension types except Angular
		/// (see DIMLUNIT System Variable).
		/// </summary>
		[DxfCodeValue(277)]
		public LinearUnitFormat LinearUnitFormat { get; set; } = LinearUnitFormat.Decimal;

		/// <summary>
		/// Specifies a single-character decimal separator to use when creating dimensions whose unit
		/// format is decimal
		/// (see DIMDSEP System Variable).
		/// </summary>
		/// <remarks>
		/// When prompted, enter a single character at the Command prompt. If dimension units is set
		/// to Decimal, the <i>DecimalSeparator</i> character is used instead of the default decimal point. 
		/// If <i>DecimalSeparator</i> is set to NULL (default value), the decimal point is used as
		/// the dimension separator.
		/// </remarks>
		[DxfCodeValue(278)]
		public char DecimalSeparator { get; set; } = '.';

		/// <summary>
		/// Sets dimension text movement rules
		/// (see DIMTMOVE System Variable).
		/// </summary>
		[DxfCodeValue(279)]
		public TextMovement TextMovement { get; set; }

		/// <summary>
		/// Controls whether extension lines are set to a fixed length
		/// (see DIMFXLON System Variable).
		/// </summary>
		/// <value>
		/// <b>true</b> when extension lines are set to the length specified by
		/// <see cref="FixedExtensionLineLength"/>; otherwise, <b>false</b>.
		/// </value>
		[DxfCodeValue(290)]
		public bool IsExtensionLineLengthFixed { get; set; }

		/// <summary>
		/// Specifies the reading direction of the dimension text
		/// (see DIMTXTDIRECTION System Variable).
		/// </summary>
		[DxfCodeValue(295)]
		public TextDirection TextDirection { get; set; }

		public double AltMzf { get; set; }

		public double Mzf { get; set; }

		/// <summary>
		/// Assigns lineweight to dimension lines
		/// (see DIMLWD System Variable).
		/// </summary>
		/// <value>
		/// Positive values represent line weight in hundredths of millimeters.
		/// (Multiply a value by 2540 to convert values from inches to hundredths of millimeters.)
		/// </value>
		[DxfCodeValue(371)]
		public LineweightType DimensionLineWeight { get; set; }

		/// <summary>
		/// Assigns lineweight to extension lines
		/// (see DIMLWE System Variable).
		/// </summary>
		/// <value>
		/// Positive values represent line weight in hundredths of millimeters.
		/// (Multiply a value by 2540 to convert values from inches to hundredths of millimeters.)
		/// </value>
		[DxfCodeValue(372)]
		public LineweightType ExtensionLineWeight { get; set; }

		public string AltMzs { get; set; }
		public string Mzs { get; set; }

		/// <summary>
		/// Determines how dimension text and arrows are arranged when space is not sufficient
		/// to place both within the extension lines.
		/// (see DIMATFIT System Variable).
		/// </summary>
		/// <remarks>
		/// A leader is added to moved dimension text when <see cref="TextMovement"/> is set to
		/// <see cref="TextMovement.AddLeaderWhenTextMoved"/>.
		/// </remarks>
		[DxfCodeValue(289)]
		public short DimensionTextArrowFit { get; set; }

		/// <summary>
		/// Specifies the text style of the dimension
		/// (see DIMTXSTY System Variable).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public TextStyle Style
		{
			get { return this._style; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._style = this.updateTable(value, this.Document.TextStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <summary>
		/// Specifies the arrow type for leaders
		/// (see DIMLDRBLK System Variable).
		/// </summary>
		/// <value>
		/// A <see cref="BlockRecord"/> that makes up an arrowhead or null if the default,
		/// closed-filled arrowhead is to be displayed.
		/// </value>
		/// <remarks>
		/// Note: Annotative blocks cannot be used as custom arrowheads for dimensions or leaders.
		/// </remarks>
		[DxfCodeValue(341)]
		public BlockRecord LeaderArrow { get; set; }

		/// <summary>
		/// Gets or sets the arrowhead block displayed at the ends of dimension lines
		/// (see DIMBLK System Variable).
		/// </summary>
		/// <value>
		/// A <see cref="BlockRecord"/> that makes up an arrowhead or null if the default,
		/// closed-filled arrowhead is to be displayed.
		/// </value>
		/// <remarks>
		/// <para>
		/// Note: Annotative blocks cannot be used as custom arrowheads for dimensions or leaders.
		/// </para>
		/// </remarks>
		[DxfCodeValue(342)]
		public BlockRecord ArrowBlock { get; set; }

		//5	DIMBLK(obsolete, now object ID)

		//6	DIMBLK1(obsolete, now object ID)

		//7	DIMBLK2(obsolete, now object ID)

		/// <summary>
		/// Gets or sets the arrowhead for the first end of the dimension line when
		/// <see cref="SeparateArrowBlocks"/> is on (true)
		/// (see DIMBLK1 System Variable).
		/// </summary>
		/// <value>
		/// A <see cref="BlockRecord"/> that makes up an arrowhead or null if the default,
		/// closed-filled arrowhead is to be displayed.
		/// </value>
		/// <remarks>
		/// <para>
		/// Note: Annotative blocks cannot be used as custom arrowheads for dimensions or leaders.
		/// </para>
		/// </remarks>
		[DxfCodeValue(343)]
		public BlockRecord DimArrow1 { get; set; }

		/// <summary>
		/// Gets or sets the arrowhead for the first end of the dimension line when
		/// <see cref="SeparateArrowBlocks"/> is on (true)
		/// (see DIMBLK2 System Variable).
		/// </summary>
		/// <value>
		/// A <see cref="BlockRecord"/> that makes up an arrowhead or null if the default,
		/// closed-filled arrowhead is to be displayed.
		/// </value>
		/// <remarks>
		/// <para>
		/// Note: Annotative blocks cannot be used as custom arrowheads for dimensions or leaders.
		/// </para>
		/// </remarks>
		[DxfCodeValue(344)]
		public BlockRecord DimArrow2 { get; set; }

		private double _arrowSize = 0.18;

		private TextStyle _style = TextStyle.Default;

		internal DimensionStyle() : base() { }

		public DimensionStyle(string name) : base(name) { }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			DimensionStyle clone = new DimensionStyle(this.Name);
			clone.Style = (TextStyle)this.Style?.Clone();
			clone.LeaderArrow = (BlockRecord)this.LeaderArrow?.Clone();
			clone.ArrowBlock = (BlockRecord)this.ArrowBlock?.Clone();
			clone.DimArrow1 = (BlockRecord)this.DimArrow1?.Clone();
			clone.DimArrow2 = (BlockRecord)this.DimArrow2?.Clone();
			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = this.updateTable(this.Style, doc.TextStyles);

			doc.DimensionStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.DimensionStyles.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Style = (TextStyle)this.Style.Clone();
		}

		protected void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.Style))
			{
				this.Style = this.Document.TextStyles[TextStyle.DefaultName];
			}
		}
	}
}
