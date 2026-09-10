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

	[Fact]
	public void WriteLongSatLineCutsGroupsAtSpaces()
	{
		// A SAT line longer than a group value (255 characters) continues in
		// code 3 groups; every cut must fall on a space, never inside a token,
		// because the restore reads the tokens group by group.
		StringBuilder longLine = new StringBuilder("intcurve-curve $-1 forward { exactcur nurbs 3 open 3");
		for (int i = 0; i < 40; i++)
		{
			longLine.Append(' ').Append((0.123456789 * i).ToString("R", System.Globalization.CultureInfo.InvariantCulture));
		}
		longLine.Append(" null_surface null_surface nullbs nullbs I I 0 0 0 I I } I I #");

		string sat = string.Join("\n", "400 1 1 0", "body $-1 $1 $-1 $-1 #", longLine.ToString(), "End-of-ACIS-data ");

		CadDocument doc = new CadDocument();
		doc.Header.Version = ACadVersion.AC1024;
		Region region = new Region();
		region.AcisData = Encoding.ASCII.GetBytes(sat);
		region.ModelerFormatVersion = 1;
		doc.Entities.Add(region);

		string dxf;
		using (MemoryStream stream = new MemoryStream())
		{
			DxfWriter.Write(stream, doc);
			dxf = Encoding.ASCII.GetString(stream.ToArray());
		}

		// collect the code 1/3 values of the region in order
		string[] lines = dxf.Split('\n');
		System.Collections.Generic.List<(int code, string value)> groups = new System.Collections.Generic.List<(int, string)>();
		bool inRegion = false;
		for (int i = 0; i + 1 < lines.Length; i += 2)
		{
			string code = lines[i].Trim();
			string value = lines[i + 1].TrimEnd('\r');
			if (code == "0")
			{
				inRegion = value == "REGION";
			}
			else if (inRegion && (code == "1" || code == "3"))
			{
				groups.Add((int.Parse(code), value));
			}
		}

		Assert.Contains(groups, g => g.code == 3);
		Assert.All(groups, g => Assert.True(g.value.Length <= 255));

		// a continuation follows a chunk that ends on a space
		for (int i = 0; i < groups.Count; i++)
		{
			if (groups[i].code == 3)
			{
				Assert.EndsWith(" ", groups[i - 1].value);
			}
		}

		// the reader joins the groups back to the original text
		Assert.Equal(sat, readSingleRegion(dxf).GetAcisText());
	}

	[Fact]
	public void WriteSolid3DSubclassOnlySince2007()
	{
		// The AcDb3dSolid subclass and its History ID field (350) exist since
		// the 2007 format: the older formats stop at AcDbModelerGeometry, and a
		// subclass marker without its field makes AutoCAD discard the DXF.
		string sat = string.Join("\n", "400 1 1 0", "body $-1 $1 $-1 $-1 #", "End-of-ACIS-data ");

		System.Collections.Generic.List<(string code, string value)> older = writeSolid3D(ACadVersion.AC1015, sat);
		Assert.DoesNotContain(older, g => g.code == "100" && g.value == DxfSubclassMarker.Solid3D);
		Assert.DoesNotContain(older, g => g.code == "350");

		System.Collections.Generic.List<(string code, string value)> newer = writeSolid3D(ACadVersion.AC1024, sat);
		int marker = newer.FindIndex(g => g.code == "100" && g.value == DxfSubclassMarker.Solid3D);
		Assert.True(marker >= 0);
		Assert.Equal("350", newer[marker + 1].code);
	}

	// writes a single 3DSOLID and returns its groups in order
	private static System.Collections.Generic.List<(string code, string value)> writeSolid3D(ACadVersion version, string sat)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;
		Solid3D solid = new Solid3D();
		solid.AcisData = Encoding.ASCII.GetBytes(sat);
		solid.ModelerFormatVersion = 1;
		doc.Entities.Add(solid);

		string dxf;
		using (MemoryStream stream = new MemoryStream())
		{
			DxfWriter.Write(stream, doc);
			dxf = Encoding.ASCII.GetString(stream.ToArray());
		}

		string[] lines = dxf.Split('\n');
		System.Collections.Generic.List<(string code, string value)> groups = new System.Collections.Generic.List<(string, string)>();
		bool inSolid = false;
		for (int i = 0; i + 1 < lines.Length; i += 2)
		{
			string code = lines[i].Trim();
			string value = lines[i + 1].TrimEnd('\r');
			if (code == "0")
			{
				inSolid = value == "3DSOLID";
			}
			else if (inSolid)
			{
				groups.Add((code, value));
			}
		}

		return groups;
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
