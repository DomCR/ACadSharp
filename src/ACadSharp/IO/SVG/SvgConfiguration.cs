using ACadSharp.Objects;
using ACadSharp.Types.Units;
using CSMath;
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
		/// The <see cref="LineWeightType"/> will be divided by this value to process the stroke-width in the svg when the units are <see cref="UnitsType.Unitless"/>.
		/// </summary>
		/// <remarks>
		/// The default value is 100, which matches with the line weight real value in mm.
		/// </remarks>
		public double LineWeightRatio { get; set; } = 100;

		/// <summary>
		/// Weight value for the <see cref="LineWeightType.Default"/>. 
		/// </summary>
		/// <value>
		/// Value must be in mm.
		/// </value>
		public double DefaultLineWeight { get; set; } = 0.01;

		/// <summary>
		/// Radius applied for the points.
		/// </summary>
		/// <remarks>
		/// In svg the points will be drawn as circles.
		/// </remarks>
		public double PointRadius { get; set; } = 0.1;

		/// <summary>
		/// Get the value of the stroke-width in mm.
		/// </summary>
		/// <param name="lineweightType"></param>
		/// <param name="units"></param>
		/// <returns></returns>
		public double GetLineWeightValue(LineWeightType lineweightType, UnitsType units)
		{
			double value = Math.Abs((double)lineweightType);

			if (units == UnitsType.Unitless)
			{
				return value / this.LineWeightRatio;
			}

			switch (lineweightType)
			{
				case LineWeightType.Default:
					return this.DefaultLineWeight;
				case LineWeightType.W0:
					return 0.001;
			}

			return value / 100;
		}

		public static double ToPixelSize(double value, PlotPaperUnits units)
		{
			switch (units)
			{
				case PlotPaperUnits.Inches:
					return value * 96;
				case PlotPaperUnits.Millimeters:
					return value * 96 / 25.4;
				case PlotPaperUnits.Pixels:
					return value;
				default:
					throw new InvalidEnumArgumentException(nameof(units), (int)units, typeof(PlotPaperUnits));
			}
		}

		public static T ToPixelSize<T>(T value, PlotPaperUnits units)
			where T : IVector
		{
			for (int i = 0; i < value.Dimension; i++)
			{
				value[i] = ToPixelSize(value[i], units);
			}

			return value;
		}
	}
}