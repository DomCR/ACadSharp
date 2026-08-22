using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// A region is an ACIS entity: the reader keeps its payload, and until now no writer put it back.
/// Measured on a production drawing with 32 of them, through AutoCAD 2027: AutoCAD's own export of
/// the drawing has 32 REGION, its export of the file this library wrote had 0. Eight of seventeen
/// production drawings carry regions, 846 in all, and 845 of those come with a full payload.
/// </summary>
public class RegionWriteTests
{
	private static string sampleR2018 => Path.Combine(TestVariables.SamplesFolder, "sample_AC1032_ascii.dxf");

	private static string sampleR2000 => Path.Combine(TestVariables.SamplesFolder, "sample_AC1015_ascii.dxf");

	//The oldest sample whose region carries the binary payload; R2000 files carry SAT text instead.
	private static string sampleR2004 => Path.Combine(TestVariables.SamplesFolder, "sample_AC1018.dwg");

	[Fact]
	public void RegionSurvivesADxfRoundTrip()
	{
		CadDocument doc = DxfReader.Read(sampleR2018);
		Region[] before = this.regions(doc);
		Assert.NotEmpty(before);

		CadDocument back = this.roundTrip(doc);

		Region[] after = this.regions(back);
		Assert.Equal(before.Length, after.Length);
		for (int i = 0; i < before.Length; i++)
		{
			Assert.Equal(before[i].AcisData, after[i].AcisData);
		}
	}

	[Fact]
	public void ThePayloadIsWrittenToTheAcdsSectionInR2013Plus()
	{
		//R2013+ keeps the geometry out of the entity: the entity says it has a payload, the ACDSDATA
		//section carries it, keyed by the entity's handle.
		CadDocument doc = DxfReader.Read(sampleR2018);
		Region region = this.regions(doc).First();

		string text = this.write(doc);

		Assert.Contains("ACDSDATA", text);
		Assert.Contains("ASM_Data", text);
		Assert.Contains(region.Handle.ToString("X"), text);
	}

	[Fact]
	public void ABinaryPayloadIsLeftOutOfAPreR2013FileAndSaidSo()
	{
		//Before R2013 the payload has to be character-swapped SAT text inside the entity, and a
		//binary SAB payload cannot be turned into one. Writing the region empty would leave a handle
		//with no shape, so it is left out - and the reason is reported rather than left to be found.
		CadDocument doc = DxfReader.Read(sampleR2018);
		Region region = this.regions(doc).First();
		if (!region.IsBinaryAcisData)
		{
			//The upstream sample carries SAT text; make the case the production drawings show.
			region.AcisData = System.Text.Encoding.ASCII.GetBytes("ACIS BinaryFile-not-really-text");
		}

		doc.Header.Version = ACadVersion.AC1015;
		string reported = null;
		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.OnNotification += (s, e) => { if (e.Message.Contains("binary ACIS payload")) reported = e.Message; };
			writer.Write();
		}

		Assert.NotNull(reported);
		string text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.DoesNotContain("\nREGION\n", text.Replace("\r", string.Empty));
	}

	[Fact]
	public void ASatPayloadIsWrittenInsideThePreR2013Entity()
	{
		CadDocument doc = DxfReader.Read(sampleR2000);
		Region[] before = this.regions(doc);
		Assert.NotEmpty(before);
		Assert.False(before[0].IsBinaryAcisData);

		doc.Header.Version = ACadVersion.AC1015;
		CadDocument back = this.roundTrip(doc);

		Region[] after = this.regions(back);
		Assert.Equal(before.Length, after.Length);
		Assert.Equal(before[0].AcisData, after[0].AcisData);
	}

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	public void ABinaryPayloadSurvivesADwgRoundTrip(ACadVersion version)
	{
		//R2004 and R2010 keep the payload inside the entity. Measured in AutoCAD 2027 on a
		//production drawing with 32 regions: written at either version the drawing opens, audits to
		//the same 5 errors it audited to before the change, and AutoCAD's own export of it has all
		//32 regions back.
		CadDocument doc = DwgReader.Read(sampleR2004);
		Region[] before = this.regions(doc);
		Assert.NotEmpty(before);
		Assert.True(before[0].IsBinaryAcisData);

		doc.Header.Version = version;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		Region[] after = this.regions(DwgReader.Read(new MemoryStream(stream.ToArray())));
		Assert.Equal(before.Length, after.Length);
		Assert.Equal(before[0].AcisData, after[0].AcisData);
	}

	[Theory]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1032)]
	public void TheVersionsThatCannotCarryThePayloadLeaveTheRegionOutAndSaySo(ACadVersion version)
	{
		//R2000 wants SAT text in the entity and refuses every file this writer produced that way;
		//R2013+ wants the payload in the AcDs data section, which this writer does not produce. A
		//region without its geometry is a handle with no shape, so it is left out - said, not hidden.
		CadDocument doc = DwgReader.Read(sampleR2004);
		Assert.NotEmpty(this.regions(doc));

		doc.Header.Version = version;
		string reported = null;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.OnNotification += (s, e) => { if (e.Message.Contains("is not written to a")) reported = e.Message; };
			writer.Write();
		}

		Assert.NotNull(reported);
		Assert.Empty(this.regions(DwgReader.Read(new MemoryStream(stream.ToArray()))));
	}

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1032)]
	public void ARegionWithNoGeometryIsNotWrittenToADwg(ACadVersion version)
	{
		//A region read from an R2013+ DWG has no payload: the geometry is in the AcDs data section,
		//which the reader does not read. Written as an empty region it is a handle with no shape,
		//and AutoCAD refuses the whole drawing - `sample_AC1032` round-tripped to R2018 would not
		//open at all until this case was caught.
		CadDocument doc = DwgReader.Read(sampleR2004);
		Region region = this.regions(doc).First();
		region.AcisData = null;

		doc.Header.Version = version;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		Assert.Empty(this.regions(DwgReader.Read(new MemoryStream(stream.ToArray()))));
	}

	private Region[] regions(CadDocument doc)
	{
		return doc.BlockRecords
			.SelectMany(b => b.Entities.OfType<Region>())
			.OrderBy(r => r.Handle)
			.ToArray();
	}

	private string write(CadDocument doc)
	{
		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.Write();
		}

		return System.Text.Encoding.UTF8.GetString(stream.ToArray());
	}

	private CadDocument roundTrip(CadDocument doc)
	{
		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.Write();
		}

		return DxfReader.Read(new MemoryStream(stream.ToArray()));
	}
}
