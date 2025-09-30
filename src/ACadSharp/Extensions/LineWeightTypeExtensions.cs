using System;

namespace ACadSharp.Extensions
{
	public static class LineWeightTypeExtensions
	{
		public static double GetLineWeightValue(this LineWeightType lineWeight)
		{
			double value = Math.Abs((double)lineWeight);

			switch (lineWeight)
			{
				case LineWeightType.W0:
					return 0.001;
			}

			return value / 100;
		}
	}
}
