using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp
{
	internal static class CadUtils
	{
		private static readonly LineweightType[] IndexedValue = new LineweightType[]
		{
				 LineweightType.W0,
				 LineweightType.W5,
				 LineweightType.W9,
				 LineweightType.W13,
				 LineweightType.W15,
				 LineweightType.W18,
				 LineweightType.W20,
				 LineweightType.W25,
				 LineweightType.W30,
				 LineweightType.W35,
				 LineweightType.W40,
				 LineweightType.W50,
				 LineweightType.W53,
				 LineweightType.W60,
				 LineweightType.W70,
				 LineweightType.W80,
				 LineweightType.W90,
				 LineweightType.W100,
				 LineweightType.W106,
				 LineweightType.W120,
				 LineweightType.W140,
				 LineweightType.W158,
				 LineweightType.W200,
				 LineweightType.W211
		};

		private static readonly CodePage[] _pageCodes = new CodePage[]
		{
			CodePage.Unknown,
			CodePage.Usascii,
			CodePage.Iso88591,
			CodePage.Iso88592,
			CodePage.Iso88593,
			CodePage.Iso88594,
			CodePage.Iso88595,
			CodePage.Iso88596,
			CodePage.Iso88597,
			CodePage.Iso88598,
			CodePage.Iso88599,
			CodePage.Ibm437,
			CodePage.Ibm850,
			CodePage.Ibm852,
			CodePage.Ibm855,
			CodePage.Ibm857,
			CodePage.Ibm860,
			CodePage.Ibm861,
			CodePage.Ibm863,
			CodePage.Ibm864,
			CodePage.Ibm865,
			CodePage.Ibm869,
			CodePage.Shift_jis,
			CodePage.Macintosh,
			CodePage.big5,
			CodePage.Ksc5601,
			CodePage.Johab,
			CodePage.Cp866,
			CodePage.Windows1250,
			CodePage.Windows1251,
			CodePage.Windows1252,
			CodePage.Gb2312,
			CodePage.Windows1253,
			CodePage.Windows1254,
			CodePage.Windows1255,
			CodePage.Windows1256,
			CodePage.Windows1257,
			CodePage.Windows874,
			CodePage.Shift_jis,
			CodePage.Gb2312,
			CodePage.Ksc5601,
			CodePage.big5,
			CodePage.Johab,
			CodePage.Utf16,
			CodePage.Windows1258
		};

		public static LineweightType ToValue(byte b)
		{
			switch (b)
			{
				case 28:
				case 29:
					return LineweightType.ByLayer;
				case 30:
					return LineweightType.ByBlock;
				case 31:
					return LineweightType.Default;
				default:
					if (b < 0 || b >= IndexedValue.Length)
					{
						return LineweightType.Default;
					}
					return IndexedValue[b];
			}
		}

		public static byte ToIndex(LineweightType value)
		{
			byte result = 0;
			switch (value)
			{
				case LineweightType.Default:
					result = 31;
					break;
				case LineweightType.ByBlock:
					result = 30;
					break;
				case LineweightType.ByLayer:
					result = 29;
					break;
				default:
					result = (byte)Array.IndexOf(IndexedValue, value);
					if (result < 0)
					{
						result = 31;
					}
					break;
			}

			return result;
		}

		public static CodePage GetCodePage(int value)
		{
			return _pageCodes.ElementAtOrDefault(value);
		}

		public static int GetCodeIndex(CodePage code)
		{
			return _pageCodes.ToList().IndexOf(code);
		}

		public static ACadVersion GetVersionFromName(string name)
		{
			//Modify the format of the name
			string vname = name.Replace('.', '_').ToUpper();

			if (Enum.TryParse(vname, out ACadVersion version))
				return version;
			else
				return ACadVersion.Unknown;
		}

		public static string GetNameFromVersion(ACadVersion version)
		{
			return version.ToString().Replace('_', '.');
		}

		public static double ToJulianCalendar(DateTime date)
		{
			int year = date.Year;
			int month = date.Month;
			int day = date.Day;
			double hour = date.Hour;
			double minute = date.Minute;
			double second = date.Second;
			double millisecond = date.Millisecond;
			double fraction = day + hour / 24.0 + minute / 1440.0 + (second + millisecond / 1000) / 86400.0;

			if (month < 3)
			{
				year -= 1;
				month += 12;
			}

			int a = year / 100;
			int b = 2 - a + a / 4;
			int c;
			if (year < 0)
			{
				c = (int)(365.25 * year - 0.75);
			}
			else
			{
				c = (int)(365.25 * year);
			}

			int d = (int)(30.6001 * (month + 1));
			return b + c + d + 1720995 + fraction;
		}

		public static DateTime FromJulianCalendar(double date)
		{
			if (date < 1721426 || date > 5373484)
			{
				throw new ArgumentOutOfRangeException(nameof(date), "The valid values range from 1721426 and 5373484 that correspond to January 1, 1 and December 31, 9999 respectively.");
			}

			double julian = (int)date;
			double fraction = date - julian;

			int temp = (int)((julian - 1867216.25) / 36524.25);
			julian = julian + 1 + temp - (int)(temp / 4.0);

			int a = (int)julian + 1524;
			int b = (int)((a - 122.1) / 365.25);
			int c = (int)(365.25 * b);
			int d = (int)((a - c) / 30.6001);

			int months = d < 14 ? d - 1 : d - 13;
			int years = months > 2 ? b - 4716 : b - 4715;
			int days = a - c - (int)(30.6001 * d);

			int hours = (int)(fraction * 24);
			fraction -= hours / 24.0;
			int minutes = (int)(fraction * 1440);
			fraction -= minutes / 1440.0;

			double decimalSeconds = fraction * 86400;
			int seconds = (int)decimalSeconds;
			int milliseconds = (int)((decimalSeconds - seconds) * 1000);

			return new DateTime(years, months, days, hours, minutes, seconds, milliseconds);
		}

		public static TimeSpan EditingTime(double elapsed)
		{
			int days = (int)elapsed;
			double fraction = elapsed - days;

			int hours = (int)(fraction * 24);
			fraction -= hours / 24.0;

			int minutes = (int)(fraction * 1440);
			fraction -= minutes / 1440.0;

			double decimalSeconds = fraction * 86400;
			int seconds = (int)decimalSeconds;
			int milliseconds = (int)((decimalSeconds - seconds) * 1000);

			return new TimeSpan(days, hours, minutes, seconds, milliseconds);
		}

		public static void DateToJulian(DateTime date, out int jdate, out int miliseconds)
		{
			if (date < new DateTime(1, 1, 1, 12, 0, 0))
			{
				jdate = 0;
				miliseconds = 0;
				return;
			}

			date = date.AddHours(-12.0);
			int day = (int)Math.Floor((14.0 - date.Month) / 12.0);
			int year = date.Year + 4800 - day;
			int month = date.Month;
			jdate = date.Day + (int)System.Math.Floor((153.0 * (double)(month + 12 * day - 3) + 2.0) / 5.0) + 365 * year + (int)System.Math.Floor((double)year / 4.0) - (int)System.Math.Floor((double)year / 100.0) + (int)System.Math.Floor((double)year / 400.0) - 32045;
			miliseconds = date.Millisecond + date.Second * 1000 + date.Minute * 60000 + date.Hour * 3600000;
		}
	}
}
