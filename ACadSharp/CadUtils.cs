using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	internal class CadUtils
	{
		/// <summary>
		/// Get the version of the autocad drawing by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static ACadVersion GetVersionFromName(string name)
		{
			//Modify the format of the name
			string vname = name.Replace('.', '_').ToUpper();

			if (Enum.TryParse(vname, out ACadVersion version))
				return version;
			else
				return ACadVersion.Unknown;
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
	}
}
