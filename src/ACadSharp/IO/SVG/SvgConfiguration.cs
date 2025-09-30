using ACadSharp.Types.Units;
using System;

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

		public int ArcPoints { get; set; } = 256;

		/// <summary>
		/// Get the value of the stroke-width in mm.
		/// </summary>
		/// <param name="lineWeight"></param>
		/// <param name="units"></param>
		/// <returns></returns>
		public double GetLineWeightValue(LineWeightType lineWeight, UnitsType units)
		{
			double value = Math.Abs((double)lineWeight);

			if (units == UnitsType.Unitless)
			{
				return value / this.LineWeightRatio;
			}

			switch (lineWeight)
			{
				case LineWeightType.Default:
					return this.DefaultLineWeight;
				case LineWeightType.W0:
					return 0.001;
			}

			return value / 100;
		}
	}
}