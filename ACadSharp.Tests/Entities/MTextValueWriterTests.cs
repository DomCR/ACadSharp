using System;
using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.IO;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueWriterTests
    {

        public static IEnumerable<object[]> EscapesData = new List<object[]>()
        {
            new[] { @"\\", @"\" },
            new[] { @"\\\\\\", @"\\\" },
            // ToDo: Review if this one is valid or not.
            //new[] { @"\", @"\" },
            new[] { @"\{", @"{" },
            new[] { @"\}", @"}" },
            new[] { @"\\P", @"\P" },
            new[] { @"\\~", @"\~" },
            new[] { @"1\\~2", @"1\~2" },
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

        [Theory, MemberData(nameof(EscapesData))]
        public void Escapes(string expected, string input)
        {
            var reader = new MText.ValueWriter();
            var parts = reader.Seralize(new[] { new MText.TokenValue(input) });
            Assert.Equal(expected, string.Concat(parts));
        }
        
        public static IEnumerable<object[]> ReadsTextData = new List<object[]>()
        {
        new [] { "0", "0" },
        new [] { "a", "a" },
        new [] { "0123456789", "0123456789" },
        new [] { "abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz" },
        new [] { @"\P", "\n" },
        // ToDo: Review if this one is valid or not.
        //new [] { @"\", @"\" },
        new [] { @"\~", "\u00A0" },
    };

        [Theory, MemberData(nameof(ReadsTextData))]
        public void ReadsText(string expected, string input)
        {
            var reader = new MText.ValueWriter();
            var parts = reader.Seralize(new[] { new MText.TokenValue(input) });
            Assert.Equal(expected, string.Concat(parts));
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
            new[]
            {
                new MTextFormatsTestData(@"{\c245612;1}",
                    new MText.TokenValue(new() { Color = Color.FromTrueColor(245612) }, "1"))
            },
            new[]
            {
                new MTextFormatsTestData(@"{\c0;1}",
                    new MText.TokenValue(new() { Color = Color.FromTrueColor(0) }, "1"))
            },
            new[]
            {
                new MTextFormatsTestData(@"0{\c0;1}2", new[]
                {
                    new MText.TokenValue("0"),
                    new MText.TokenValue(new MText.Format()
                    {
                        Color = Color.FromTrueColor(0)
                    }, "1"),
                    new MText.TokenValue("2"),
                })
            },
        };

        [Theory, MemberData(nameof(ColorsData))]
        public void Colors(MTextFormatsTestData data)
        {
            TestFormatData(data);
        }

        public static IEnumerable<object[]> FractionsData = new List<object[]>()
        {/*
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
            },*/
            new[]
            {
                new MTextFormatsTestData(@"\SNUM\##DEN\;;",
                    new MText.TokenFraction("NUM#", "DEN;", MText.TokenFraction.Divider.Condensed))
            },
        };

        [Theory, MemberData(nameof(FractionsData))]
        public void Fractions(MTextFormatsTestData data)
        {
            TestFormatData(data);
        }


        private void TestFormatData(MTextFormatsTestData data)
        {
            var reader = new MText.ValueWriter();
            var parts = reader.Seralize(data.Parsed);
            Assert.Equal(data.Text, string.Concat(parts));
        }
    }
}
