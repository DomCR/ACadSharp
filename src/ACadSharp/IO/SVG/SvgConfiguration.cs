using System;

namespace ACadSharp.IO
{
	/// <summary>
	/// Configuration for writing SVG files.
	/// </summary>
	public class SvgConfiguration : CadWriterConfiguration
	{
		public double LineWeightRatio { get; set; } = 5;

		public double PointRadius { get; set; } = 0.5;

		public const double PixelSize = 3.7795275591;

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