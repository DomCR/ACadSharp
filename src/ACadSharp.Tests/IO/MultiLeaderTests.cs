using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class MultiLeaderTests : IOTestsBase
	{
		public static TheoryData<FileModel> MultiLeaderFilePaths { get; } = new();

		static MultiLeaderTests()
		{
			//loadSamples("./", "dxf", MultiLeaderFilePaths);
			loadSamples("./", "dwg", MultiLeaderFilePaths);
		}

		public MultiLeaderTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(MultiLeaderFilePaths))]
		public void MultiLeaderDwg(FileModel test)
		{
			CadDocument doc;

			if (test.IsDxf)
			{
				doc = DxfReader.Read(test.Path);
			}
			else
			{
				doc = DwgReader.Read(test.Path);
			}

			MultiLeader multiLeader;

			multiLeader = doc.GetCadObject<MultiLeader>(0xB1A);
			Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB1B);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB1C);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Center, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB1D);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB1E);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB1F);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB20);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB21);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB22);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB23);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB24);
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB25);
			Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.BottomLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = doc.GetCadObject<MultiLeader>(0xB26);
			Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.TopOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			if (doc.Header.Version > ACadVersion.AC1021)
			{
				//For some reason this entity is not compatible for versions before AC1021
				multiLeader = doc.GetCadObject<MultiLeader>(0xB27);
				Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
				Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
				Assert.Equal(TextAttachmentType.TopOfTopLine, multiLeader.TextLeftAttachment);
				Assert.False(multiLeader.TextFrame);
				Assert.Equal(8, multiLeader.LandingDistance);
				Assert.Equal(TextAttachmentDirectionType.Vertical, multiLeader.TextAttachmentDirection);
			}
		}
	}
}
