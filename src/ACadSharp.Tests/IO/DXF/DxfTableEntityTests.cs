using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DXF
{
	/// <summary>
	/// A table is a block reference of the anonymous *T block that holds its drawn geometry, with
	/// the cell data on top. The writer used to skip the whole entity, so every table disappeared
	/// from a DXF: the block stayed in the file with nothing referencing it, and the drawn table
	/// was gone. It goes out as its block reference instead.
	/// </summary>
	public class DxfTableEntityTests
	{
		[Fact]
		public void TableIsWrittenAsTheBlockReferenceOfItsTableBlock()
		{
			CadDocument doc = new CadDocument();
			BlockRecord block = new BlockRecord("*T1");
			block.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(10, 0, 0)));
			doc.BlockRecords.Add(block);
			TableEntity table = new TableEntity(block)
			{
				InsertPoint = new XYZ(5, 7, 0),
			};
			doc.ModelSpace.Entities.Add(table);

			CadDocument result = writeAndRead(doc, out bool notified);

			Insert insert = Assert.Single(result.ModelSpace.Entities.OfType<Insert>());
			Assert.Equal("*T1", insert.Block.Name);
			Assert.Equal(5, insert.InsertPoint.X);
			Assert.Equal(7, insert.InsertPoint.Y);
			Assert.Empty(result.ModelSpace.Entities.OfType<TableEntity>());
			Assert.True(notified, "the loss of the cell data is notified");
		}

		[Fact]
		public void TableBlockContentSurvivesTheRoundTrip()
		{
			CadDocument doc = new CadDocument();
			BlockRecord block = new BlockRecord("*T2");
			block.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(10, 0, 0)));
			block.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(0, 4, 0)));
			doc.BlockRecords.Add(block);
			doc.ModelSpace.Entities.Add(new TableEntity(block));

			CadDocument result = writeAndRead(doc, out _);

			BlockRecord written = result.BlockRecords["*T2"];
			Assert.Equal(2, written.Entities.OfType<Line>().Count());
		}

		private static CadDocument writeAndRead(CadDocument doc, out bool notified)
		{
			bool raised = false;
			byte[] bytes;
			using (MemoryStream stream = new MemoryStream())
			using (DxfWriter writer = new DxfWriter(stream, doc, false))
			{
				writer.OnNotification += (s, e) =>
				{
					if (e.Message.Contains("cell values", System.StringComparison.OrdinalIgnoreCase))
					{
						raised = true;
					}
				};
				writer.Write();
				bytes = stream.ToArray();
			}

			notified = raised;
			return DxfReader.Read(new MemoryStream(bytes));
		}
	}
}
