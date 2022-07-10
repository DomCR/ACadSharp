using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="PlotSettings"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectPlotSettings"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PlotSettings"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectPlotSettings)]
	[DxfSubClass(DxfSubclassMarker.PlotSettings)]
	public class PlotSettings : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.INVALID;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectPlotSettings;

		/// <summary>
		/// Page Setup name
		/// </summary>
		[DxfCodeValue(1)]
		public string PageName { get; set; }

		/// <summary>
		/// Name of system printer or plot configuration file
		/// </summary>
		[DxfCodeValue(2)]
		public string SystemPrinterName { get; set; }

		/// <summary>
		/// Paper size
		/// </summary>
		[DxfCodeValue(4)]
		public string PaperSize { get; set; }

		/// <summary>
		/// Plot view name
		/// </summary>
		[DxfCodeValue(6)]
		public string PlotViewName { get; set; }

		/// <summary>
		/// Gets or set the size, in millimeters, of unprintable margins of paper
		/// </summary>
		[DxfCodeValue(40, 41, 42, 43)]
		public PaperMargin UnprintableMargin { get; set; }

		/// <summary>
		/// Physical paper width in millimeters
		/// </summary>
		[DxfCodeValue(44)]
		public double PaperWidth { get; set; }

		/// <summary>
		/// Physical paper height in millimeters
		/// </summary>
		[DxfCodeValue(45)]
		public double PaperHeight { get; set; }

		/// <summary>
		///Plot origin in millimeters
		/// </summary>
		public XY PlotOrigin { get; set; }

		/// <summary>
		/// Plot origin: X value of origin offset in millimeters
		/// </summary>
		[DxfCodeValue(46)]
		public double PlotOriginX { get; set; }

		/// <summary>
		/// Plot origin: Y value of origin offset in millimeters
		/// </summary>
		[DxfCodeValue(47)]
		public double PlotOriginY { get; set; }

		/// <summary>
		/// Plot lower-left window corner
		/// </summary>
		public XY WindowLowerLeft { get; set; }

		/// <summary>
		/// Plot window area: X value of lower-left window corner
		/// </summary>
		[DxfCodeValue(48)]
		public double WindowLowerLeftX { get; set; }

		/// <summary>
		/// Plot window area: Y value of upper-right window corner
		/// </summary>
		[DxfCodeValue(49)]
		public double WindowLowerLeftY { get; set; }

		/// <summary>
		/// Plot upper-left window corner
		/// </summary>
		public XY WindowUpperLeft { get; set; }

		/// <summary>
		/// Plot window area: X value of lower-left window corner
		/// </summary>
		[DxfCodeValue(140)]
		public double WindowUpperLeftX { get; set; }

		/// <summary>
		/// Plot window area: Y value of upper-right window corner
		/// </summary>
		[DxfCodeValue(141)]
		public double WindowUpperLeftY { get; set; }

		/// <summary>
		/// Gets the scale factor.
		/// </summary>
		public double PrintScale
		{
			get { return this.NumeratorScale / this.DenominatorScale; }
		}

		/// <summary>
		/// Numerator of custom print scale: real world(paper) units
		/// </summary>
		[DxfCodeValue(142)]
		public double NumeratorScale
		{
			get { return this._numeratorScale; }
			set
			{
				if (value <= 0.0)
				{
					throw new ArgumentOutOfRangeException(nameof(this.NumeratorScale), value, "Value must be greater than zero");
				}

				this._numeratorScale = value;
			}
		}

		/// <summary>
		/// Denominator of custom print scale: drawing units
		/// </summary>
		[DxfCodeValue(143)]
		public double DenominatorScale
		{
			get { return this._denominatorScale; }
			set
			{
				if (value <= 0.0)
				{
					throw new ArgumentOutOfRangeException(nameof(this.DenominatorScale), value, "Value must be greater than zero");
				}

				this._denominatorScale = value;
			}
		}

		/// <summary>
		/// Plot layout flags
		/// </summary>
		[DxfCodeValue(70)]
		public PlotFlags PlotFlags { get; set; }

		/// <summary>
		/// Plot paper units.
		/// </summary>
		[DxfCodeValue(72)]
		public PlotPaperUnits PaperUnits { get; set; }

		/// <summary>
		/// Plot paper units.
		/// </summary>
		[DxfCodeValue(73)]
		public PlotRotation PaperRotation { get; set; }

		/// <summary>
		/// Portion of paper space to output to the media
		/// </summary>
		[DxfCodeValue(74)]
		public PlotType PlotType { get; set; }

		/// <summary>
		/// Current style sheet
		/// </summary>
		[DxfCodeValue(7)]
		public string StyleSheet { get; set; }

		/// <summary>
		/// Standard scale type
		/// </summary>
		[DxfCodeValue(75)]
		public ScaledType ScaledFit { get; set; }

		/// <summary>
		/// Plot shade mode.
		/// </summary>
		[DxfCodeValue(76)]
		public ShadePlotMode ShadePlotMode { get; set; }

		/// <summary>
		/// Plot resolution.
		/// </summary>
		[DxfCodeValue(77)]
		public ShadePlotResolutionMode ShadePlotResolutionMode { get; set; }

		/// <summary>
		/// Shade plot custom DPI.
		/// </summary>
		/// <remarks>
		/// Only applied when the ShadePlot resolution level is set to 5 (Custom)
		/// </remarks>
		/// <value>
		/// Valid shade plot DPI values range from 100 to 23767.
		/// </value>
		[DxfCodeValue(78)]
		public short ShadePlotDPI
		{
			get { return this._shadePlotDPI; }
			set
			{
				if (value < 100 || value > 32767)
					throw new ArgumentOutOfRangeException(nameof(value), value, "The valid shade plot DPI values range from 100 to 23767.");
				this._shadePlotDPI = value;
			}
		}

		/// <summary>
		/// A floating point scale factor that represents the standard scale value specified in code 75.
		/// </summary>
		[DxfCodeValue(147)]
		public double StandardScale { get; set; }

		/// <summary>
		/// Paper image origin
		/// </summary>
		public XY PaperImageOrigin { get; set; }

		/// <summary>
		/// Paper image origin: X value
		/// </summary>
		[DxfCodeValue(148)]
		public double PaperImageOriginX { get; set; }

		/// <summary>
		/// Paper image origin: Y value
		/// </summary>
		[DxfCodeValue(149)]
		public double PaperImageOriginY { get; set; }

		/// <summary>
		///ShadePlot ID/Handle(optional)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Ignored, 333)]
		public ulong ShadePlotIDHandle { get; set; }

		private short _shadePlotDPI = 300;

		private double _numeratorScale = 1.0d;

		private double _denominatorScale = 1.0d;
	}
}
