using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfAcdsDataSectionTests
{
	[Fact]
	public void ReadAcdsDataAttachesAcisPayloadToOwnerEntity()
	{
		// Minimal R2013+ style layout: the REGION entity carries no geometry codes,
		// the SAB payload lives in the ACDSDATA section and points back to the
		// entity through the AcDbDs::ID handle at group code 320.
		// The payload here is just the "ACIS BinaryFile" signature (15 bytes).
		string dxf = string.Join("\n",
			"0", "SECTION",
			"2", "ENTITIES",
			"0", "REGION",
			"5", "1FF0",
			"100", "AcDbEntity",
			"8", "0",
			"100", "AcDbModelerGeometry",
			"70", "1",
			"0", "ENDSEC",
			"0", "SECTION",
			"2", "ACDSDATA",
			"70", "2",
			"71", "2",
			"0", "ACDSSCHEMA",
			"90", "1",
			"1", "AcDb3DSolid_ASM_Data",
			"2", "AcDbDs::ID",
			"280", "10",
			"91", "0",
			"2", "ASM_Data",
			"280", "15",
			"91", "0",
			"0", "ACDSRECORD",
			"90", "1",
			"2", "AcDbDs::ID",
			"280", "10",
			"320", "1FF0",
			"2", "ASM_Data",
			"280", "15",
			"94", "15",
			"310", "414349532042696E61727946696C65",
			"0", "ENDSEC",
			"0", "EOF");

		CadDocument doc;
		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dxf)))
		using (DxfReader reader = new DxfReader(stream))
		{
			doc = reader.Read();
		}

		Region region = doc.Entities.OfType<Region>().Single();

		Assert.NotNull(region.AcisData);
		Assert.Equal(15, region.AcisData.Length);
		Assert.True(region.IsBinaryAcisData);
		Assert.Null(region.GetAcisText());
		Assert.Equal("ACIS BinaryFile", Encoding.ASCII.GetString(region.AcisData));
	}

	[Fact]
	public void AcisDataTextHelpers()
	{
		Region region = new Region();

		Assert.False(region.IsBinaryAcisData);
		Assert.Null(region.GetAcisText());

		region.AcisData = Encoding.ASCII.GetBytes("400 0 1 0\nbody $-1 $1 $-1 $-1 #\n");

		Assert.False(region.IsBinaryAcisData);
		Assert.StartsWith("400 0 1 0", region.GetAcisText());
	}
}
