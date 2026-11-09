using ACadSharp.Entities;
using ACadSharp.IO;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class TableEntityTests : CommonEntityTests<TableEntity>
{
	public override void CloneTest()
	{
		base.CloneTest();
	}

	[Fact(Skip = "Not implemented")]
	public override void GetBoundingBoxTest()
	{
	}

	[Theory(Skip = "Not implemented")]
	[InlineData(CadFileFormat.DXF, ACadVersion.AC1014)]
	[InlineData(CadFileFormat.DXF, ACadVersion.AC1032)]
	[InlineData(CadFileFormat.DWG, ACadVersion.AC1032)]
	public override void ValidEntityTest(CadFileFormat format, ACadVersion version)
	{
	}
}