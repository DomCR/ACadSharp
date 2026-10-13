using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

/// <summary>
/// A table is stored one way up to R2007 and another from R2010, and this library reads both but
/// writes only the second. Writing a table read from the older layout into an R2010+ file
/// unconverted did not lose the table - it lost the drawing: AutoCAD 2027 rejected the file
/// outright at R2010 (ErrorStatus 53) and was still working on it after ten minutes at R2018. For
/// a while such tables were dropped with a notification; they are now converted on write, because
/// the real wall was never the missing R2010-only structures (all optional, measured by stripping
/// them from an R2018-authored table) but an empty-string cell value that writeStringCadValue
/// wrote with two lengths, shifting every field after it (T84).
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
	public void ATableFromTheOlderLayoutIsConvertedOnWriteAndSurvives(ACadVersion version)
	{
		//This used to assert the drop-with-a-notification, because written unconverted the
		//drawing did not open. The wall fell with the writeStringCadValue fix: the conversion
		//needs only the merge ranges and the merged-away cells' content, everything else the
		//R2010 layout carries is optional, and the converted file opens and audits 0 in AutoCAD
		//at all three versions (T84).
		CadDocument doc = DwgReader.Read(sampleR2004);
		int before = this.tables(doc).Length;
		Assert.NotEqual(0, before);

		doc.Header.Version = version;
		string converted = null;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.OnNotification += (s, e) => { if (e.Message.Contains("was converted")) converted = e.Message; };
			writer.Write();
		}

		Assert.NotNull(converted);
		TableEntity[] after = this.tables(DwgReader.Read(new MemoryStream(stream.ToArray())));
		Assert.Equal(before, after.Length);
		Assert.Contains(after.SelectMany(t => t.Rows).SelectMany(r => r.Cells).SelectMany(c => c.Contents),
			c => c.CadValue?.Value?.ToString() == "Table sample");
		Assert.Contains(after, t => t.MergedCellRanges.Count > 0);
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
	public void AnEmptyStringCellValueRoundTripsWithoutShiftingTheStream()
	{
		//writeStringCadValue used to write a bare zero length for an empty string AND then fall
		//through to the full write, so every field after the empty value was shifted by one bit
		//long plus two bytes: this library's own reader threw reading the file back and the
		//failsafe dropped the table, and AutoCAD worked on such a file for as long as it was
		//given. No R2010-authored table carries an empty String value - only one converted from
		//the pre-R2010 layout does - which is why T84's conversion experiments kept failing
		//while every ordinary round trip stayed green.
		CadDocument doc = DwgReader.Read(sampleR2018);
		TableEntity table = this.tables(doc).First();
		TableEntity.CellContent content = table.Rows.SelectMany(r => r.Cells)
			.SelectMany(c => c.Contents).First();
		content.CadValue.ValueType = CadValueType.String;
		content.CadValue.SetValue(string.Empty);
		content.CadValue.IsEmpty = false;
		int countBefore = this.tables(doc).Length;

		doc.Header.Version = ACadVersion.AC1032;
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		TableEntity[] after = this.tables(DwgReader.Read(new MemoryStream(stream.ToArray())));
		Assert.Equal(countBefore, after.Length);
		Assert.Equal(string.Empty, after.First().Rows.SelectMany(r => r.Cells)
			.SelectMany(c => c.Contents).First().CadValue.Value);
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
