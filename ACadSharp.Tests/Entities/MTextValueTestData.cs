#nullable enable
using System;
using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.Tests.Entities
{
	public class MTextValueTestData
	{
		/// <summary>
		/// Creates a token value with the passed parameters for it's starting state.  Used for testing.
		/// </summary>
		/// <param name="format">Current Format of the value.</param>
		/// <param name="value">String value this token contains.</param>
		public static MText.TokenValue CreateTokenValue(MText.Format format, string value)
		{
			return new MText.TokenValue(format, value.AsMemory());
		}

		/// <summary>
		/// Creates a token value with the passed parameters for it's starting state.  Used for testing.
		/// </summary>
		/// <param name="value">String value this token contains.</param>
		public static MText.TokenValue CreateTokenValue(string value)
		{
			return new MText.TokenValue(new MText.Format(), value.AsMemory());
		}

		/// <summary>
		/// Creates a format with the specified simple formats.  Testing use onle.
		/// </summary>
		/// <param name="formats">Format string to use.</param>
		public static MText.Format CreateFormat(string formats)
		{
			var format = new MText.Format();
			// Used only for testing
			if (formats.Contains("L"))
				format.IsUnderline = true;

			if (formats.Contains("O"))
				format.IsOverline = true;

			if (formats.Contains("K"))
				format.IsStrikeThrough = true;

			if (formats.Contains("l"))
				format.IsUnderline = false;

			if (formats.Contains("o"))
				format.IsOverline = false;

			if (formats.Contains("k"))
				format.IsStrikeThrough = false;

			return format;
		}

		/// <summary>
		/// Creates a fraction token with the passed parameters for it's starting state. Used for testing.
		/// </summary>
		/// <param name="numerator">Numerator to set.</param>
		/// <param name="denominator">Denominator to set.</param>
		/// <param name="divider">Divisor to set.</param>
		public static MText.TokenFraction CreateTokenFraction(string? numerator, string? denominator, MText.TokenFraction.Divider divider)
		{
			return CreateTokenFraction(new MText.Format(), numerator, denominator, divider);
		}

		/// <summary>
		/// Creates a fraction token with the passed parameters for it's starting state. Used for testing.
		/// </summary>
		/// <param name="format">Format to set.</param>
		/// <param name="numerator">Numerator to set.</param>
		/// <param name="denominator">Denominator to set.</param>
		/// <param name="divider">Divisor to set.</param>
		public static MText.TokenFraction CreateTokenFraction(MText.Format format, string? numerator, string? denominator, MText.TokenFraction.Divider divider)
		{
			return new MText.TokenFraction(format)
			{
				Numerator = new[] { numerator.AsMemory() },
				Denominator = new[] { denominator.AsMemory() },
				DividerType = divider
			};
		}

		public class FormatData
		{
			public FormatData(string input, MText.Token? expected)
				: this(new[] { input }, expected == null ? null : new[] { expected })
			{
			}

			public FormatData(string[] input, MText.Token? expected)
				: this(input, expected == null ? null : new[] { expected })
			{
			}

			public FormatData(string encoded, MText.Token[]? decoded)
				: this(new[] { encoded }, decoded, null)
			{
			}

			public FormatData(string[] encoded, MText.Token[]? decoded)
				: this(encoded, decoded, null)
			{
			}


			public FormatData(string encoded, MText.Token[]? decoded, MText.Format? format)
				: this(new[] { encoded }, decoded, format)
			{

			}

			public FormatData(string[] encoded, MText.Token[]? decoded, MText.Format? format)
			{
				this.Encoded = encoded;
				this.Decoded = decoded;
				this.Format = format;
			}

			public MText.Format? Format { get; set; }
			public string[] Encoded { get; set; }
			public MText.Token[]? Decoded { get; set; }

			public virtual bool Equals(FormatData? other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}

				if (ReferenceEquals(this, other))
				{
					return true;
				}

				return Encoded == other.Encoded
				       && Decoded?.Equals(other.Decoded) == true;
			}
		}

		public class TextData
		{
			public TextData(string encoded, string decoded)
				: this(new[] { encoded }, decoded)
			{

			}

			public TextData(string[] encoded, string decoded)
			{
				this.Encoded = encoded;
				this.Decoded = decoded;
			}

			public string[] Encoded { get; }
			public string Decoded { get; }
		}

		public static IEnumerable<object[]> EscapesData = new List<object[]>()
		{
			new object[] { new TextData(@"\\", @"\") },
			new object[] { new TextData(@"\\\\\\", @"\\\") },
			new object[] { new TextData(@"\{", @"{") },
			new object[] { new TextData(@"\}", @"}") },
			new object[] { new TextData(@"\\P", @"\P") },
			new object[] { new TextData(@"\\~", @"\~") },
		};

		public static IEnumerable<object[]> ReadsTextData = new List<object[]>()
		{
			new object[] { new TextData("0", "0") },
			new object[] { new TextData("a", "a") },
			new object[] { new TextData("0123456789", "0123456789") },
			new object[] { new TextData("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz") },
			new object[] { new TextData(@"\P", "\n") },
			new object[] { new TextData(@"1\P2", "1\n2") },
			new object[] { new TextData(@"\~", "\u00A0") },
			new object[] { new TextData(@"1\~2", "1\u00A02") },
			new object[] { new TextData(@"%%", "%%") },
			new object[] { new TextData(@"%", "%") },
			new object[] { new TextData(new[] { @"%%d", @"%%D" }, "°") },
			new object[] { new TextData(new[] { @"1%%d", @"1%%D" }, "1°") },
			new object[] { new TextData(new[] { @"1%%d2", @"1%%D2" }, "1°2") },
			new object[] { new TextData(new[] { @"%%d2", @"%%D2" }, "°2") },
			new object[] { new TextData(new[] { @"%%p", @"%%P" }, "±") },
			new object[] { new TextData(new[] { @"1%%p", @"1%%P" }, "1±") },
			new object[] { new TextData(new[] { @"1%%p2", @"1%%P2" }, "1±2") },
			new object[] { new TextData(new[] { @"%%p2", @"%%P2" }, "±2") },
			new object[] { new TextData(new[] { @"%%c", @"%%C" }, "Ø") },
			new object[] { new TextData(new[] { @"1%%c", @"1%%C" }, "1Ø") },
			new object[] { new TextData(new[] { @"1%%c2", @"1%%C2" }, "1Ø2") },
			new object[] { new TextData(new[] { @"%%c2", @"%%C2" }, "Ø2") },
		};

		public static IEnumerable<object[]> FormatsData = new List<object[]>()
		{
			new object[]
			{
				new FormatData(@"\A0;BOTTOM",
					CreateTokenValue(new() { Align = MText.Format.Alignment.Bottom }, "BOTTOM"))
			},
			new object[]
			{
				new FormatData(@"\A1;CENTER",
					CreateTokenValue(new() { Align = MText.Format.Alignment.Center }, "CENTER"))
			},
			new object[]
			{
				new FormatData(@"\A2;TOP",
					CreateTokenValue(new() { Align = MText.Format.Alignment.Top }, "TOP"))
			},
			new object[]
			{
				new FormatData(new[]
					{
						@"BEFORE{\OFORMATTED}AFTER",
						@"BEFORE\OFORMATTED\oAFTER",
					},
					new MText.Token[]
					{
						CreateTokenValue("BEFORE"),
						CreateTokenValue(CreateFormat("O"), "FORMATTED"),
						CreateTokenValue(CreateFormat("o"), "AFTER")
					})
			},
			new object[]
			{
				new FormatData(new[]
					{
						@"BEFORE{\LFORMATTED}AFTER",
						@"BEFORE\LFORMATTED\lAFTER"
					},
					new MText.Token[]
					{
						CreateTokenValue("BEFORE"),
						CreateTokenValue(CreateFormat("L"), "FORMATTED"),
						CreateTokenValue(CreateFormat("l"), "AFTER")
					})
			},
			new object[]
			{
				new FormatData(new[]
					{
						@"BEFORE{\KFORMATTED}AFTER",
						@"BEFORE\KFORMATTED\kAFTER"
					},
					new MText.Token[]
					{
						CreateTokenValue("BEFORE"),
						CreateTokenValue(CreateFormat("K"), "FORMATTED"),
						CreateTokenValue(CreateFormat("k"), "AFTER"),
					})
			},
			new object[]
			{
				new FormatData(@"BEFORE\T2;FORMATTED",
					new MText.Token[]
						{ CreateTokenValue("BEFORE"), CreateTokenValue(new() { Tracking = 2 }, "FORMATTED") })
			},
			new object[]
			{
				new FormatData(@"\H2.64x;FORMATTED",
					new MText.Token[]
						{ CreateTokenValue(new() { Height = 2.64f, IsHeightRelative = true }, "FORMATTED"), },
					new MText.Format()
					{
						Height = 1f
					})
			},
			new object[]
			{
				new FormatData(new[]
					{
						@"{\H4;{\H2.64x;FORMATTED}}",
						@"\H10.56x;FORMATTED"
					},
					new MText.Token[]
					{
						CreateTokenValue(new() { Height = 10.56f, IsHeightRelative = true }, "FORMATTED"),
					}, new MText.Format() { Height = 1f })
			},
			new object[]
			{
				new FormatData(@"\H2.64;FORMATTED",
					CreateTokenValue(new() { Height = 2.64f }, "FORMATTED"))
			},
			new object[]
			{
				new FormatData(new[]
					{
						@"{\H4;{\H2.64;FORMATTED}}",
						@"\H2.64;FORMATTED",
					},
					CreateTokenValue(new() { Height = 2.64f }, "FORMATTED"))
			},
			new object[]
			{
				new FormatData(@"\T1.8;FORMATTED",
					CreateTokenValue(new() { Tracking = 1.8f }, "FORMATTED"))
			},
			new object[]
			{
				new FormatData(@"\Q14.84;FORMATTED",
					CreateTokenValue(new() { Obliquing = 14.84f }, "FORMATTED"))
			},
			new object[]
			{
				new FormatData(@"\Q-14.84;FORMATTED",
					CreateTokenValue(new() { Obliquing = -14.84f }, "FORMATTED"))
			},
			new object[]
			{
				new FormatData(@"\pt128.09,405.62,526.60;FORMATTED",
					CreateTokenValue(new()
					{
						Paragraph = { "t128.09".AsMemory(), "405.62".AsMemory(), "526.60".AsMemory() }
					}, "FORMATTED"))
			},
			new object[]
			{
				new FormatData(@"\pi-70.76154,l70.76154,t70.76154;FORMATTED",
					CreateTokenValue(new()
					{
						Paragraph = { "i-70.76154".AsMemory(), "l70.76154".AsMemory(), "t70.76154".AsMemory() }
					}, "FORMATTED"))
			},
			new object[]
			{
				new FormatData(@"1\P2", new MText.Token[]
				{
					CreateTokenValue("1\n"),
					CreateTokenValue("2"),
				})
			},
			new object[]
			{
				new FormatData(@"{\C165;\fArial|b0|i0|c0|p0;FORMATTED}", new MText.Token[]
				{
					CreateTokenValue(new MText.Format()
					{
						Color = new Color(165),
						Font =
						{
							FontFamily = "Arial".AsMemory(),
							CodePage = 0,
							Pitch = 0
						}
					}, "FORMATTED")
				})
			},
		};

		public static IEnumerable<object[]> FontsData = new List<object[]>()
		{
			new object[]
			{
				new FormatData(@"{\fArial|b0|i1|c22|p123;FORMAT}AFTER",
					new MText.Token[]
					{
						CreateTokenValue(new MText.Format()
						{
							Font =
							{
								FontFamily = "Arial".AsMemory(),
								IsBold = false,
								IsItalic = true,
								CodePage = 22,
								Pitch = 123
							}
						}, "FORMAT"),
						CreateTokenValue("AFTER"),
					})
			},
		};

		public static IEnumerable<object[]> ColorsData = new List<object[]>()
		{
			new object[] { new FormatData(@"{\C1;1}", CreateTokenValue(new() { Color = new Color(1) }, "1")) },
			new object[]
			{
				new FormatData(@"{\C184;1}NORMAL",
					new MText.Token[]
					{
						CreateTokenValue(new() { Color = new Color(184) }, "1"),
						CreateTokenValue("NORMAL")
					})
			},

			// True Colors
			new object[]
			{
				new FormatData(@"{\c245612;1}",
					CreateTokenValue(new() { Color = Color.FromTrueColor(245612) }, "1"))
			},
			new object[]
				{ new FormatData(@"{\c0;1}", CreateTokenValue(new() { Color = Color.FromTrueColor(0) }, "1")) },
		};

		public static IEnumerable<object[]> FractionsData = new List<object[]>()
		{
			new object[]
			{
				new FormatData(@"\S1^2;",
					CreateTokenFraction("1", "2", MText.TokenFraction.Divider.Stacked))
			},
			new object[]
			{
				new FormatData(@"\S1/2;",
					CreateTokenFraction("1", "2", MText.TokenFraction.Divider.FractionBar))
			},
			new object[]
			{
				new FormatData(@"\S1#2;",
					CreateTokenFraction("1", "2", MText.TokenFraction.Divider.Condensed))
			},
			new object[]
			{
				new FormatData(@"\SNUM^DEN;",
					CreateTokenFraction("NUM", "DEN", MText.TokenFraction.Divider.Stacked))
			},
			new object[]
			{
				new FormatData(@"\SNUM/DEN;",
					CreateTokenFraction("NUM", "DEN", MText.TokenFraction.Divider.FractionBar))
			},
			new object[]
			{
				new FormatData(@"\SNUM#DEN;",
					CreateTokenFraction("NUM", "DEN", MText.TokenFraction.Divider.Condensed))
			},

			// Escapes
			new object[]
			{
				new FormatData(@"\SNUM#DEN\;;",
					CreateTokenFraction("NUM", "DEN;", MText.TokenFraction.Divider.Condensed))
			},
			new object[]
			{
				new FormatData(@"\SNUM\##DEN\;;",
					CreateTokenFraction("NUM#", "DEN;", MText.TokenFraction.Divider.Condensed))
			},

			//Empty Numerator
			new object[]
			{
				new FormatData(@"\S^DEN;",
					CreateTokenFraction("", "DEN", MText.TokenFraction.Divider.Stacked))
			},
			new object[]
			{
				new FormatData(@"\S/DEN;",
					CreateTokenFraction("", "DEN", MText.TokenFraction.Divider.FractionBar))
			},
			new object[]
			{
				new FormatData(@"\S#DEN;",
					CreateTokenFraction("", "DEN", MText.TokenFraction.Divider.Condensed))
			},

			// Empty Denominator
			new object[]
			{
				new FormatData(@"\SNUM^;",
					CreateTokenFraction("NUM", "", MText.TokenFraction.Divider.Stacked))
			},
			new object[]
			{
				new FormatData(@"\SNUM/;",
					CreateTokenFraction("NUM", "", MText.TokenFraction.Divider.FractionBar))
			},
			new object[]
			{
				new FormatData(@"\SNUM#;",
					CreateTokenFraction("NUM", "", MText.TokenFraction.Divider.Condensed))
			},

			// Unexpected end to string.
			new object[] { new FormatData(@"\SNUMDEN\;;", (MText.Token?)null) },
			new object[] { new FormatData(@"\SNUMDEN", (MText.Token?)null) },
		};

		public static IEnumerable<object[]> ParseData = new List<object[]>()
		{
			new object[]
			{
				@"\A1;\P{\OOVERSTRIKE}\P{\LUNDERLINE}\P{STRIKE-THROUGH}\P{\fArial|b1|i0|c0|p34;BOLD}\P{\fArial|b0|i1|c0|p34;ITALIC\P\H0.7x;\SSUPERSET^;\H1.42857x;\P\H0.7x;\S^SUBSET;\H1.42857x;\P\H0.7x;\S1/2;\H1.42857x;\P\fArial|b0|i0|c0|p34;\P\pi-21.34894,l21.34894,t21.34894;\fSymbol|b0|i0|c2|p18;·	\fArial|b0|i0|c0|p34;BULLET 1\P\fSymbol|b0|i0|c2|p18;·	\fArial|b0|i0|c0|p34;BULLET 2\P\fSymbol|b0|i0|c2|p18;·	\fArial|b0|i0|c0|p34;BULLET 3\P\pi0,l0,tz;\P\pi-21.34894,l21.34894,t21.34894;a.	LOWERCASE LETTER A\Pb.	LOWERCASE LETTER B\Pc.	LOWERCASE LETTER C\P\pi0,l0,tz;\P\pi-21.34894,l21.34894,t21.34894;A.	UPPERCASE LETTER A\PB.	UPPERCASE LETTER B\PC.	UPPERCASE LETTER C\P\pi0,l0,tz;\P\pi-21.34894,l21.34894,t21.34894;1.	NUMBERED 1\P2.	NUMBERED 2\P3.	NUMBERED 3\P\pi0,l0,tz;\P\{TEST\}\P\PNOTE 1:	NOTE TEXT 1"" 2"" 3' 4'\P\\A1;\P\\P\PNOTE 2:	NOTE TEXT.\P}NESTED FORMATTING {\fArial|b1|i0|c0|p34;BOLD \fArial|b1|i1|c0|p34;ITALICS \LUNDERLINE \H2.24835x;FONT \fBaby Kruffy|b1|i1|c0|p2;DIFFERENT\fArial|b1|i1|c0|p34;\H0.44477x; \OOVERLINE \H0.7x;\SSUPER-TEXT^;\H1.42857x; \H0.7x;\S^SUB-TEXT;}",
				22
			},
		};
	}
}
