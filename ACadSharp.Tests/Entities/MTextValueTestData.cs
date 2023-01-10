#nullable enable
using System;
using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueTestData
    {
        public class FormatData
        {
            public FormatData(string input, MText.Token? expected)
                : this(new []{ input }, expected == null ? null : new[] { expected })
            {
            }

            public FormatData(string[] input, MText.Token? expected)
                : this(input , expected == null ? null : new[] { expected })
            {
            }

            public FormatData(string encoded, MText.Token[]? decoded)
                : this(new[]{ encoded }, decoded, null)
            {
            }

            public FormatData(string[] encoded, MText.Token[]? decoded)
                : this(encoded, decoded, null)
            {
            }


            public FormatData(string encoded, MText.Token[]? decoded, MText.Format? format)
                : this(new []{ encoded }, decoded, format)
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
                :this (new []{ encoded }, decoded)
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
            new object[] { new TextData(new[] { @"%%d" , @"%%D" }, "°") },
            new object[] { new TextData(new[] { @"1%%d", @"1%%D" }, "1°") },
            new object[] { new TextData(new[] { @"1%%d2", @"1%%D2" }, "1°2") },
            new object[] { new TextData(new[] { @"%%d2", @"%%D2" }, "°2") },
            new object[] { new TextData(new[] { @"%%p" , @"%%P" }, "±") },
            new object[] { new TextData(new[] { @"1%%p", @"1%%P" }, "1±") },
            new object[] { new TextData(new[] { @"1%%p2", @"1%%P2" }, "1±2") },
            new object[] { new TextData(new[] { @"%%p2", @"%%P2" }, "±2") },
            new object[] { new TextData(new[] { @"%%c" , @"%%C" }, "Ø") },
            new object[] { new TextData(new[] { @"1%%c", @"1%%C" }, "1Ø") },
            new object[] { new TextData(new[] { @"1%%c2", @"1%%C2" }, "1Ø2") },
            new object[] { new TextData(new[] { @"%%c2", @"%%C2" }, "Ø2") },
        };

        public static IEnumerable<object[]> FormatsData = new List<object[]>()
        {
            new object[]
            {
                new FormatData(@"\A0;BOTTOM",
                    new MText.TokenValue(new() { Align = MText.Format.Alignment.Bottom }, "BOTTOM"))
            },
            new object[]
            {
                new FormatData(@"\A1;CENTER",
                    new MText.TokenValue(new() { Align = MText.Format.Alignment.Center }, "CENTER"))
            },
            new object[]
            {
                new FormatData(@"\A2;TOP",
                    new MText.TokenValue(new() { Align = MText.Format.Alignment.Top }, "TOP"))
            },
            new object[]
            {
                new FormatData(new []
                    {
                        @"BEFORE{\OFORMATTED}AFTER",
                        @"BEFORE\OFORMATTED\oAFTER",
                    },
                    new MText.Token[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("O"), "FORMATTED"),
                        new MText.TokenValue(new("o"),"AFTER")
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
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("L"), "FORMATTED"),
                        new MText.TokenValue(new("l"), "AFTER")
                    })
            },
            new object[]
            {
                new FormatData(new [] {
                        @"BEFORE{\KFORMATTED}AFTER",
                        @"BEFORE\KFORMATTED\kAFTER"
                    },
                    new MText.Token[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("K"), "FORMATTED"),
                        new MText.TokenValue(new("k"), "AFTER"),
                    })
            },
            new object[]
            {
                new FormatData(@"BEFORE\T2;FORMATTED",
                    new MText.Token[] { new MText.TokenValue("BEFORE"), new MText.TokenValue(new() { Tracking = 2 }, "FORMATTED") })
            },
            new object[]
            {
                new FormatData(@"\H2.64x;FORMATTED",
                    new MText.Token[] { new MText.TokenValue(new() { Height = 2.64f, IsHeightRelative = true}, "FORMATTED"), }, new MText.Format()
                    {
                        Height = 1f
                    })
            },
            new object[]
            {
                new FormatData(new []
                    {
                        @"{\H4;{\H2.64x;FORMATTED}}",
                        @"\H10.56x;FORMATTED"
                    },
                    new MText.Token[]
                    {
                        new MText.TokenValue(new() { Height = 10.56f, IsHeightRelative = true }, "FORMATTED"),
                    }, new MText.Format() { Height = 1f })
            },
            new object[]
            {
                new FormatData(@"\H2.64;FORMATTED",
                    new MText.TokenValue(new() { Height = 2.64f }, "FORMATTED"))
            },
            new object[]
            {
                new FormatData(new []
                    {
                        @"{\H4;{\H2.64;FORMATTED}}",
                        @"\H2.64;FORMATTED",
                    },
                    new MText.TokenValue(new() { Height = 2.64f }, "FORMATTED"))
            },
            new object[]
            {
                new FormatData(@"\T1.8;FORMATTED",
                    new MText.TokenValue(new() { Tracking = 1.8f }, "FORMATTED"))
            },
            new object[]
            {
                new FormatData(@"\Q14.84;FORMATTED",
                    new MText.TokenValue(new() { Obliquing = 14.84f }, "FORMATTED"))
            },
            new object[]
            {
                new FormatData(@"\pt128.09,405.62,526.60;FORMATTED",
                    new MText.TokenValue(new()
                    {
                        Paragraph = { "t128.09".AsMemory(), "405.62".AsMemory(), "526.60".AsMemory() }
                    }, "FORMATTED"))
            },
            new object[]
            {
                new FormatData(@"\pi-70.76154,l70.76154,t70.76154;FORMATTED",
                    new MText.TokenValue(new()
                    {
                        Paragraph = { "i-70.76154".AsMemory(), "l70.76154".AsMemory(), "t70.76154".AsMemory() }
                    }, "FORMATTED"))
            },
            new object[]
            {
                new FormatData(@"1\P2", new MText.Token[]
                {
                    new MText.TokenValue("1\n"),
                    new MText.TokenValue("2"),
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
                        new MText.TokenValue(new MText.Format()
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
                        new MText.TokenValue("AFTER"),
                    })
            },
        };

        public static IEnumerable<object[]> ColorsData = new List<object[]>()
        {
            new object[] { new FormatData(@"{\C1;1}", new MText.TokenValue(new() { Color = new Color(1) }, "1")) },
            new object[]
            {
                new FormatData(@"{\C184;1}NORMAL",
                    new MText.Token[]
                    {
                        new MText.TokenValue(new() { Color = new Color(184) }, "1"),
                        new MText.TokenValue("NORMAL")
                    })
            },

            // True Colors
            new object[] { new FormatData(@"{\c245612;1}", new MText.TokenValue(new() { Color = Color.FromTrueColor(245612) }, "1")) },
            new object[] { new FormatData(@"{\c0;1}", new MText.TokenValue(new() { Color = Color.FromTrueColor(0) }, "1")) },
        };

        public static IEnumerable<object[]> FractionsData = new List<object[]>()
        {
            new object[]
            {
                new FormatData(@"\S1^2;",
                    new MText.TokenFraction("1", "2", MText.TokenFraction.Divider.Stacked))
            },
            new object[]
            {
                new FormatData(@"\S1/2;",
                    new MText.TokenFraction("1", "2", MText.TokenFraction.Divider.FractionBar))
            },
            new object[]
            {
                new FormatData(@"\S1#2;",
                    new MText.TokenFraction("1", "2", MText.TokenFraction.Divider.Condensed))
            },
            new object[]
            {
                new FormatData(@"\SNUM^DEN;",
                    new MText.TokenFraction("NUM", "DEN", MText.TokenFraction.Divider.Stacked))
            },
            new object[]
            {
                new FormatData(@"\SNUM/DEN;",
                    new MText.TokenFraction("NUM", "DEN", MText.TokenFraction.Divider.FractionBar))
            },
            new object[]
            {
                new FormatData(@"\SNUM#DEN;",
                    new MText.TokenFraction("NUM", "DEN", MText.TokenFraction.Divider.Condensed))
            },
            
            // Escapes
            new object[]
            {
                new FormatData(@"\SNUM#DEN\;;",
                    new MText.TokenFraction("NUM", "DEN;", MText.TokenFraction.Divider.Condensed))
            },
            new object[]
            {
                new FormatData(@"\SNUM\##DEN\;;",
                    new MText.TokenFraction("NUM#", "DEN;", MText.TokenFraction.Divider.Condensed))
            },

            //Empty Numerator
            new object[]
            {
                new FormatData(@"\S^DEN;",
                    new MText.TokenFraction("", "DEN", MText.TokenFraction.Divider.Stacked))
            },
            new object[]
            {
                new FormatData(@"\S/DEN;",
                    new MText.TokenFraction("", "DEN", MText.TokenFraction.Divider.FractionBar))
            },
            new object[]
            {
                new FormatData(@"\S#DEN;",
                    new MText.TokenFraction("", "DEN", MText.TokenFraction.Divider.Condensed))
            },

            // Empty Denominator
            new object[]
            {
                new FormatData(@"\SNUM^;",
                    new MText.TokenFraction("NUM", "", MText.TokenFraction.Divider.Stacked))
            },
            new object[]
            {
                new FormatData(@"\SNUM/;",
                    new MText.TokenFraction("NUM", "", MText.TokenFraction.Divider.FractionBar))
            },
            new object[]
            {
                new FormatData(@"\SNUM#;",
                    new MText.TokenFraction("NUM", "", MText.TokenFraction.Divider.Condensed))
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
