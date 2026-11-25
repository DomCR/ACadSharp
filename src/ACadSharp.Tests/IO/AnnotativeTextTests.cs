using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class AnnotativeTextTests
{
	private static CadDocument annotativeDocument(out TextEntity text)
	{
		CadDocument doc = new CadDocument();
		Assert.True(doc.Scales.TryGet("1:100", out Scale scale), "a new document lists 1:100");

		text = new TextEntity
		{
			Value = "BH01",
			InsertPoint = new XYZ(10, 20, 0),
			AlignmentPoint = new XYZ(10, 20, 0),
			Height = 0.25,
			Rotation = 0.5,
		};
		doc.Entities.Add(text);

		AnnotativeData.SetAnnotative(text, true);
		TextObjectContextData context = TextObjectContextData.Create(text, scale);
		context.Default = true;
		AnnotativeData.AddContextData(text, context);
		return doc;
	}

	private static void assertAnnotative(CadDocument read)
	{
		TextEntity text = read.Entities.OfType<TextEntity>().Single();
		Assert.True(AnnotativeData.IsAnnotative(text));

		TextObjectContextData context = Assert.IsType<TextObjectContextData>(Assert.Single(AnnotativeData.GetContextData(text)));
		Assert.Equal("1:100", context.Scale.Name);
		Assert.Equal(100, context.Scale.DrawingUnits);
		Assert.Equal(new XY(10, 20), context.InsertPoint);
		Assert.Equal(0.5, context.Rotation, 9);
		Assert.True(context.Default);
		Assert.Equal(TextHorizontalAlignment.Left, context.HorizontalAlignment);
	}

	[Fact]
	public void CreatesTheContextDataChain()
	{
		annotativeDocument(out TextEntity text);

		Assert.True(text.XDictionary.TryGetEntry(AnnotativeData.ContextDataManagerName, out CadDictionary manager));
		Assert.True(manager.TryGetEntry(AnnotativeData.AnnotationScalesName, out CadDictionary scales));
		Assert.True(scales.ContainsKey("*A1"));
		Assert.True(AnnotativeData.IsAnnotative(text));

		AnnotativeData.SetAnnotative(text, false);
		Assert.False(AnnotativeData.IsAnnotative(text));
	}

	[Theory]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void DxfRoundTrip(ACadVersion version)
	{
		CadDocument doc = annotativeDocument(out _);
		doc.Header.Version = version;

		using MemoryStream stream = new MemoryStream();
		DxfWriter.Write(stream, doc, false, new DxfWriterConfiguration { CloseStream = false });

		assertAnnotative(DxfReader.Read(new MemoryStream(stream.ToArray())));
	}

	[Theory]
	
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void DwgRoundTrip(ACadVersion version)
	{
		CadDocument doc = annotativeDocument(out _);
		doc.Header.Version = version;

		using MemoryStream stream = new MemoryStream();
		DwgWriter.Write(stream, doc, new DwgWriterConfiguration { CloseStream = false });

		assertAnnotative(DwgReader.Read(new MemoryStream(stream.ToArray())));
	}

	[Fact]
	public void DxfWritesTheObjectContextDataSubclass()
	{
		CadDocument doc = annotativeDocument(out _);

		using MemoryStream stream = new MemoryStream();
		DxfWriter.Write(stream, doc, false, new DxfWriterConfiguration { CloseStream = false });

		string dxf = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		File.WriteAllText(Path.Combine(Path.GetTempPath(), "acadsharp-annotative.dxf"), dxf);
		// The object itself: the CLASSES section names the class first.
		int start = dxf.IndexOf("  0\r\n" + DxfFileToken.TextObjectContextData + "\r\n", System.StringComparison.Ordinal);
		Assert.True(start >= 0, "the context data object is written");
		string body = dxf.Substring(start, dxf.IndexOf("\r\n  0\r\n", start, System.StringComparison.Ordinal) - start);
		Assert.True(body.Contains(DxfSubclassMarker.ObjectContextData), body);
		Assert.True(body.Contains(DxfSubclassMarker.AnnotScaleObjectContextData), body);
	}
}
