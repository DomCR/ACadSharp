using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using CSMath;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF
{
	/// <summary>
	/// A drawing does not have to spell its model space block "*Model_Space": AutoCAD matches the
	/// name without case, and real drawings carry "*MODEL_SPACE". The DXF writer compared the name
	/// exactly, so for such a drawing every model entity was written inside the BLOCKS section as
	/// an ordinary block and the ENTITIES section came out empty - AutoCAD still draws it, but any
	/// consumer that reads the ENTITIES section sees an empty drawing.
	/// </summary>
	public class DxfModelSpaceNameTests
	{
		[Fact]
		public void ModelSpaceNamedInUpperCaseStillWritesItsEntitiesToTheEntitiesSection()
		{
			CadDocument doc = DxfReader.Read(new MemoryStream(Encoding.ASCII.GetBytes(minimalDxf("*MODEL_SPACE"))));

			Assert.Single(doc.ModelSpace.Entities);

			string written = writeToString(doc);

			Assert.Equal(1, countInSection(written, "ENTITIES", "LINE"));
			Assert.Equal(0, countInSection(written, "BLOCKS", "LINE"));
		}

		[Fact]
		public void ModelSpaceNamedTheUsualWayIsUnchanged()
		{
			CadDocument doc = DxfReader.Read(new MemoryStream(Encoding.ASCII.GetBytes(minimalDxf("*Model_Space"))));

			string written = writeToString(doc);

			Assert.Equal(1, countInSection(written, "ENTITIES", "LINE"));
			Assert.Equal(0, countInSection(written, "BLOCKS", "LINE"));
		}

		[Fact]
		public void AnOrdinaryBlockKeepsItsEntitiesInTheBlocksSection()
		{
			CadDocument doc = new CadDocument();
			BlockRecord block = new BlockRecord("DOOR");
			block.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(1, 0, 0)));
			doc.BlockRecords.Add(block);

			string written = writeToString(doc);

			Assert.Equal(1, countInSection(written, "BLOCKS", "LINE"));
			Assert.Equal(0, countInSection(written, "ENTITIES", "LINE"));
		}

		private static string writeToString(CadDocument doc)
		{
			byte[] bytes;
			using (MemoryStream stream = new MemoryStream())
			using (DxfWriter writer = new DxfWriter(stream, doc, false))
			{
				writer.Write();
				bytes = stream.ToArray();
			}

			return Encoding.UTF8.GetString(bytes);
		}

		private static int countInSection(string dxf, string section, string entity)
		{
			string[] lines = dxf.Replace("\r\n", "\n").Split('\n');
			string current = null;
			bool expectName = false;
			int count = 0;
			for (int i = 0; i + 1 < lines.Length; i += 2)
			{
				string code = lines[i].Trim();
				string value = lines[i + 1].Trim();
				if (code == "0" && value == "SECTION") { expectName = true; continue; }
				if (code == "2" && expectName) { current = value; expectName = false; continue; }
				if (code == "0" && value == "ENDSEC") { current = null; continue; }
				if (code == "0" && value == entity && current == section) { count++; }
			}

			return count;
		}

		private static string minimalDxf(string modelSpaceName)
		{
			//Only what the reader needs: the block record table with the model space record, the
			//block that carries one line, and an empty entities section.
			return string.Join("\r\n", new[]
			{
				"0", "SECTION", "2", "TABLES",
				"0", "TABLE", "2", "BLOCK_RECORD", "5", "1", "100", "AcDbSymbolTable", "70", "1",
				"0", "BLOCK_RECORD", "5", "1F", "100", "AcDbSymbolTableRecord", "100", "AcDbBlockTableRecord", "2", modelSpaceName,
				"0", "ENDTAB",
				"0", "ENDSEC",
				"0", "SECTION", "2", "BLOCKS",
				"0", "BLOCK", "5", "20", "330", "1F", "100", "AcDbEntity", "8", "0", "100", "AcDbBlockBegin",
				"2", modelSpaceName, "70", "0", "10", "0.0", "20", "0.0", "30", "0.0", "3", modelSpaceName, "1", "",
				"0", "LINE", "5", "21", "330", "1F", "100", "AcDbEntity", "8", "0", "100", "AcDbLine",
				"10", "0.0", "20", "0.0", "30", "0.0", "11", "10.0", "21", "0.0", "31", "0.0",
				"0", "ENDBLK", "5", "22", "330", "1F", "100", "AcDbEntity", "8", "0", "100", "AcDbBlockEnd",
				"0", "ENDSEC",
				"0", "SECTION", "2", "ENTITIES", "0", "ENDSEC",
				"0", "EOF",
				string.Empty,
			});
		}
	}
}
