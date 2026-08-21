using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// The order a draw order table has in the file is part of what it says: where two entries carry the
/// same sort handle - which real drawings do, and AutoCAD repairs on open - the first one read wins.
/// Enumerating the table used to sort it in place, so reading a drawing reordered it and the writer
/// wrote that order back out.
/// </summary>
public class SortEntitiesTableOrderTests
{
	[Fact]
	public void EnumeratingDoesNotReorderTheTable()
	{
		CadDocument doc = new CadDocument();
		SortEntitiesTable table = this.table(doc, out Entity[] entities);

		ulong[] before = table.StoredOrder.Select(x => x.SortHandle).ToArray();
		_ = table.ToArray();
		_ = table.Count();

		Assert.Equal(before, table.StoredOrder.Select(x => x.SortHandle).ToArray());
	}

	[Fact]
	public void EnumerationIsStillSortedBySortHandle()
	{
		CadDocument doc = new CadDocument();
		SortEntitiesTable table = this.table(doc, out Entity[] entities);

		ulong[] enumerated = table.Select(s => s.SortHandle).ToArray();

		Assert.Equal(enumerated.OrderBy(h => h).ToArray(), enumerated);
	}

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1032)]
	public void DwgKeepsTheOrderTheDocumentHas(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;
		SortEntitiesTable table = this.table(doc, out Entity[] entities);
		ulong[] written = table.StoredOrder.Select(x => x.SortHandle).ToArray();

		MemoryStream ms = new MemoryStream();
		using (DwgWriter writer = new DwgWriter(ms, doc))
		{
			writer.Write();
		}

		CadDocument read = DwgReader.Read(new MemoryStream(ms.ToArray()));
		SortEntitiesTable back = (SortEntitiesTable)read.ModelSpace.XDictionary[SortEntitiesTable.DictionaryEntryName];

		Assert.Equal(written, back.StoredOrder.Select(x => x.SortHandle).ToArray());
	}

	private SortEntitiesTable table(CadDocument doc, out Entity[] entities)
	{
		entities = new Entity[]
		{
			new Line(new XYZ(0, 0, 0), new XYZ(1, 0, 0)),
			new Line(new XYZ(0, 1, 0), new XYZ(1, 1, 0)),
			new Line(new XYZ(0, 2, 0), new XYZ(1, 2, 0)),
		};
		foreach (Entity e in entities)
		{
			doc.Entities.Add(e);
		}

		SortEntitiesTable table = new SortEntitiesTable(doc.ModelSpace);
		//Descending on purpose: sorted order and stored order are then not the same list.
		table.Add(entities[0], 0x300);
		table.Add(entities[1], 0x200);
		table.Add(entities[2], 0x100);

		doc.ModelSpace.CreateExtendedDictionary().Add(SortEntitiesTable.DictionaryEntryName, table);

		return table;
	}
}
