using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// A drawing that AutoCAD opens - repairing it as it goes - must not cost entities here. The
/// values used are the ones real drawings carry: a scale of 0 on a block reference, which AutoCAD
/// repairs and keeps.
/// </summary>
public class DamagedValueTests
{
	public static TheoryData<ACadVersion> Versions => new TheoryData<ACadVersion>
	{
		ACadVersion.AC1015,
		ACadVersion.AC1018,
		ACadVersion.AC1024,
		ACadVersion.AC1032,
	};

	[Theory]
	[MemberData(nameof(Versions))]
	public void DwgKeepsAnInsertWithAZeroScale(ACadVersion version)
	{
		CadDocument doc = this.documentWithZeroScale(version, "_zscale");

		MemoryStream ms = new MemoryStream();
		using (DwgWriter writer = new DwgWriter(ms, doc))
		{
			writer.Write();
		}

		CadDocument read = DwgReader.Read(new MemoryStream(ms.ToArray()));
		Insert insert = read.Entities.OfType<Insert>().Single();
		Assert.Equal("my_block", insert.Block.Name);
		Assert.Equal(1, insert.ZScale);
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void DwgKeepsAnInsertWithAZeroXScale(ACadVersion version)
	{
		CadDocument doc = this.documentWithZeroScale(version, "_xscale");

		MemoryStream ms = new MemoryStream();
		using (DwgWriter writer = new DwgWriter(ms, doc))
		{
			writer.Write();
		}

		CadDocument read = DwgReader.Read(new MemoryStream(ms.ToArray()));
		Insert insert = read.Entities.OfType<Insert>().Single();
		Assert.Equal(1, insert.XScale);
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void DxfKeepsAnInsertWithAZeroScale(ACadVersion version)
	{
		CadDocument doc = this.documentWithZeroScale(version, "_zscale");

		MemoryStream ms = new MemoryStream();
		using (DxfWriter writer = new DxfWriter(ms, doc, false))
		{
			writer.Write();
		}

		CadDocument read = DxfReader.Read(new MemoryStream(ms.ToArray()));
		Insert insert = read.Entities.OfType<Insert>().Single();
		Assert.Equal("my_block", insert.Block.Name);
		Assert.Equal(1, insert.ZScale);
	}

	private CadDocument documentWithZeroScale(ACadVersion version, string field)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		BlockRecord block = new BlockRecord("my_block");
		block.Entities.Add(new Line(new CSMath.XYZ(0, 0, 0), new CSMath.XYZ(10, 10, 0)));
		doc.BlockRecords.Add(block);

		Insert insert = new Insert(block);
		doc.Entities.Add(insert);

		//The property refuses a scale of 0 on purpose - a drawing carrying one is damaged. The
		//point of the test is what happens when a file holds it anyway, so the field is set
		//directly, which is the only way to build that file from this side.
		typeof(Insert)
			.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
			.SetValue(insert, 0.0);

		return doc;
	}
}
