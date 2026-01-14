using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
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
		[MemberData(nameof(GenericDynamicBlocksPaths))]
		public void DynamicBlocksTest(FileModel test)
		{
			CadDocument doc;

			if (test.IsDxf)
			{
				DxfReaderConfiguration configuration = new();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DxfReader.Read(test.Path, configuration, this.onNotification);

				if (doc.Header.Version <= ACadVersion.AC1021)
				{
					return;
				}
			}
			else
			{
				DwgReaderConfiguration configuration = new DwgReaderConfiguration();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DwgReader.Read(test.Path, configuration, this.onNotification);
			}

			string dynamicName = "my-dynamic-block";

			BlockRecord blk = doc.BlockRecords[dynamicName];

			Assert.True(blk.IsDynamic);

			//Dictionary entry
			EvaluationGraph eval = blk.XDictionary.GetEntry<EvaluationGraph>("ACAD_ENHANCEDBLOCK");

			//Extended data related to the dynamic block
			var a = blk.ExtendedData.Get(doc.AppIds["AcDbBlockRepETag"]);
			var b = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockTrueName"]);
			var c = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockGUID"]);

			Insert basic = doc.GetCadObject<Insert>(0xABA);
			Insert modified = doc.GetCadObject<Insert>(0xAC5);

			Assert.NotNull(modified.Block.Source);
			Assert.Equal(dynamicName, modified.Block.Source.Name);
		}

		[Theory]
		[MemberData(nameof(IsolatedDynamicBlocksPaths))]
		public void IsolatedTest(FileModel test)
		{
			var config = getConfiguration(test);
			var doc = this.readDocument(test, config);

			var original = doc.BlockRecords["block_visibility_parameter"];
			foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
			{
				Assert.Equal(original, record.Source);
			}

			foreach (Insert insert in doc.Entities.OfType<Insert>())
			{
				var dict = insert.XDictionary.GetEntry<CadDictionary>("AcDbBlockRepresentation");
				var representation = dict.GetEntry<BlockRepresentationData>("AcDbRepData");

				Assert.NotNull(representation);
				Assert.Equal(original, representation.Block);
			}
		}
	}
}
