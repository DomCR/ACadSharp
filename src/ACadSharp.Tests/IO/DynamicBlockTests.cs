using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO;

public class DynamicBlockTests : IOTestsBase
{
	public static TheoryData<FileModel> GenericDynamicBlocksPaths { get; } = new();

	public static TheoryData<FileModel> IsolatedDynamicBlocksPaths { get; } = new();

	static DynamicBlockTests()
	{
		loadSamples("./", "dxf", GenericDynamicBlocksPaths);
		loadSamples("./", "dwg", GenericDynamicBlocksPaths);

		loadSamples("./dynamic-blocks", "*dwg", IsolatedDynamicBlocksPaths);
		loadSamples("./dynamic-blocks", "*dxf", IsolatedDynamicBlocksPaths);
	}

	public DynamicBlockTests(ITestOutputHelper output) : base(output)
	{
	}

	[Theory]
	[MemberData(nameof(IsolatedDynamicBlocksPaths))]
	public void IsolatedTest(FileModel test)
	{
		var config = this.getReaderConfiguration(test);
		var doc = this.readDocument(test, true, config);

		switch (test.NoExtensionName)
		{
			case DxfFileToken.ObjectBlockBasePointParameter:
				this.assertBlockParameter(doc, "BASE_POINT_PARAMETER", typeof(BlockBasePointParameter));
				break;
			case DxfFileToken.ObjectBlockVisibilityParameter:
				this.assertBlockParameter(doc, "block_visibility_parameter", typeof(BlockVisibilityParameter));
				break;
			case DxfFileToken.ObjectBlockRotationParameter:
				this.assertBlockParameter(doc, "dynamic_block", typeof(BlockRotationParameter));
				break;
			case DxfFileToken.ObjectBlockPointParameter:
				this.assertBlockParameter(doc, "block_translation_parameter", typeof(BlockPointParameter));
				break;
			case DxfFileToken.ObjectBlockLinearParameter:
				this.assertBlockParameter(doc, "LINEAR_PARAM", typeof(BlockLinearParameter));
				break;
			case DxfFileToken.ObjectBlockLookupParameter:
				this.assertBlockParameter(doc, "My_Look_Block", typeof(BlockLookupParameter));
				break;
			case DxfFileToken.ObjectBlockAlignmentParameter:
				this.assertBlockParameter(doc, "ALIGNMENT_PARAMETER", typeof(BlockAlignmentParameter));
				break;
			case DxfFileToken.ObjectBlockFlipParameter:
				this.assertBlockParameter(doc, "BLOCK_FLIP_PARAMETER", typeof(BlockFlipParameter));
				break;
			case DxfFileToken.ObjectBlockPolarParameter:
				this.assertBlockParameter(doc, "BLOCK_POLAR_PARAMETER", typeof(BlockPolarParameter));
				break;
			default:
				throw new System.NotImplementedException();
		}
	}

	[Theory]
	[MemberData(nameof(IsolatedDynamicBlocksPaths))]
	public void RewriteIsolatedTest(FileModel test)
	{
		var readerConfiguration = this.getReaderConfiguration(test);
		var doc = this.readDocument(test, false, readerConfiguration);

		string file = Path.GetFileName(test.Path);
		string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, file);

		CadWriterConfiguration writerConfiguration = this.getWriterConfiguration(test);
		writerConfiguration.WriteDynamicBlockData = true;

		if (test.IsDxf)
		{
			DxfWriter.Write(pathOut, doc, configuration: writerConfiguration as DxfWriterConfiguration, notification: this.onNotification);
		}
		else
		{
			DwgWriter.Write(pathOut, doc, configuration: writerConfiguration as DwgWriterConfiguration, notification: this.onNotification);
		}
	}

	private void assertBlockParameter(CadDocument doc, string blockName, Type parameterType)
	{
		var original = doc.BlockRecords[blockName];
		foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
		{
			if (record.Source == null)
			{
				continue;
			}

			Assert.Equal(original, record.Source);
		}

		foreach (Insert insert in doc.Entities.OfType<Insert>())
		{
			if (insert.XDictionary == null)
			{
				continue;
			}
			var dict = insert.XDictionary.GetEntry<CadDictionary>("AcDbBlockRepresentation");
			var representation = dict.GetEntry<BlockRepresentationData>("AcDbRepData");

			Assert.NotEmpty(insert.Block.Source.EvaluationGraph.Nodes
				.Select(n => n.Expression)
				.Where(e => e.GetType() == parameterType));

			Assert.NotNull(representation);
			Assert.Equal(original, representation.Block);
			XRecord record = insert.XDictionary
				.GetEntry<CadDictionary>("AcDbBlockRepresentation")
				.GetEntry<CadDictionary>("AppDataCache")
				.GetEntry<CadDictionary>("ACAD_ENHANCEDBLOCKDATA")
				.OfType<XRecord>().First();
		}
	}
}