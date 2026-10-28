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
			// R12 has no annotation-scale references. Its actual paper-space
			// viewports must still be retained instead of testing a default viewport.
			if (doc.Header.Version == ACadVersion.AC1009) Assert.Null(v.Scale);
			else Assert.NotNull(v.Scale);
		}
	}
}
