using ACadSharp.Objects;
using ACadSharp.Types.Units;
using CSMath;
using System;
using System.ComponentModel;
using System.Globalization;

namespace ACadSharp.IO.SVG
{
	internal static class SvgConverter
	{
		public static string ToSvg(this double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToSvg(this double value, UnitsType units)
		{
			string unitSufix = string.Empty;
			switch (units)
			{
				case UnitsType.Centimeters:
					unitSufix = "cm";
					break;
				case UnitsType.Millimeters:
					unitSufix = "mm";
					break;
				case UnitsType.Inches:
					unitSufix = "in";
					break;
			}

			return $"{value.ToString(CultureInfo.InvariantCulture)}{unitSufix}";
		}

		public static string ToSvg<T>(this T vector)
			where T : IVector
		{
			return $"{vector[0].ToSvg()},{vector[1].ToSvg()}";
		}

		public static string ToSvg<T>(this T vector, UnitsType units)
			where T : IVector
		{
			return $"{vector[0].ToSvg(units)},{vector[1].ToSvg(units)}";
		}

		public static double ToPixelSize(this double value, UnitsType units)
		{
			switch (units)
			{
				case UnitsType.Inches:
					return value * 96;
				case UnitsType.Millimeters:
					return value * 96 / 25.4;
				case UnitsType.Unitless:
					return value;
				default:
					throw new InvalidEnumArgumentException(nameof(units), (int)units, units.GetType());
			}
		}

		public static T ToPixelSize<T>(this T value, UnitsType units)
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
