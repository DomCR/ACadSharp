using System;

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
		public double LineWeightRatio { get; set; } = 5;

		/// <summary>
		/// Radius applied for the points.
		/// </summary>
		/// <remarks>
		/// In svg the points will be drawn as circles.
		/// </remarks>
		public double PointRadius { get; set; } = 0.5;

		/// <summary>
		/// Pixel size reference for the configuration.
		/// </summary>
		public const double PixelSize = 3.7795275591;

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
				value = 1;
			}

			return Math.Abs(value) / this.LineWeightRatio;
		}
	}
}