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
		[DxfCodeValue(46, 47)]
		public XY PlotOrigin { get; set; }

		/// <summary>
		/// Plot lower-left window corner
		/// </summary>
		[DxfCodeValue(48, 49)]
		public XY WindowLowerLeft { get; set; }

		/// <summary>
		/// Plot upper-left window corner
		/// </summary>
		[DxfCodeValue(140, 141)]
		public XY WindowUpperLeft { get; set; }

		/// <summary>
		/// Gets the scale factor.
		/// </summary>
		public double PrintScale
		{
			get { return NumeratorScale / DenominatorScale; }
		}

		/// <summary>
		/// Numerator of custom print scale: real world(paper) units
		/// </summary>
		[DxfCodeValue(142)]
		public double NumeratorScale { get; set; }

		/// <summary>
		/// Denominator of custom print scale: drawing units
		/// </summary>
		[DxfCodeValue(143)]
		public double DenominatorScale { get; set; }

		/// <summary>
		/// Plot layout flags
		/// </summary>
		[DxfCodeValue(70)]
		public PlotFlags Flags { get; set; }

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
			get { return _shadePlotDPI; }
			set
			{
				if (value < 100 || value > 32767)
					throw new ArgumentOutOfRangeException(nameof(value), value, "The valid shade plot DPI values range from 100 to 23767.");
				_shadePlotDPI = value;
			}
		}
		private short _shadePlotDPI;

		/// <summary>
		/// A floating point scale factor that represents the standard scale value specified in code 75.
		/// </summary>
		[DxfCodeValue(147)]
		public double StandardScale { get; set; }

		/// <summary>
		/// Paper image origin
		/// </summary>
		[DxfCodeValue(148, 149)]
		public XY PaperImageOrigin { get; set; }

		/// <summary>
		///ShadePlot ID/Handle(optional)
		/// </summary>
		[DxfCodeValue(333)]
		public ulong ShadePlotIDHandle { get; set; }
	}
}
