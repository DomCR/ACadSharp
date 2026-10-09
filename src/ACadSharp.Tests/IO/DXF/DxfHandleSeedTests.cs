using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfHandleSeedTests
{
	[Fact]
	public void ReadR12WithZeroHandleSeed()
	{
		// R12 file without handles ($HANDLING 0) that still declares $HANDSEED 0,
		// as some CAM exporters write it. Handle 0 is taken by the document itself.
		string dxf = string.Join("\n",
			"0", "SECTION",
			"2", "HEADER",
			"9", "$ACADVER",
			"1", "AC1009",
			"9", "$HANDLING",
			"70", "0",
			"9", "$HANDSEED",
			"5", "0",
			"0", "ENDSEC",
			"0", "SECTION",
			"2", "TABLES",
			"0", "TABLE",
			"2", "LTYPE",
			"70", "1",
			"0", "LTYPE",
			"2", "CONTINUOUS",
			"70", "0",
			"3", "Solid line",
			"72", "65",
			"73", "0",
			"40", "0.0",
			"0", "ENDTAB",
			"0", "TABLE",
			"2", "LAYER",
			"70", "2",
			"0", "LAYER",
			"2", "0",
			"70", "0",
			"62", "7",
			"6", "CONTINUOUS",
			"0", "LAYER",
			"2", "Cutting",
			"70", "0",
			"62", "1",
			"6", "CONTINUOUS",
			"0", "ENDTAB",
			"0", "ENDSEC",
			"0", "SECTION",
			"2", "ENTITIES",
			"0", "LINE",
			"8", "Cutting",
			"10", "0.0",
			"20", "0.0",
			"30", "0.0",
			"11", "100.0",
			"21", "50.0",
			"31", "0.0",
			"0", "ENDSEC",
			"0", "EOF");

		CadDocument doc;
		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dxf)))
		using (DxfReader reader = new DxfReader(stream))
		{
			doc = reader.Read();
		}

		Line line = Assert.IsType<Line>(doc.Entities.Single());
		Assert.Equal("Cutting", line.Layer.Name);
		Assert.NotEqual(0UL, line.Handle);
		Assert.True(doc.Header.HandleSeed > line.Handle);
	}
}
