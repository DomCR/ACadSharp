using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfLegacyInsertTests
{
	[Fact]
	public void ReadAttributesOfAnInsertInR12()
	{
		//Issue #1393: R12 has no owner handles, the ATTRIBs and the SEQEND follow their INSERT
		string[] pairs =
		{
			"0", "SECTION", "2", "HEADER", "9", "$ACADVER", "1", "AC1009", "9", "$HANDSEED", "5", "100", "0", "ENDSEC",
			"0", "SECTION", "2", "BLOCKS",
			"0", "BLOCK", "8", "0", "2", "B1", "70", "2", "10", "0.0", "20", "0.0", "30", "0.0", "3", "B1",
			"0", "ATTDEF", "8", "0", "10", "0.0", "20", "0.0", "30", "0.0", "40", "1.0", "1", "default", "3", "Prompt", "2", "TAG1", "70", "0",
			"0", "ENDBLK", "8", "0",
			"0", "ENDSEC",
			"0", "SECTION", "2", "ENTITIES",
			"0", "INSERT", "5", "20", "8", "0", "66", "1", "2", "B1", "10", "10.0", "20", "20.0", "30", "0.0",
			"0", "ATTRIB", "5", "21", "8", "0", "10", "10.0", "20", "20.0", "30", "0.0", "40", "1.0", "1", "hello", "2", "TAG1", "70", "0",
			"0", "SEQEND", "5", "22", "8", "0",
			"0", "ENDSEC",
			"0", "EOF",
		};

		CadDocument doc;
		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(string.Join("\n", pairs) + "\n")))
		{
			doc = DxfReader.Read(stream, new DxfReaderConfiguration { CreateDefaults = true });
		}

		Insert insert = Assert.Single(doc.ModelSpace.Entities.OfType<Insert>());
		AttributeEntity att = Assert.Single(insert.Attributes);
		Assert.Equal("TAG1", att.Tag);
		Assert.Equal("hello", att.Value);
		Assert.NotNull(insert.Attributes.Seqend);
		Assert.Single(doc.ModelSpace.Entities);
	}

	[Fact]
	public void ReadAttributesOfTheInsertsInTheR12Sample()
	{
		CadDocument doc = DxfReader.Read(
			Path.Combine(TestVariables.SamplesFolder, "sample_AC1009_ascii.dxf"),
			new DxfReaderConfiguration { CreateDefaults = true });

		Assert.NotEmpty(doc.GetCadObject<Insert>(0x704).Attributes);
		Assert.NotEmpty(doc.GetCadObject<Insert>(0x79C).Attributes);
		Assert.DoesNotContain(doc.ModelSpace.Entities, e => e is AttributeEntity || e is Seqend);

		doc.Header.Version = ACadVersion.AC1032;
		DwgWriter.Write(new MemoryStream(), doc);
	}
}
