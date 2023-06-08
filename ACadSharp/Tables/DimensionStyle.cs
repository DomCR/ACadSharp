using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using System;
using ACadSharp.Blocks;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="DimensionStyle"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableDimstyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DimensionStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableDimstyle)]
	[DxfSubClass(DxfSubclassMarker.DimensionStyle)]
	public class DimensionStyle : TableEntry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMSTYLE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableDimstyle;

		public const string DefaultName = "Standard";

		public static DimensionStyle Default { get { return new DimensionStyle(DefaultName); } }

		/// <summary>
		/// DIMPOST
		/// </summary>
		[DxfCodeValue(3)]
		public string PostFix { get; set; } = "<>";

		/// <summary>
		/// DIMAPOST
		/// </summary>
		[DxfCodeValue(4)]
		public string AlternateDimensioningSuffix { get; set; } = "[]";

		/// <summary>
		/// DIMTOL
		/// </summary>
		[DxfCodeValue(71)]
		public bool GenerateTolerances { get; set; }

		/// <summary>
		/// DIMLIM
		/// </summary>
		[DxfCodeValue(72)]
		public bool LimitsGeneration { get; set; }

		/// <summary>
		/// DIMTIH
		/// </summary>
		[DxfCodeValue(73)]
		public bool TextInsideHorizontal { get; set; }

		/// <summary>
		/// DIMTOH
		/// </summary>
		[DxfCodeValue(74)]
		public bool TextOutsideHorizontal { get; set; }

		/// <summary>
		/// DIMSE1
		/// </summary>
		[DxfCodeValue(75)]
		public bool SuppressFirstExtensionLine { get; set; }

		/// <summary>
		/// DIMSE2
		/// </summary>
		[DxfCodeValue(76)]
		public bool SuppressSecondExtensionLine { get; set; }

		/// <summary>
		/// DIMALT
		/// </summary>
		[DxfCodeValue(170)]
		public bool AlternateUnitDimensioning { get; set; }

		/// <summary>
		/// DIMTOFL
		/// </summary>
		[DxfCodeValue(172)]
		public bool TextOutsideExtensions { get; set; }

		/// <summary>
		/// DIMSAH
		/// </summary>
		[DxfCodeValue(173)]
		public bool SeparateArrowBlocks { get; set; }

		/// <summary>
		/// DIMTIX
		/// </summary>
		[DxfCodeValue(174)]
		public bool TextInsideExtensions { get; set; }

		/// <summary>
		/// DIMSOXD
		/// </summary>
		[DxfCodeValue(175)]
		public bool SuppressOutsideExtensions { get; set; }

		/// <summary>
		/// DIMALTD
		/// </summary>
		[DxfCodeValue(171)]
		public short AlternateUnitDecimalPlaces { get; set; }

		/// <summary>
		/// DIMZIN
		/// </summary>
		[DxfCodeValue(78)]
		public ZeroHandling ZeroHandling { get; set; }

		/// <summary>
		/// DIMSD1
		/// </summary>
		[DxfCodeValue(281)]
		public bool SuppressFirstDimensionLine { get; set; }

		/// <summary>
		/// DIMSD2
		/// </summary>
		[DxfCodeValue(282)]
		public bool SuppressSecondDimensionLine { get; set; }

		/// <summary>
		/// DIMTOLJ
		/// </summary>
		[DxfCodeValue(283)]
		public ToleranceAlignment ToleranceAlignment { get; set; }

		/// <summary>
		/// DIMJUST
		/// </summary>
		[DxfCodeValue(280)]
		public DimensionTextHorizontalAlignment TextHorizontalAlignment { get; set; }

		/// <summary>
		/// DIMFIT
		/// </summary>
		[DxfCodeValue(287)]
		public char DimensionFit { get; set; }

		/// <summary>
		/// DIMUPT
		/// </summary>
		[DxfCodeValue(288)]
		public bool CursorUpdate { get; set; }

		/// <summary>
		/// DIMTZIN
		/// </summary>
		[DxfCodeValue(284)]
		public ZeroHandling ToleranceZeroHandling { get; set; }

		/// <summary>
		/// DIMALTZ
		/// </summary>
		[DxfCodeValue(285)]
		public ZeroHandling AlternateUnitZeroHandling { get; set; }

		/// <summary>
		/// DIMADEC
		/// </summary>
		[DxfCodeValue(179)]
		public short AngularDimensionDecimalPlaces { get; set; }

		/// <summary>
		/// DIMALTTZ
		/// </summary>
		[DxfCodeValue(286)]
		public ZeroHandling AlternateUnitToleranceZeroHandling { get; set; }

		/// <summary>
		/// DIMTAD
		/// </summary>
		[DxfCodeValue(77)]
		public DimensionTextVerticalAlignment TextVerticalAlignment { get; set; }

		/// <summary>
		/// DIMUNIT (obsolete, now use DIMLUNIT AND DIMFRAC)
		/// </summary>
		[DxfCodeValue(270)]
		public short DimensionUnit { get; set; }

		/// <summary>
		/// DIMAUNIT
		/// </summary>
		[DxfCodeValue(275)]
		public AngularUnitFormat AngularUnit { get; set; }

		/// <summary>
		/// DIMDEC
		/// </summary>
		[DxfCodeValue(271)]
		public short DecimalPlaces { get; set; }

		/// <summary>
		/// DIMTDEC
		/// </summary>
		[DxfCodeValue(272)]
		public short ToleranceDecimalPlaces { get; set; }

		/// <summary>
		/// DIMALTU
		/// </summary>
		[DxfCodeValue(273)]
		public LinearUnitFormat AlternateUnitFormat { get; set; }

		/// <summary>
		/// DIMALTTD
		/// </summary>
		[DxfCodeValue(274)]
		public short AlternateUnitToleranceDecimalPlaces { get; set; }

		/// <summary>
		/// DIMSCALE
		/// </summary>
		[DxfCodeValue(40)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// DIMASZ
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
		/// DIMEXO
		/// </summary>
		[DxfCodeValue(42)]
		public double ExtensionLineOffset { get; set; }

		/// <summary>
		/// DIMDLI
		/// </summary>
		[DxfCodeValue(43)]
		public double DimensionLineIncrement { get; set; }

		/// <summary>
		/// DIMEXE
		/// </summary>
		[DxfCodeValue(44)]
		public double ExtensionLineExtension { get; set; }

		/// <summary>
		/// DIMRND
		/// </summary>
		[DxfCodeValue(45)]
		public double Rounding { get; set; }

		/// <summary>
		/// DIMDLE
		/// </summary>
		[DxfCodeValue(46)]
		public double DimensionLineExtension { get; set; }

		/// <summary>
		/// DIMTP
		/// </summary>
		[DxfCodeValue(47)]
		public double PlusTolerance { get; set; }

		/// <summary>
		/// DIMTM
		/// </summary>
		[DxfCodeValue(48)]
		public double MinusTolerance { get; set; }

		/// <summary>
		/// DIMFXL
		/// </summary>
		[DxfCodeValue(49)]
		public double FixedExtensionLineLength { get; set; }

		/// <summary>
		/// DIMJOGANG
		/// </summary>
		[DxfCodeValue(50)]
		public double JoggedRadiusDimensionTransverseSegmentAngle { get; set; }

		/// <summary>
		/// DIMTFILL
		/// </summary>
		[DxfCodeValue(69)]
		public DimensionTextBackgroundFillMode TextBackgroundFillMode { get; set; }

		/// <summary>
		/// DIMTFILLCLR
		/// </summary>
		//[DxfCodeValue(70)]	//No present in the dxf documentation
		public Color TextBackgroundColor { get; set; }

		/// <summary>
		/// DIMAZIN
		/// </summary>
		[DxfCodeValue(79)]
		public ZeroHandling AngularZeroHandling { get; set; }

		/// <summary>
		/// DIMARCSYM
		/// </summary>
		[DxfCodeValue(90)]
		public ArcLengthSymbolPosition ArcLengthSymbolPosition { get; set; }

		/// <summary>
		/// DIMTXT
		/// </summary>
		[DxfCodeValue(140)]
		public double TextHeight { get; set; }

		/// <summary>
		/// DIMCEN
		/// </summary>
		[DxfCodeValue(141)]
		public double CenterMarkSize { get; set; }

		/// <summary>
		/// DIMTSZ
		/// </summary>
		[DxfCodeValue(142)]
		public double TickSize { get; set; }

		/// <summary>
		/// DIMALTF
		/// </summary>
		[DxfCodeValue(143)]
		public double AlternateUnitScaleFactor { get; set; }

		/// <summary>
		/// DIMLFAC
		/// </summary>
		[DxfCodeValue(144)]
		public double LinearScaleFactor { get; set; }

		/// <summary>
		/// DIMTVP
		/// </summary>
		[DxfCodeValue(145)]
		public double TextVerticalPosition { get; set; }

		/// <summary>
		/// DIMTFAC
		/// </summary>
		[DxfCodeValue(146)]
		public double ToleranceScaleFactor { get; set; }

		/// <summary>
		/// DIMGAP
		/// </summary>
		[DxfCodeValue(147)]
		public double DimensionLineGap { get; set; }

		/// <summary>
		/// DIMALTRND
		/// </summary>
		[DxfCodeValue(148)]
		public double AlternateUnitRounding { get; set; }

		/// <summary>
		/// DIMCLRD
		/// </summary>
		[DxfCodeValue(176)]
		public Color DimensionLineColor { get; set; }

		/// <summary>
		/// DIMCLRE
		/// </summary>
		[DxfCodeValue(177)]
		public Color ExtensionLineColor { get; set; }

		/// <summary>
		/// DIMCLRT
		/// </summary>
		[DxfCodeValue(178)]
		public Color TextColor { get; set; }

		/// <summary>
		/// DIMFRAC
		/// </summary>
		[DxfCodeValue(276)]
		public FractionFormat FractionFormat { get; set; }

		/// <summary>
		/// DIMLUNIT
		/// </summary>
		[DxfCodeValue(277)]
		public LinearUnitFormat LinearUnitFormat { get; set; }

		/// <summary>
		/// DIMDSEP
		/// </summary>
		[DxfCodeValue(278)]
		public char DecimalSeparator { get; set; } = '.';

		/// <summary>
		/// DIMTMOVE
		/// </summary>
		[DxfCodeValue(279)]
		public TextMovement TextMovement { get; set; }

		/// <summary>
		/// DIMFXLON
		/// </summary>
		[DxfCodeValue(290)]
		public bool IsExtensionLineLengthFixed { get; set; }

		/// <summary>
		/// DIMTXTDIRECTION
		/// </summary>
		[DxfCodeValue(295)]
		public TextDirection TextDirection { get; set; }

		public double AltMzf { get; set; }

		public double Mzf { get; set; }

		/// <summary>
		/// DIMLWD
		/// </summary>
		[DxfCodeValue(371)]
		public LineweightType DimensionLineWeight { get; set; }

		/// <summary>
		/// DIMLWE
		/// </summary>
		[DxfCodeValue(372)]
		public LineweightType ExtensionLineWeight { get; set; }

		public string AltMzs { get; set; }
		public string Mzs { get; set; }

		//289	DIMATFIT
		public short DimensionTextArrowFit { get; set; }

		/// <summary>
		/// DIMTXSTY
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public TextStyle Style { get; set; } = TextStyle.Default;

		/// <summary>
		/// Arrowhead block for leaders
		/// </summary>
		/// <remarks>
		/// DIMLDRBLK
		/// </remarks>
		[DxfCodeValue(341)]
		public Block LeaderArrow { get; set; }

		/// <summary>
		/// Arrow block for this style
		/// </summary>
		/// <remarks>
		/// DIMBLK
		/// </remarks>
		[DxfCodeValue(342)]
		public Block ArrowBlock { get; set; }

		//5	DIMBLK(obsolete, now object ID)

		//6	DIMBLK1(obsolete, now object ID)

		//7	DIMBLK2(obsolete, now object ID)

		/// <summary>
		/// Arrowhead block for the first end of the dimension line
		/// </summary>
		/// <remarks>
		/// DIMBLK1
		/// </remarks>
		[DxfCodeValue(343)]
		public Block DimArrow1 { get; set; }

		/// <summary>
		/// Arrowhead block for the second end of the dimension line
		/// </summary>
		/// <remarks>
		/// DIMBLK1
		/// </remarks>
		[DxfCodeValue(344)]
		public Block DimArrow2 { get; set; }

		private double _arrowSize = 0.18;

		internal DimensionStyle() : base() { }

		public DimensionStyle(string name) : base(name) { }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			DimensionStyle clone = new DimensionStyle(this.Name);
			clone.Style = (TextStyle)this.Style?.Clone();
			clone.LeaderArrow = (Block)this.LeaderArrow?.Clone();
			clone.ArrowBlock = (Block)this.ArrowBlock?.Clone();
			clone.DimArrow1 = (Block)this.DimArrow1?.Clone();
			clone.DimArrow2 = (Block)this.DimArrow2?.Clone();
			return clone;
		}
	}
}
