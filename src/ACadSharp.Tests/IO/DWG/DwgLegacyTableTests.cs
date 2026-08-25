using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

/// <summary>
/// A table is stored one way up to R2007 and another from R2010, and this library reads both but
/// writes only the second. That was known and recorded as a limit; what was not known is what it
/// costs. Writing a table read from the older layout into an R2010+ file does not lose the table -
/// it loses the drawing: AutoCAD 2027 rejects the file outright at R2010 (ErrorStatus 53) and was
/// still working on it after ten minutes at R2018. With the two tables of `sample_AC1018` left out,
/// the same write opens and audits 0 in two seconds.
/// </summary>
public class DwgLegacyTableTests
{
	//R2004: its tables are stored in the pre-R2010 layout.
	private static string sampleR2004 => Path.Combine(TestVariables.SamplesFolder, "sample_AC1018.dwg");

	//The same drawing saved at R2018, where the tables use the layout this writer produces.
	private static string sampleR2018 => Path.Combine(TestVariables.SamplesFolder, "sample_AC1032.dwg");

	[Fact]
	public void ACellReadFromTheOlderLayoutSaysWhatItHolds()
	{
		//The older layout has no content type - the cell type implies it - so the reader filled the
		//value and left the type at its default, Unknown. Every writer then took that at its word
		//and wrote a cell holding nothing, dropping the text it had just read.
		TableEntity table = this.tables(DwgReader.Read(sampleR2004)).First();

		TableEntity.CellContent content = table.Rows
			.SelectMany(r => r.Cells)
			.SelectMany(c => c.Contents)
			.First(c => !string.IsNullOrEmpty(c.CadValue?.Value?.ToString()));

		Assert.Equal(TableEntity.TableCellContentType.Value, content.ContentType);
	}

	[Fact]
	public void TheOlderLayoutIsReadWithItsRowsColumnsAndText()
	{
		//Guard for the test above: it only means anything while the older layout is read at all.
		TableEntity table = this.tables(DwgReader.Read(sampleR2004)).First();

		Assert.Equal(7, table.Rows.Count);
		Assert.Equal(3, table.Columns.Count);
		Assert.Contains(table.Rows.SelectMany(r => r.Cells).SelectMany(c => c.Contents),
			c => c.CadValue?.Value?.ToString() == "Table sample");
	}

	[Theory]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void ATableFromTheOlderLayoutIsLeftOutOfAnR2010PlusFileAndSaidSo(ACadVersion version)
	{
		//One entity reported is the whole point: written instead, the drawing does not open.
		CadDocument doc = DwgReader.Read(sampleR2004);
		Assert.NotEmpty(this.tables(doc));

		doc.Header.Version = version;
		string reported = null;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.OnNotification += (s, e) => { if (e.Message.Contains("is not written to a")) reported = e.Message; };
			writer.Write();
		}

		Assert.NotNull(reported);
		Assert.Contains("layout", reported);
		Assert.Empty(this.tables(DwgReader.Read(new MemoryStream(stream.ToArray()))));
	}

	[Theory]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1032)]
	public void ATableFromTheR2010LayoutIsStillWritten(ACadVersion version)
	{
		//The guard has to tell the two layouts apart, or it takes every table with it. ValueFlag is
		//what separates them: the older layout carries the field, the R2010 one does not, and no
		//writer here writes it back.
		CadDocument doc = DwgReader.Read(sampleR2018);
		TableEntity[] before = this.tables(doc);
		Assert.NotEmpty(before);
		Assert.All(before, t => Assert.Equal(0, t.ValueFlag));

		doc.Header.Version = version;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		Assert.Equal(before.Length, this.tables(DwgReader.Read(new MemoryStream(stream.ToArray()))).Length);
	}

	[Fact]
	public void TheRestOfTheDrawingSurvivesTheTablesBeingLeftOut()
	{
		//Leaving the table out has to cost the table and nothing else. Counting against the document
		//that was read would measure every other gap this writer has as well, so the control is the
		//same drawing with the tables taken out by hand: whatever else the writer keeps or drops,
		//the two runs have to agree.
		int withGuard = this.nonTableCountAfterRoundTrip(DwgReader.Read(sampleR2004), false);
		int control = this.nonTableCountAfterRoundTrip(DwgReader.Read(sampleR2004), true);

		Assert.Equal(control, withGuard);
	}

	private int nonTableCountAfterRoundTrip(CadDocument doc, bool removeTablesFirst)
	{
		if (removeTablesFirst)
		{
			foreach (var record in doc.BlockRecords)
			{
				foreach (TableEntity table in record.Entities.OfType<TableEntity>().ToList())
				{
					record.Entities.Remove(table);
				}
			}
		}

		doc.Header.Version = ACadVersion.AC1032;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		return DwgReader.Read(new MemoryStream(stream.ToArray()))
			.BlockRecords.SelectMany(b => b.Entities)
			.Count(e => !(e is TableEntity));
	}

	[Fact]
	public void ATableBuiltByACallerIsNotDroppedForSettingValueFlag()
	{
		//The guard was first keyed on ValueFlag being non-zero, which reads like provenance and is
		//not: the flag is public, DXF-mapped and documented as normally carrying 0x06, so a caller
		//building a table faithfully had it silently dropped from every DWG. It is keyed on
		//provenance recorded by the reader now, and a caller-built table survives whatever it sets.
		CadDocument doc = DwgReader.Read(sampleR2018);
		TableEntity table = this.tables(doc).First();
		int before = this.tables(doc).Length;
		Assert.True(before > 0);

		foreach (TableEntity each in this.tables(doc))
		{
			each.ValueFlag = 0x06;
		}

		doc.Header.Version = ACadVersion.AC1032;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		Assert.Equal(before, this.tables(DwgReader.Read(new MemoryStream(stream.ToArray()))).Length);
	}

	private TableEntity[] tables(CadDocument doc)
	{
		return doc.BlockRecords
			.SelectMany(b => b.Entities.OfType<TableEntity>())
			.OrderBy(t => t.Handle)
			.ToArray();
	}
}
