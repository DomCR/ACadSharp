using ACadSharp.IO;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO;

//The rewrite test in DynamicBlockTests writes every dynamic-block sample with the dynamic data
//switched on and stops there: nothing reads the file back. With the old default (off) a DWG written
//by a caller who set nothing came back with no evaluation graph at all, and every dynamic block in
//it was static from then on. These tests close that loop for both formats:
//the graph that comes out has the same nodes, in the same order, with the same expression types
//and flags, and the same edges, as the one that went in. AutoCAD audits every one of the ten
//written DWGs with 0 errors and still treats the block as dynamic (its own DXF save of the file
//keeps the ACAD_ENHANCEDBLOCK entry and the parameter and action objects).
public class DynamicBlockRoundTripTests : IOTestsBase
{
	public static TheoryData<FileModel> DynamicBlockDwgs { get; } = new();

	static DynamicBlockRoundTripTests()
	{
		loadSamples("./dynamic-blocks", "*dwg", DynamicBlockDwgs);
	}

	public DynamicBlockRoundTripTests(ITestOutputHelper output) : base(output)
	{
	}

	[Theory]
	[MemberData(nameof(DynamicBlockDwgs))]
	public void EvaluationGraphSurvivesDwg(FileModel test)
	{
		CadDocument doc = DwgReader.Read(test.Path);

		//Default configuration on purpose: this is what a caller who sets nothing gets.
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.OnNotification += this.onNotification;
			writer.Write();
		}

		CadDocument back = DwgReader.Read(new MemoryStream(stream.ToArray()), this.onNotification);
		this.assertSameGraphs(doc, back);
	}

	[Theory]
	[MemberData(nameof(DynamicBlockDwgs))]
	public void EvaluationGraphSurvivesDxf(FileModel test)
	{
		CadDocument doc = DwgReader.Read(test.Path);

		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.OnNotification += this.onNotification;
			writer.Write();
		}

		CadDocument back = DxfReader.Read(new MemoryStream(stream.ToArray()), this.onNotification);
		this.assertSameGraphs(doc, back);
	}

	[Theory]
	[MemberData(nameof(DynamicBlockDwgs))]
	public void SwitchedOffBothWritersLeaveTheGraphOut(FileModel test)
	{
		//The switch is on by default now; off, both writers drop the graph, the parameters and the
		//actions, and the block comes back static. The DXF writer used to ignore the switch and
		//write the data regardless, which is why a DXF kept the dynamic blocks a DWG lost.
		CadDocument doc = DwgReader.Read(test.Path);
		BlockRecord dynamicBlock = doc.BlockRecords.First(b => b.EvaluationGraph != null);

		using MemoryStream dwg = new();
		using (DwgWriter writer = new(dwg, doc))
		{
			writer.Configuration.WriteDynamicBlockData = false;
			writer.Write();
		}

		CadDocument backDwg = DwgReader.Read(new MemoryStream(dwg.ToArray()));
		Assert.Null(backDwg.BlockRecords[dynamicBlock.Name].EvaluationGraph);

		using MemoryStream dxf = new();
		using (DxfWriter writer = new(dxf, doc, false))
		{
			writer.Configuration.WriteDynamicBlockData = false;
			writer.Write();
		}

		CadDocument backDxf = DxfReader.Read(new MemoryStream(dxf.ToArray()));
		Assert.Null(backDxf.BlockRecords[dynamicBlock.Name].EvaluationGraph);
	}

	[Fact]
	public void DynamicBlockDataIsWrittenByDefault()
	{
		Assert.True(new DwgWriterConfiguration().WriteDynamicBlockData);
		Assert.True(new DxfWriterConfiguration().WriteDynamicBlockData);
	}

	private void assertSameGraphs(CadDocument expected, CadDocument actual)
	{
		BlockRecord[] dynamicBlocks = expected.BlockRecords.Where(b => b.EvaluationGraph != null).ToArray();
		Assert.NotEmpty(dynamicBlocks);

		foreach (BlockRecord block in dynamicBlocks)
		{
			EvaluationGraph before = block.EvaluationGraph;
			EvaluationGraph after = actual.BlockRecords[block.Name].EvaluationGraph;
			Assert.True(after != null, $"{block.Name}: the evaluation graph did not come back");

			EvaluationGraph.Node[] nodesBefore = before.Nodes.ToArray();
			EvaluationGraph.Node[] nodesAfter = after.Nodes.ToArray();
			Assert.Equal(nodesBefore.Length, nodesAfter.Length);
			for (int i = 0; i < nodesBefore.Length; i++)
			{
				Assert.Equal(nodesBefore[i].Id, nodesAfter[i].Id);
				Assert.Equal(nodesBefore[i].Flags, nodesAfter[i].Flags);
				Assert.Equal(nodesBefore[i].Expression?.GetType(), nodesAfter[i].Expression?.GetType());
			}

			Assert.Equal(before.Edges.Count, after.Edges.Count);
			for (int i = 0; i < before.Edges.Count; i++)
			{
				Assert.Equal(before.Edges[i].FromNodeIndex, after.Edges[i].FromNodeIndex);
				Assert.Equal(before.Edges[i].Index, after.Edges[i].Index);
				Assert.Equal(before.Edges[i].Flags, after.Edges[i].Flags);
			}
		}
	}
}
