using ACadSharp.Entities;
using ACadSharp.Text;
using System;
using Xunit;

namespace ACadSharp.Tests;

public class TextProcessorPortTests
{
	[Theory]
	[InlineData(@"\C18;\c0;\FTimes New Roman Bold|b1|i0|c1;\H3.162892;\W1.000000;CAR PARKING", "CAR PARKING")]
	[InlineData(@"{\W0.8;narrow} text", "narrow text")]
	[InlineData(@"\T1.1;track\Q15;oblique", "trackoblique")]
	[InlineData(@"1\S2^3;", "12/3")]
	[InlineData(@"\S1/2;", "1/2")]
	[InlineData(@"\S1#2; in", "1/2 in")]
	[InlineData(@"\S\^1^2;", "^1/2")]
	[InlineData(@"\S12;", "12")]
	[InlineData(@"\Lunder\l \Oover\o \Kstrike\k", "under over strike")]
	[InlineData(@"a\~b", "a b")]
	[InlineData(@"\\ \{x\}", @"\ {x}")]
	[InlineData(@"\S1\/2;", "1/2")]
	[InlineData(@"\S1\#2;", "1#2")]
	[InlineData(@"\S1\;2^3; after", "1;2/3 after")]
	[InlineData(@"value \S1^2", "value 1/2")]
	public void PlainTextStripsFormatting(string input, string expected)
	{
		Assert.Equal(expected, TextProcessor.Parse(input, out _));
		Assert.Equal(expected, new MText { Value = input }.PlainText);
	}

	[Theory]
	[InlineData("A1")][InlineData("Q15")][InlineData("T1.1")][InlineData("W0.8")]
	[InlineData("C18")][InlineData("c0")][InlineData("FTimes New Roman")][InlineData("fArial")]
	[InlineData("H2.5x")][InlineData("h2")][InlineData("pxqc")]
	public void UnterminatedCodesStopParsing(string code)
	{
		Assert.Equal("text", TextProcessor.Parse("text\\" + code, out _));
	}

	[Fact]
	public void ColumnAndParagraphBreaksUsePlatformNewline()
	{
		Assert.Equal("col1" + Environment.NewLine + "col2", TextProcessor.Parse(@"col1\Ncol2", out _));
		Assert.Equal("Top" + Environment.NewLine + "Bottom", TextProcessor.Parse(@"\A1;\H2.5x;\pxqc;Top\PBottom", out _));
	}
}
