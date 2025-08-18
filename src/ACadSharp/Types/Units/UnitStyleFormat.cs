using ACadSharp.Tables;
using CSMath;
using System;
using System.Globalization;
using System.Text;

namespace ACadSharp.Types.Units
{
	/// <summary>
	/// Represents the parameters to convert linear and angular units to its string representation.
	/// </summary>
	public class UnitStyleFormat
	{
		/// <summary>
		/// Gets or sets the number of decimal places for angular units.
		/// </summary>
		public short AngularDecimalPlaces
		{
			get { return this._angularDecimalPlaces; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "The number of decimal places must be equals or greater than zero.");
				}
				this._angularDecimalPlaces = value;
			}
		}

		/// <summary>
		/// Gets or sets the suppression of zeros in the angular values.
		/// </summary>
		public ZeroHandling AngularZeroHandling { get; set; } = ZeroHandling.SuppressDecimalTrailingZeroes;

		/// <summary>
		/// Gets or set the decimal separator.
		/// </summary>
		public string DecimalSeparator { get; set; }

		/// <summary>
		/// Gets or set the angle degrees symbol.
		/// </summary>
		public string DegreesSymbol { get; set; }

		/// <summary>
		/// Gets or sets the separator between feet and inches.
		/// </summary>
		public string FeetInchesSeparator { get; set; }

		/// <summary>
		/// Gets or set the feet symbol.
		/// </summary>
		public string FeetSymbol { get; set; }

		/// <summary>
		/// Gets or sets the scale of fractions relative to dimension text height.
		/// </summary>
		public double FractionHeightScale
		{
			get { return this._fractionHeightScale; }
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "The fraction height scale must be greater than zero.");
				}
				this._fractionHeightScale = value;
			}
		}

		/// <summary>
		/// Gets or sets the fraction format for architectural or fractional units.
		/// </summary>
		/// <remarks>
		/// Horizontal stacking<br/>
		/// Diagonal stacking<br/>
		/// Not stacked (for example, 1/2)
		/// </remarks>
		public FractionFormat FractionType { get; set; }

		/// <summary>
		/// Gets or set the angle gradians symbol.
		/// </summary>
		public string GradiansSymbol { get; set; }

		/// <summary>
		/// Gets or set the inches symbol.
		/// </summary>
		public string InchesSymbol { get; set; }

		/// <summary>
		/// Gets or sets the number of decimal places for linear units.
		/// </summary>
		/// <remarks>
		/// For architectural and fractional the precision used for the minimum fraction is 1/2^LinearDecimalPlaces.
		/// </remarks>
		public short LinearDecimalPlaces
		{
			get { return this._linearDecimalPlaces; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "The number of decimal places must be equals or greater than zero.");
				}
				this._linearDecimalPlaces = value;
			}
		}

		/// <summary>
		/// Gets or sets the suppression of zeros in the linear values.
		/// </summary>
		public ZeroHandling LinearZeroHandling { get; set; } = ZeroHandling.SuppressDecimalTrailingZeroes;

		/// <summary>
		/// Gets or set the angle minutes symbol.
		/// </summary>
		public string MinutesSymbol { get; set; }

		/// <summary>
		/// Gets or set the angle radians symbol.
		/// </summary>
		public string RadiansSymbol { get; set; }

		/// <summary>
		/// Gets or set the angle seconds symbol.
		/// </summary>
		public string SecondsSymbol { get; set; }

		/// <summary>
		/// Suppresses leading zeros in angular decimal dimensions (for example, 0.5000 becomes .5000).
		/// </summary>
		public bool SuppressAngularLeadingZeros
		{
			get
			{
				return this.LinearZeroHandling == ZeroHandling.SuppressDecimalLeadingZeroes
					|| this.LinearZeroHandling == ZeroHandling.SuppressDecimalLeadingAndTrailingZeroes;
			}
		}

		/// <summary>
		/// Suppresses trailing zeros in angular decimal dimensions (for example, 12.5000 becomes 12.5).
		/// </summary>
		public bool SuppressAngularTrailingZeros
		{
			get
			{
				return this.LinearZeroHandling == ZeroHandling.SuppressDecimalTrailingZeroes
					|| this.LinearZeroHandling == ZeroHandling.SuppressDecimalLeadingAndTrailingZeroes;
			}
		}

		/// <summary>
		/// Suppresses leading zeros in linear decimal dimensions (for example, 0.5000 becomes .5000).
		/// </summary>
		public bool SuppressLinearLeadingZeros
		{
			get
			{
				return this.LinearZeroHandling == ZeroHandling.SuppressDecimalLeadingZeroes
					|| this.LinearZeroHandling == ZeroHandling.SuppressDecimalLeadingAndTrailingZeroes;
			}
		}

		/// <summary>
		/// Suppresses trailing zeros in linear decimal dimensions (for example, 12.5000 becomes 12.5).
		/// </summary>
		public bool SuppressLinearTrailingZeros
		{
			get
			{
				return this.LinearZeroHandling == ZeroHandling.SuppressDecimalTrailingZeroes
					|| this.LinearZeroHandling == ZeroHandling.SuppressDecimalLeadingAndTrailingZeroes;
			}
		}

		/// <summary>
		/// Suppresses zero feet in architectural dimensions.
		/// </summary>
		public bool SuppressZeroFeet
		{
			get
			{
				return this.LinearZeroHandling == ZeroHandling.SuppressZeroFeetAndInches
					|| this.LinearZeroHandling == ZeroHandling.SuppressZeroFeetShowZeroInches;
			}
		}

		/// <summary>
		/// Suppresses zero inches in architectural dimensions.
		/// </summary>
		public bool SuppressZeroInches
		{
			get
			{
				return this.LinearZeroHandling == ZeroHandling.SuppressZeroFeetAndInches
					|| this.LinearZeroHandling == ZeroHandling.ShowZeroFeetSuppressZeroInches;
			}
		}

		private short _angularDecimalPlaces;

		private double _fractionHeightScale;

		private short _linearDecimalPlaces;

		/// <summary>
		/// Initializes a new instance of the <c>UnitStyleFormat</c> class.
		/// </summary>
		public UnitStyleFormat()
		{
			this._linearDecimalPlaces = 2;
			this._angularDecimalPlaces = 0;
			this.DecimalSeparator = ".";
			this.FeetInchesSeparator = "-";
			this.DegreesSymbol = "°";
			this.MinutesSymbol = "\'";
			this.SecondsSymbol = "\"";
			this.RadiansSymbol = "r";
			this.GradiansSymbol = "g";
			this.FeetSymbol = "\'";
			this.InchesSymbol = "\"";
			this._fractionHeightScale = 1.0;
			this.FractionType = FractionFormat.Horizontal;
		}

		/// <summary>
		/// Get the string format based on the zero handling of the style.
		/// </summary>
		/// <param name="isAngular"></param>
		/// <returns></returns>
		public string GetZeroHandlingFormat(bool isAngular = false)
		{
			short decimalPlaces = this.LinearDecimalPlaces;
			ZeroHandling handling;

			if (isAngular)
			{
				handling = this.AngularZeroHandling;
			}
			else
			{
				handling = this.LinearZeroHandling;
			}

			char leading = handling == ZeroHandling.SuppressDecimalLeadingZeroes
				   || handling == ZeroHandling.SuppressDecimalLeadingAndTrailingZeroes ?
				   '#' : '0';

			char trailing = handling == ZeroHandling.SuppressDecimalTrailingZeroes
			   || handling == ZeroHandling.SuppressDecimalLeadingAndTrailingZeroes ?
			   '#' : '0';

			StringBuilder zeroes = new();

			zeroes.Append(leading);
			zeroes.Append(".");

			for (int i = 0; i < decimalPlaces; i++)
			{
				zeroes.Append(trailing);
			}

			return zeroes.ToString();
		}

		/// <summary>
		/// Converts a value into its feet and fractional inches string representation.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>A string that represents the value in feet and fractional inches.</returns>
		public string ToArchitectural(double value)
		{
			int feet = (int)(value / 12);
			double inchesDec = value - 12 * feet;
			int inches = (int)inchesDec;

			if (MathHelper.IsZero(inchesDec))
			{
				if (feet == 0)
				{
					if (this.SuppressZeroFeet)
					{
						return string.Format("0{0}", this.InchesSymbol);
					}

					if (this.SuppressZeroInches)
					{
						return string.Format("0{0}", this.FeetSymbol);
					}

					return string.Format("0{0}{1}0{2}", this.FeetSymbol, this.FeetInchesSeparator, this.InchesSymbol);
				}

				if (this.SuppressZeroInches)
				{
					return string.Format("{0}{1}", feet, this.FeetSymbol);
				}

				return string.Format("{0}{1}{2}0{3}", feet, this.FeetSymbol, this.FeetInchesSeparator, this.InchesSymbol);
			}

			getFraction(inchesDec, (short)Math.Pow(2, this.LinearDecimalPlaces), out int numerator, out int denominator);

			if (numerator == 0)
			{
				if (inches == 0)
				{
					if (feet == 0)
					{
						if (this.SuppressZeroFeet)
						{
							return string.Format("0{0}", this.InchesSymbol);
						}

						if (this.SuppressZeroInches)
						{
							return string.Format("0{0}", this.FeetSymbol);
						}

						return string.Format("0{0}{1}0{2}", this.FeetSymbol, this.FeetInchesSeparator, this.InchesSymbol);
					}

					if (this.SuppressZeroInches)
					{
						return string.Format("{0}{1}", feet, this.FeetSymbol);
					}

					return string.Format("{0}{1}{2}0{3}", feet, this.FeetSymbol, this.FeetInchesSeparator, this.InchesSymbol);
				}
				if (feet == 0)
				{
					if (this.SuppressZeroFeet)
					{
						return string.Format("{0}{1}", inches, this.InchesSymbol);
					}

					return string.Format("0{0}{1}{2}{3}", this.FeetSymbol, this.FeetInchesSeparator, inches, this.InchesSymbol);
				}

				return string.Format("{0}{1}{2}{3}{4}", feet, this.FeetSymbol, this.FeetInchesSeparator, inches, this.InchesSymbol);
			}

			string text = string.Empty;
			string feetStr;
			if (this.SuppressZeroFeet && feet == 0)
			{
				feetStr = string.Empty;
			}
			else
			{
				feetStr = feet + this.FeetSymbol + this.FeetInchesSeparator;
			}

			switch (this.FractionType)
			{
				case FractionFormat.Diagonal:
					text = $"\\A1;{feetStr}{inches}{{\\H{this.FractionHeightScale}x;\\S{numerator}#{denominator};}}{this.InchesSymbol}";
					break;

				case FractionFormat.Horizontal:
					text = $"\\A1;{feetStr}{inches}{{\\H{this.FractionHeightScale}x;\\S{numerator}/{denominator};}}{this.InchesSymbol}";
					break;

				case FractionFormat.None:
					text = $"{feetStr}{inches} {numerator}/{denominator}{this.InchesSymbol}";
					break;
			}
			return text;
		}

		/// <summary>
		/// Converts a value into its decimal string representation.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isAngular"></param>
		/// <returns>A string that represents the value in decimal units.</returns>
		public string ToDecimal(double value, bool isAngular = false)
		{
			NumberFormatInfo numberFormat = new NumberFormatInfo
			{
				NumberDecimalSeparator = this.DecimalSeparator
			};

			return value.ToString(this.GetZeroHandlingFormat(isAngular), numberFormat);
		}

		/// <summary>
		/// Converts an angle value in degrees into its degrees, minutes and seconds string representation.
		/// </summary>
		/// <param name="angle">Angle value in radians.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public string ToDegreesMinutesSeconds(double angle)
		{
			double degrees = MathHelper.RadToDeg(angle);
			double minutes = (degrees - (int)degrees) * 60;
			double seconds = (minutes - (int)minutes) * 60;

			NumberFormatInfo numberFormat = new NumberFormatInfo
			{
				NumberDecimalSeparator = this.DecimalSeparator
			};

			if (this.AngularDecimalPlaces == 0)
			{
				return string.Format(numberFormat, "{0}" + this.DegreesSymbol, (int)Math.Round(degrees, 0));
			}

			if (this.AngularDecimalPlaces == 1 || this.AngularDecimalPlaces == 2)
			{
				return string.Format(numberFormat, "{0}" + this.DegreesSymbol + "{1}" + this.MinutesSymbol, (int)degrees, (int)Math.Round(minutes, 0));
			}

			if (this.AngularDecimalPlaces == 3 || this.AngularDecimalPlaces == 4)
			{
				return string.Format(numberFormat, "{0}" + this.DegreesSymbol + "{1}" + this.MinutesSymbol + "{2}" + this.SecondsSymbol, (int)degrees, (int)minutes, (int)Math.Round(seconds, 0));
			}

			// the suppression of leading or trailing zeros is not applicable to DegreesMinutesSeconds angles format
			string f = "0." + new string('0', this.AngularDecimalPlaces - 4);
			return string.Format(numberFormat, "{0}" + this.DegreesSymbol + "{1}" + this.MinutesSymbol + "{2}" + this.SecondsSymbol, (int)degrees, (int)minutes, seconds.ToString(f, numberFormat));
		}

		/// <summary>
		/// Converts a value into its feet and decimal inches string representation.
		/// </summary>
		/// <param name="value">Must be in inches.</param>
		/// <returns>A string that represents the value in feet and decimal inches.</returns>
		public string ToEngineering(double value)
		{
			NumberFormatInfo numberFormat = new NumberFormatInfo
			{
				NumberDecimalSeparator = this.DecimalSeparator
			};
			int feet = (int)(value / 12);
			double inches = value - 12 * feet;

			if (MathHelper.IsZero(inches))
			{
				if (feet == 0)
				{
					if (this.SuppressZeroFeet)
					{
						return string.Format("0{0}", this.InchesSymbol);
					}

					if (this.SuppressZeroInches)
					{
						return string.Format("0{0}", this.FeetSymbol);
					}
					return string.Format("0{0}{1}0{2}", this.FeetSymbol, this.FeetInchesSeparator, this.InchesSymbol);
				}

				if (this.SuppressZeroInches)
				{
					return string.Format("{0}{1}", feet, this.FeetSymbol);
				}

				return string.Format("{0}{1}{2}0{3}", feet, this.FeetSymbol, this.FeetInchesSeparator, this.InchesSymbol);
			}

			string inchesDec = inches.ToString(this.GetZeroHandlingFormat(), numberFormat);
			if (feet == 0)
			{
				if (this.SuppressZeroFeet)
				{
					return string.Format("{0}{1}", inches, this.InchesSymbol);
				}

				return string.Format("0{0}{1}{2}{3}", this.FeetSymbol, this.FeetInchesSeparator, inchesDec, this.InchesSymbol);
			}
			return string.Format("{0}{1}{2}{3}{4}", feet, this.FeetSymbol, this.FeetInchesSeparator, inchesDec, this.InchesSymbol);
		}

		/// <summary>
		/// Converts a value into its fractional string representation.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>A string that represents the value in fractional units.</returns>
		public string ToFractional(double value)
		{
			int num = (int)value;
			getFraction(value, (short)Math.Pow(2, this.LinearDecimalPlaces), out int numerator, out int denominator);
			if (numerator == 0)
			{
				return string.Format("{0}", (int)value);
			}

			string text = string.Empty;
			switch (this.FractionType)
			{
				case FractionFormat.Diagonal:
					text = $"\\A1;{num}{{\\H{this.FractionHeightScale}x;\\S{numerator}#{denominator};}}";
					break;

				case FractionFormat.Horizontal:
					text = $"\\A1;{num}{{\\H{this.FractionHeightScale}x;\\S{numerator}/{denominator};}}";
					break;

				case FractionFormat.None:
					string prefix = num == 0 ? string.Empty : $"{num.ToString()} ";
					text = $"{prefix}{numerator}/{denominator}";
					break;
			}
			return text;
		}

		/// <summary>
		/// Converts an angle value in radians into its gradians string representation.
		/// </summary>
		/// <param name="angle">The angle value in radians.</param>
		/// <returns>A string that represents the angle in gradians.</returns>
		public string ToGradians(double angle)
		{
			NumberFormatInfo numberFormat = new NumberFormatInfo
			{
				NumberDecimalSeparator = this.DecimalSeparator
			};

			return (MathHelper.RadToGrad(angle)).ToString(GetZeroHandlingFormat(true), numberFormat) + this.GradiansSymbol;
		}

		/// <summary>
		/// Converts an angle value in radians into its string representation.
		/// </summary>
		/// <param name="angle">The angle value in radians.</param>
		/// <returns>A string that represents the angle in radians.</returns>
		public string ToRadians(double angle)
		{
			NumberFormatInfo numberFormat = new NumberFormatInfo
			{
				NumberDecimalSeparator = this.DecimalSeparator
			};
			return angle.ToString(GetZeroHandlingFormat(true), numberFormat) + this.RadiansSymbol;
		}

		/// <summary>
		/// Converts a value into its scientific string representation.
		/// </summary>
		/// <param name="value">The length value.</param>
		/// <returns>A string that represents the length in scientific units.</returns>
		public string ToScientific(double value)
		{
			return value.ToString($"{this.GetZeroHandlingFormat()}E+00");
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