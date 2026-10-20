using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.XData;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.IO;

public class TextStyleFontSerializationTests
{
	[Fact]
	public void ShxFontsDoNotReceiveTrueTypeData()
	{
		var style = new TextStyle("Simplex") { Filename = "simplex.shx", TrueType = FontFlags.Bold };
		Assert.Same(style.ExtendedData, TextStyleExtendedData.ForWrite(style));
	}

	[Fact]
	public void ExistingFontPitchAndCharsetBitsSurviveFlagUpdates()
	{
		var document = new CadDocument();
		var style = new TextStyle("Arial") { Filename = "Arial.ttf", TrueType = FontFlags.Italic };
		document.TextStyles.Add(style);
		style.ExtendedData.Add("ACAD", new ExtendedData(new ExtendedDataRecord[]
		{
			new ExtendedDataString("Arial"), new ExtendedDataInteger32(0x02000022),
		}));
		var result = TextStyleExtendedData.ForWrite(style);
		Assert.Equal(0x01000022, result.Get("ACAD").Records[1].RawValue);
		Assert.Equal(0x02000022, style.ExtendedData.Get("ACAD").Records[1].RawValue);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void BoldItalicRoundtripPreservesCustomXDataWithoutMutatingSource(bool dwg)
	{
		var document = new CadDocument();
		var style = new TextStyle("ArialBoldItalic") { Filename = "fonts/Arial.ttf", TrueType = FontFlags.Bold | FontFlags.Italic };
		document.TextStyles.Add(style);
		style.ExtendedData.Add("ACAD", new ExtendedData(new ExtendedDataRecord[] { new ExtendedDataString("custom"), new ExtendedDataInteger16(7) }));
		CadDocument result;
		if (dwg) result = DWG.ModelerGeometryWriterTests.Roundtrip(document);
		else
		{
			using var output = new MemoryStream();
			using (var writer = new DxfWriter(output, document, false))
			{
				writer.Configuration.CloseStream = false;
				writer.Write();
			}
			using var input = new MemoryStream(output.ToArray());
			using var reader = new DxfReader(input);
			result = reader.Read();
		}
		Assert.Equal(2, style.ExtendedData.Get("ACAD").Records.Count);
		var actual = result.TextStyles[style.Name];
		Assert.Equal(style.TrueType, actual.TrueType);
		var records = actual.ExtendedData.Get("ACAD").Records;
		Assert.Equal(4, records.Count);
		Assert.Equal("Arial", records[0].RawValue);
		Assert.Equal(0x03000000, records[1].RawValue);
		Assert.Equal("custom", records[2].RawValue);
		Assert.Equal((short)7, records[3].RawValue);
	}
}
