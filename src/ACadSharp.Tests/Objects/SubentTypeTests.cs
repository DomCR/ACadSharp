using ACadSharp.Objects;
using Xunit;

namespace ACadSharp.Tests.Objects;

public class SubentTypeTests
{
	[Theory]
	[InlineData(SubentType.Unknown, 0)]
	[InlineData(SubentType.Face, 1)]
	[InlineData(SubentType.Edge, 2)]
	[InlineData(SubentType.Vertex, 3)]
	public void ValuesMatchAutoCAD(SubentType type, short value)
	{
		//AutoCAD writes 73 = 2 (kEdgeSubentType) for every osnap reference to a LINE, LWPOLYLINE, ARC or CIRCLE.
		Assert.Equal(value, (short)type);
	}
}
