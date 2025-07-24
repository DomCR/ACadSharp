using CSMath;
using System;
using System.Globalization;

namespace ACadSharp.IO.SVG
{
	[Obsolete]
	internal static class SvgConverter
	{
		public static string SvgDouble(this double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string SvgPoint<T>(this T vector)
			where T : IVector
		{
			return $"{vector[0].SvgDouble()},{vector[1].SvgDouble()}";
		}
	}
}
