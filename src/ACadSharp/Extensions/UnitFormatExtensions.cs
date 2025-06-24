using ACadSharp.Tables;
using System;

namespace ACadSharp.Extensions
{
	public class UnitFormatExtensions
	{
		public static string ToFractional(double value, FractionFormat fractionFormat, double linearDecimalPlaces, double fractionHeightScale)
		{
			int num = (int)value;
			getFraction(value, (short)Math.Pow(2, linearDecimalPlaces), out int numerator, out int denominator);
			if (numerator == 0)
			{
				return string.Format("{0}", (int)value);
			}

			string text = string.Empty;
			switch (fractionFormat)
			{
				case FractionFormat.Diagonal:
					text = $"\\A1;{num}{{\\H{fractionHeightScale}x;\\S{numerator}#{denominator};}}";
					break;
				case FractionFormat.Horizontal:
					text = $"\\A1;{num}{{\\H{fractionHeightScale}x;\\S{numerator}/{denominator};}}";
					break;
				case FractionFormat.None:
					text = num + " " + numerator + "/" + denominator;
					break;
			}

			return text;
		}

		private static void getFraction(double number, int precision, out int numerator, out int denominator)
		{
			numerator = Convert.ToInt32((number - (int)number) * precision);
			int commonFactor = getGCD(numerator, precision);
			if (commonFactor <= 0)
			{
				commonFactor = 1;
			}
			numerator = numerator / commonFactor;
			denominator = precision / commonFactor;
		}

		private static int getGCD(int number1, int number2)
		{
			int a = number1;
			int b = number2;
			while (b != 0)
			{
				int count = a % b;
				a = b;
				b = count;
			}
			return a;
		}
	}
}
