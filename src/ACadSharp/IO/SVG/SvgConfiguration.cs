using ACadSharp.Objects;
using System;
using System.ComponentModel;

namespace ACadSharp.IO
{
	/// <summary>
	/// Configuration for writing SVG files.
	/// </summary>
	public class SvgConfiguration : CadWriterConfiguration
	{
		/// <summary>
		/// The <see cref="Entities.Entity.LineWeight"/> will be divided by this value to process the stroke-width in the svg.
		/// </summary>
		/// <remarks>
		/// The default value is 100, which matches with the line weight real value in mm.
		/// </remarks>
		public double LineWeightRatio { get; set; } = 100;

		/// <summary>
		/// Radius applied for the points.
		/// </summary>
		/// <remarks>
		/// In svg the points will be drawn as circles.
		/// </remarks>
		public double PointRadius { get; set; } = 0.1;

		/// <summary>
		/// Get the value of the stroke-width.
		/// </summary>
		/// <param name="lineweightType"></param>
		/// <returns></returns>
		public double GetLineWeightValue(LineweightType lineweightType)
		{
			double value = (double)lineweightType;
			if (lineweightType == LineweightType.W0)
			{
				value = 0.001;
			}

			return ToPixelSize(Math.Abs(value) / this.LineWeightRatio, PlotPaperUnits.Milimeters);
		}

		public static double ToPixelSize(double value, PlotPaperUnits units)
		{
			switch (units)
			{
				case PlotPaperUnits.Inches:
					return value * 96;
				case PlotPaperUnits.Milimeters:
					return value * 96 / 25.4;
				case PlotPaperUnits.Pixels:
					return value;
				default:
					throw new InvalidEnumArgumentException(nameof(units), (int)units, typeof(PlotPaperUnits));
			}
		}
	}
}