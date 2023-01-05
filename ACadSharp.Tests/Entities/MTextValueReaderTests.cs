using System;
using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.IO;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueReaderTests
    {

        public static IEnumerable<object[]> EscapesData = new List<object[]>()
        {
            new[] { @"\\", @"\" },
            new[] { @"\\\\\\", @"\\\" },
            new[] { @"\", @"\" },
            new[] { @"\{", @"{" },
            new[] { @"\}", @"}" },
            new[] { @"\\P", @"\P" },
            new[] { @"\\~", @"\~" },
        };

        [Theory, MemberData(nameof(EscapesData))]
        public void Escapes(string input, string expected)
        {
            var reader = new MText.ValueReader();
            var parts = reader.Parse(input);

            if (parts[0] is MText.TokenValue value1)
                Assert.Equal(expected, value1.CombinedValues);
        }

        public static IEnumerable<object[]> ReadsTextData = new List<object[]>()
        {
        new [] { "0", "0" },
        new [] { "a", "a" },
        new [] { "0123456789", "0123456789" },
        new [] { "abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz" },
        new [] { @"\P", "\n" },
        new [] { @"1\P2", "1\n2" },
        new [] { @"\", @"\" },
        new [] { @"\~", "\u00A0" },
        new [] { @"1\~2", "1\u00A02" },
        new [] { @"%%", "%%" },
        new [] { @"%%%", "%" },
        new [] { @"%%d", "°" },
        new [] { @"1%%d", "1°" },
        new [] { @"1%%d2", "1°2" },
        new [] { @"1%%D2", "1°2" },
        new [] { @"%%p", "±" },
        new [] { @"1%%p", "1±" },
        new [] { @"1%%p2", "1±2" },
        new [] { @"1%%P2", "1±2" }, 
        new [] { @"%%c", "Ø" },
        new [] { @"1%%c", "1Ø" },
        new [] { @"1%%c2", "1Ø2" },
        new [] { @"1%%C2", "1Ø2" },
        };

        [Theory, MemberData(nameof(ReadsTextData))]
        public void ReadsText(string input, string expected)
        {
            var reader = new MText.ValueReader();
            var parts = reader.Parse(input);

            var combined = parts.OfType<MText.TokenValue>().Select(t => t.CombinedValues);
            Assert.Equal(expected, string.Concat(combined));
        }

        public static IEnumerable<object[]> FormatsData = new List<object[]>()
        {
            new[]
            {
                new MTextFormatsTestData(@"\A0;BOTTOM",
                    new MText.TokenValue(new() { Align = MText.Format.Alignment.Bottom }, "BOTTOM"))
            },
            new[]
            {
                new MTextFormatsTestData(@"\A1;CENTER",
                    new MText.TokenValue(new() { Align = MText.Format.Alignment.Center }, "CENTER"))
            },
            new[]
            {
                new MTextFormatsTestData(@"\A2;TOP",
                    new MText.TokenValue(new() { Align = MText.Format.Alignment.Top }, "TOP"))
            },
            new[]
            {
                new MTextFormatsTestData(@"BEFORE{\OFORMATTED}AFTER",
                    new[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("O"), "FORMATTED"),
                        new MText.TokenValue("AFTER")
                    })
            },
            new[]
            {
                new MTextFormatsTestData(@"BEFORE\OFORMATTED\oAFTER",
                    new[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("O"), "FORMATTED"),
                        new MText.TokenValue(new("o"), "AFTER")
                    })
            },
            new[]
            {
                new MTextFormatsTestData(@"BEFORE{\LFORMATTED}AFTER",
                    new[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("L"), "FORMATTED"),
                        new MText.TokenValue("AFTER")
                    })
            },
            new[]
            {
                new MTextFormatsTestData(@"BEFORE\LFORMATTED\lAFTER",
                    new[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("L"), "FORMATTED"),
                        new MText.TokenValue(new("l"), "AFTER")
                    })
            },
            new[]
            {
                new MTextFormatsTestData(@"BEFORE{\KFORMATTED}AFTER",
                    new[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("K"), "FORMATTED"),
                        new MText.TokenValue("AFTER"),
                    })
            },
            new[]
            {
                new MTextFormatsTestData(@"BEFORE\KFORMATTED\kAFTER",
                    new[]
                    {
                        new MText.TokenValue("BEFORE"),
                        new MText.TokenValue(new("K"), "FORMATTED"),
                        new MText.TokenValue(new("k"), "AFTER"),
                    })
            },
            new[]
            {
                new MTextFormatsTestData(@"BEFORE\T2;FORMATTED",
                    new[] { new MText.TokenValue("BEFORE"), new MText.TokenValue(new() { Tracking = 2 }, "FORMATTED") })
            },
            new[]
            {
                new MTextFormatsTestData(@"\H2.64x;FORMATTED",
                    new[] { new MText.TokenValue(new() { Height = 2.64f }, "FORMATTED"), }, new MText.Format()
                    {
                        Height = 1f
                    })
            },
            new[]
            {
                new MTextFormatsTestData(@"{\H4;{\H2.64x;FORMATTED}}",
                    new[]
                    {
                        new MText.TokenValue(new() { Height = 10.56f }, "FORMATTED"),
                    }, new MText.Format() { Height = 1f })
            },
            new[]
            {
                new MTextFormatsTestData(@"\H2.64;FORMATTED",
                    new MText.TokenValue(new() { Height = 2.64f }, "FORMATTED"))
            },
            new[]
            {
                new MTextFormatsTestData(@"{\H4;{\H2.64;FORMATTED}}",
                    new MText.TokenValue(new() { Height = 2.64f }, "FORMATTED"))
            },
            new[]
            {
                new MTextFormatsTestData(@"\T1.8;FORMATTED",
                    new MText.TokenValue(new() { Tracking = 1.8f }, "FORMATTED"))
            },
            new[]
            {
                new MTextFormatsTestData(@"\Q14.84;FORMATTED",
                    new MText.TokenValue(new() { Obliquing = 14.84f }, "FORMATTED"))
            },
            new[]
            {
                new MTextFormatsTestData(@"\pt128.09,405.62,526.60;FORMATTED",
                    new MText.TokenValue(new()
                    {
                        Paragraph = { "t128.09".AsMemory(), "405.62".AsMemory(), "526.60".AsMemory() }
                    }, "FORMATTED"))
            },
            new[]
            {
                new MTextFormatsTestData(@"\pi-70.76154,l70.76154,t70.76154;FORMATTED",
                    new MText.TokenValue(new()
                    {
                        Paragraph = { "i-70.76154".AsMemory(), "l70.76154".AsMemory(), "t70.76154".AsMemory() }
                    }, "FORMATTED"))
            },
            new[]
            {
                new MTextFormatsTestData(@"1\P2", new []
                {
                    new MText.TokenValue("1\n"),
                    new MText.TokenValue("2"),
                })
            },
        };

        [Theory, MemberData(nameof(FormatsData))]
        public void Formats(MTextFormatsTestData data)
        {
            TestFormatData(data);
        }

        public static IEnumerable<object[]> FontsData = new List<object[]>()
        {
            new[]
            {
                new MTextFormatsTestData(@"{\fArial|b0|i1|c22|p123;FORMAT}AFTER",
                    new[]
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

        [Theory, MemberData(nameof(FontsData))]
        public void Fonts(MTextFormatsTestData data)
        {
            TestFormatData(data);
        }

        public static IEnumerable<object[]> ColorsData = new List<object[]>()
        {
            new[] { new MTextFormatsTestData(@"{\C1;1}", new MText.TokenValue(new() { Color = new Color(1) }, "1")) },
            new[]
            {
                new MTextFormatsTestData(@"{\C184;1}NORMAL",
                    new[]
                    {
                        new MText.TokenValue(new() { Color = new Color(184) }, "1"),
                        new MText.TokenValue("NORMAL")
                    })
            },

            // True Colors
            new[] { new MTextFormatsTestData(@"{\c245612;1}", new MText.TokenValue(new() { Color = Color.FromTrueColor(245612) }, "1")) },
            new[] { new MTextFormatsTestData(@"{\c0;1}", new MText.TokenValue(new() { Color = Color.FromTrueColor(0) }, "1")) },
        };

        [Theory, MemberData(nameof(ColorsData))]
        public void Colors(MTextFormatsTestData data)
        {
            TestFormatData(data);
        }

        public static IEnumerable<object[]> FractionsData = new List<object[]>()
        {
            new[]
            {
                new MTextFormatsTestData(@"\S1^2;",
                    new MText.TokenFraction("1", "2", MText.TokenFraction.Divider.Stacked))
            },
            new[]
            {
                new MTextFormatsTestData(@"\S1/2;",
                    new MText.TokenFraction("1", "2", MText.TokenFraction.Divider.FractionBar))
            },
            new[]
            {
                new MTextFormatsTestData(@"\S1#2;",
                    new MText.TokenFraction("1", "2", MText.TokenFraction.Divider.Condensed))
            },
            new[]
            {
                new MTextFormatsTestData(@"\SNUM^DEN;",
                    new MText.TokenFraction("NUM", "DEN", MText.TokenFraction.Divider.Stacked))
            },
            new[]
            {
                new MTextFormatsTestData(@"\SNUM/DEN;",
                    new MText.TokenFraction("NUM", "DEN", MText.TokenFraction.Divider.FractionBar))
            },
            new[]
            {
                new MTextFormatsTestData(@"\SNUM#DEN;",
                    new MText.TokenFraction("NUM", "DEN", MText.TokenFraction.Divider.Condensed))
            },

            // Escapes
            new[]
            {
                new MTextFormatsTestData(@"\SNUM#DEN\;;",
                    new MText.TokenFraction("NUM", "DEN;", MText.TokenFraction.Divider.Condensed))
            },
            new[]
            {
                new MTextFormatsTestData(@"\SNUM\##DEN\;;",
                    new MText.TokenFraction("NUM#", "DEN;", MText.TokenFraction.Divider.Condensed))
            },

            // Unexpected end to string.
            new[] { new MTextFormatsTestData(@"\SNUMDEN\;;", (MText.Token?)null) },
            new[] { new MTextFormatsTestData(@"\SNUMDEN", (MText.Token?)null) },
        };

        [Theory, MemberData(nameof(FractionsData))]
        public void Fractions(MTextFormatsTestData data)
        {
            TestFormatData(data);
        }


        public static IEnumerable<object[]> ParseData = new List<object[]>()
        {
            new object[]
            {
                @"\A1;\P{\OOVERSTRIKE}\P{\LUNDERLINE}\P{STRIKE-THROUGH}\P{\fArial|b1|i0|c0|p34;BOLD}\P{\fArial|b0|i1|c0|p34;ITALIC\P\H0.7x;\SSUPERSET^;\H1.42857x;\P\H0.7x;\S^SUBSET;\H1.42857x;\P\H0.7x;\S1/2;\H1.42857x;\P\fArial|b0|i0|c0|p34;\P\pi-21.34894,l21.34894,t21.34894;\fSymbol|b0|i0|c2|p18;·	\fArial|b0|i0|c0|p34;BULLET 1\P\fSymbol|b0|i0|c2|p18;·	\fArial|b0|i0|c0|p34;BULLET 2\P\fSymbol|b0|i0|c2|p18;·	\fArial|b0|i0|c0|p34;BULLET 3\P\pi0,l0,tz;\P\pi-21.34894,l21.34894,t21.34894;a.	LOWERCASE LETTER A\Pb.	LOWERCASE LETTER B\Pc.	LOWERCASE LETTER C\P\pi0,l0,tz;\P\pi-21.34894,l21.34894,t21.34894;A.	UPPERCASE LETTER A\PB.	UPPERCASE LETTER B\PC.	UPPERCASE LETTER C\P\pi0,l0,tz;\P\pi-21.34894,l21.34894,t21.34894;1.	NUMBERED 1\P2.	NUMBERED 2\P3.	NUMBERED 3\P\pi0,l0,tz;\P\{TEST\}\P\PNOTE 1:	NOTE TEXT 1"" 2"" 3' 4'\P\\A1;\P\\P\PNOTE 2:	NOTE TEXT.\P}NESTED FORMATTING {\fArial|b1|i0|c0|p34;BOLD \fArial|b1|i1|c0|p34;ITALICS \LUNDERLINE \H2.24835x;FONT \fBaby Kruffy|b1|i1|c0|p2;DIFFERENT\fArial|b1|i1|c0|p34;\H0.44477x; \OOVERLINE \H0.7x;\SSUPER-TEXT^;\H1.42857x; \H0.7x;\S^SUB-TEXT;}",
                22
            },
        };

        [Theory, MemberData(nameof(ParseData))]
        public void Parse(string text, int expectedParts)
        {
            var reader = new MText.ValueReader();
            var parts = reader.Parse(text);
        }

        private void TestFormatData(MTextFormatsTestData data)
        {
            var reader = new MText.ValueReader();
            var parts = reader.Parse(data.Text, data.Format);

            if (data.Parsed == null)
            {
                Assert.Empty(parts);
                return;
            }

            Assert.Equal(data.Parsed!.Length, parts.Length);

            for (int i = 0; i < parts.Length; i++)
            {
                Assert.Equal(data.Parsed[i].Format, parts[i].Format);
                Assert.Equal(data.Parsed[i], parts[i]);
            }
        }
    }
}
