using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class DynamicBlockTests : IOTestsBase
	{
		public static TheoryData<FileModel> DynamicBlocksPaths { get; } = new();

		static DynamicBlockTests()
		{
			loadSamples("dynamic-blocks", "dwg", DynamicBlocksPaths);
		}

		public DynamicBlockTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DynamicBlocksPaths))]
		public void DynamicBlocksTest(FileModel test)
		{
			//"my-dynamic-block" handle = 570

			DwgReaderConfiguration configuration = new DwgReaderConfiguration();
			configuration.KeepUnknownEntities = true;
			configuration.KeepUnknownNonGraphicalObjects = true;

			CadDocument doc = DwgReader.Read(test.Path, configuration, this.onNotification);

			BlockRecord blk = doc.BlockRecords["my-dynamic-block"];

			//Dictionary entry
			EvaluationGraph eval = blk.XDictionary.GetEntry<EvaluationGraph>("ACAD_ENHANCEDBLOCK");

			//Extended data related to the dynamic block
			var a = blk.ExtendedData.Get(doc.AppIds["AcDbBlockRepETag"]);
			var b = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockTrueName"]);
			var c = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockGUID"]);

			Insert basic = doc.GetCadObject<Insert>(788);
			Insert modified = doc.GetCadObject<Insert>(889);
		}
	}
}
