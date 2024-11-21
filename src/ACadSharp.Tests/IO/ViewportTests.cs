using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class ViewportTests : IOTestsBase
	{
		public ViewportTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		[MemberData(nameof(DxfAsciiFiles))]
		public void ScaleInViewport(FileModel test)
		{
			CadDocument doc = this.readDocument(test);

			ACadSharp.Tables.BlockRecord paper = doc.PaperSpace;
			foreach (Viewport v in paper.Viewports)
			{
				Assert.NotNull(v.Scale);
			}
		}
	}
}
