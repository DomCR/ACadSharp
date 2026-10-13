using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// An action in a dynamic block refers to the parameters and grips it is wired to, not only to
/// entities. The model held only entities, so on a production drawing 385 of 2,803 references in 118
/// actions were dropped on write - none of them an entity, all of them objects AutoCAD keeps.
/// </summary>
public class BlockActionReferencesTests
{
	private static string sample => Path.Combine(TestVariables.SamplesFolder, "dynamic-blocks", "BLOCKFLIPPARAMETER.dwg");

	//There is no DWG theory here yet: the DWG writer does not round trip the evaluation graph of
	//this sample at all (EvaluationGraph comes back null, measured on the commit before this
	//change too), so a DWG assertion would be testing that defect rather than this one. The DWG
	//writer change is covered by the production-drawing measurement in the commit message until
	//the graph round trip is fixed.

	[Fact]
	public void AnActionKeepsAReferenceToAParameterThroughDxf()
	{
		CadDocument doc = DwgReader.Read(sample);
		(BlockFlipAction action, BlockFlipParameter parameter) = this.wire(doc);

		MemoryStream ms = new MemoryStream();
		using (DxfWriter writer = new DxfWriter(ms, doc, false))
		{
			writer.Write();
		}

		this.assertWired(DxfReader.Read(new MemoryStream(ms.ToArray())), action.Elements.Count);
	}

	[Fact]
	public void EntitiesAddedOnlyToTheTypedListAreStillWritten()
	{
		//A caller that never heard of Elements and adds to Entities, as before, loses nothing.
		CadDocument doc = DwgReader.Read(sample);
		(BlockFlipAction action, _) = this.find(doc);
		int before = action.GetReferencedObjects().Count();

		var extra = new Circle { Radius = 1 };
		doc.Entities.Add(extra);
		action.Entities.Add(extra);

		Assert.Equal(before + 1, action.GetReferencedObjects().Count());
		Assert.Contains(extra, action.GetReferencedObjects());
	}

	private (BlockFlipAction, BlockFlipParameter) find(CadDocument doc)
	{
		EvaluationGraph graph = doc.BlockRecords["BLOCK_FLIP_PARAMETER"].EvaluationGraph;
		var expressions = graph.Nodes.Select(n => n.Expression).ToList();
		return (expressions.OfType<BlockFlipAction>().Single(), expressions.OfType<BlockFlipParameter>().Single());
	}

	private (BlockFlipAction, BlockFlipParameter) wire(CadDocument doc)
	{
		(BlockFlipAction action, BlockFlipParameter parameter) = this.find(doc);
		//The sample's action refers to entities only. Wire it to its parameter, which is what a
		//drawing authored in AutoCAD holds and what used to be dropped.
		action.Elements.Add(parameter);
		return (action, parameter);
	}

	private void assertWired(CadDocument doc, int expectedCount)
	{
		(BlockFlipAction action, BlockFlipParameter parameter) = this.find(doc);
		Assert.Contains(parameter, action.Elements);
		Assert.Equal(expectedCount, action.Elements.Count);
		Assert.DoesNotContain(action.Entities, e => ReferenceEquals(e, parameter));
	}
}
