namespace ACadSharp.Tests.IO;

using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

/// <summary>
/// An OLE2FRAME recovers its four corners by parsing the OLE2 payload it carries. AutoCAD's own DXF
/// export writes the frame's geometry as XDATA under OLEBEGIN and emits NO binary chunks at all, so
/// there is no payload to parse - and the template read on from a null buffer regardless. The
/// ArgumentNullException came out of BuildDocument, so one frame lost the entire document: a
/// drawing of 32,571 entities holding twenty of them could not be read back from AutoCAD's DXF.
/// </summary>
public class Ole2FrameWithoutPayloadTests
{
	[Fact]
	public void AnOleFrameWithNoBinaryChunksDoesNotLoseTheDocument()
	{
		string dxf = this.dxfWithInjectedOleFrame();

		CadDocument doc = DxfReader.Read(new MemoryStream(Encoding.UTF8.GetBytes(dxf)));

		//The line is the point of the assertion: it says the rest of the document survived.
		Assert.Single(doc.Entities.OfType<Line>());
	}

	/// <summary>
	/// Writes a real document, then splices in an OLE2FRAME shaped the way AutoCAD writes one -
	/// subclass, flags, an "OLE" label, and no 310 chunk anywhere.
	/// </summary>
	private string dxfWithInjectedOleFrame()
	{
		CadDocument doc = new CadDocument();
		doc.Entities.Add(new Line(XYZ.Zero, new XYZ(10, 10, 0)));

		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.Write();
		}

		string text = Encoding.UTF8.GetString(stream.ToArray());

		string marker = "ENTITIES";
		int at = text.IndexOf(marker, StringComparison.Ordinal);
		Assert.True(at > 0, "the written DXF has no ENTITIES section");
		at = text.IndexOf('\n', at) + 1;

		string frame = string.Join("\n", new[]
		{
			"  0", "OLE2FRAME", "  5", "FFFF", "100", "AcDbEntity", "  8", "0",
			"100", "AcDbOle2Frame", " 70", "     2", "  1", "OLE", ""
		});

		return text.Substring(0, at) + frame + text.Substring(at);
	}
}
