using ACadSharp.Entities;
using ACadSharp.Tests.TestModels;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO;

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

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	[MemberData(nameof(DxfAsciiFiles))]
	public void VisualStyleInViewport(FileModel test)
	{
		CadDocument doc = this.readDocument(test);

		if (doc.Header.Version < ACadVersion.AC1021)
		{
			return;
		}

		ACadSharp.Tables.BlockRecord paper = doc.PaperSpace;
		Assert.Contains(paper.Viewports, v => v.VisualStyle != null);
	}
}
