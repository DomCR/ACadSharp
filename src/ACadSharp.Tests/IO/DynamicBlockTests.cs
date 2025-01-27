using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class DynamicBlockTests : IOTestsBase
	{
		public static TheoryData<FileModel> DwgDynamicBlocksPaths { get; } = new();

		static DynamicBlockTests()
		{
			loadSamples("dynamic-blocks", "*", DwgDynamicBlocksPaths);
		}

		public DynamicBlockTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgDynamicBlocksPaths))]
		public void DynamicBlocksTest(FileModel test)
		{
			CadDocument doc;

			if (test.Extension == ".dxf")
			{
				DxfReaderConfiguration configuration = new();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DxfReader.Read(test.Path, configuration, this.onNotification);
			}
			else
			{
				DwgReaderConfiguration configuration = new DwgReaderConfiguration();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DwgReader.Read(test.Path, configuration, this.onNotification);
			}

			//"my-dynamic-block" handle = 570

			BlockRecord blk = doc.BlockRecords["my-dynamic-block"];

			Assert.True(blk.IsDynamic);

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
