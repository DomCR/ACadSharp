using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfAcisSatTextTests
{
	[Fact]
	public void ReadCharacterSwappedSatFromCodes1And3()
	{
		// Pre-2013 files store the SAT text of the modeler geometry entities with
		// the documented character swap (0x9F minus the character, above space).
		// Group code 1 starts a SAT line, group code 3 continues the previous one.
		string line1 = "400 0 1 0";
		string line2 = "body $-1 $1 $-1 $-1 #";

		// Split the second line between codes 1 and 3 to check the join.
		string chunkA = line2.Substring(0, 10);
		string chunkB = line2.Substring(10);

		string dxf = string.Join("\n",
			"0", "SECTION",
			"2", "ENTITIES",
			"0", "REGION",
			"5", "A1",
			"100", "AcDbEntity",
			"8", "0",
			"100", "AcDbModelerGeometry",
			"70", "1",
			"1", encode(line1),
			"1", encode(chunkA),
			"3", encode(chunkB),
			"0", "ENDSEC",
			"0", "EOF");

		Region region = readSingleRegion(dxf);

		Assert.NotNull(region.AcisData);
		Assert.False(region.IsBinaryAcisData);
		Assert.Equal(line1 + "\n" + line2, region.GetAcisText());
	}

	[Fact]
	public void ReadPlainSatFromCodes1And3()
	{
		// A payload that already starts with the numeric SAT header must pass
		// through untouched.
		string line1 = "400 0 1 0";
		string line2 = "body $-1 $1 $-1 $-1 #";

		string dxf = string.Join("\n",
			"0", "SECTION",
			"2", "ENTITIES",
			"0", "REGION",
			"5", "A1",
			"100", "AcDbEntity",
			"8", "0",
			"100", "AcDbModelerGeometry",
			"70", "1",
			"1", line1,
			"1", line2,
			"0", "ENDSEC",
			"0", "EOF");

		Region region = readSingleRegion(dxf);

		Assert.Equal(line1 + "\n" + line2, region.GetAcisText());
	}

	private static Region readSingleRegion(string dxf)
	{
		CadDocument doc;
		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dxf)))
		using (DxfReader reader = new DxfReader(stream))
		{
			doc = reader.Read();
		}

		return doc.Entities.OfType<Region>().Single();
	}

	private static string encode(string text)
	{
		StringBuilder sb = new StringBuilder(text.Length);
		foreach (char c in text)
		{
			sb.Append(c > ' ' ? (char)(0x9F - c) : c);
		}

		return sb.ToString();
	}
}
