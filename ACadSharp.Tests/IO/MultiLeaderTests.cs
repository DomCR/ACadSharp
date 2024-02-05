using ACadSharp.Entities;
using ACadSharp.IO;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class MultiLeaderTests : IOTestsBase
	{
		public MultiLeaderTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void MultiLeaderDwg()
		{
			string inPath = Path.Combine($"{samplesFolder}", "multileader", "sample_MLeader_AC1032.dwg");
			CadDocument doc = DwgReader.Read(inPath);

			// There are 14 multileaders in DWG file
			Assert.Equal(14, doc.Entities.Count);

			List<Entity> entities = new List<Entity>(doc.Entities);

			MultiLeader multiLeader;
			
			multiLeader = (MultiLeader)entities[0];
			Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[1];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[2];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Center, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[3];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[4];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[5];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[6];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[7];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[8];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[9];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[10];
			Assert.Equal(@"MULTILEADER\PTEST\P123", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Right, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.MiddleOfTopLine, multiLeader.TextLeftAttachment);
			Assert.True(multiLeader.TextFrame);
			Assert.Equal(16, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[11];
			Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.BottomLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[12];
			Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.TopOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Horizontal, multiLeader.TextAttachmentDirection);

			multiLeader = (MultiLeader)entities[13];
			Assert.Equal(@"MULTILEADER TEST", multiLeader.ContextData.TextLabel);
			Assert.Equal(TextAttachmentPointType.Left, multiLeader.TextAttachmentPoint);
			Assert.Equal(TextAttachmentType.TopOfTopLine, multiLeader.TextLeftAttachment);
			Assert.False(multiLeader.TextFrame);
			Assert.Equal(8, multiLeader.LandingDistance);
			Assert.Equal(TextAttachmentDirectionType.Vertical, multiLeader.TextAttachmentDirection);
		}
	}
}
