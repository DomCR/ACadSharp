using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities;

public abstract class CommonVertexTests<T> : CommonEntityTests<T>
	where T : Vertex, new()
{
	public abstract VertexFlags[] ValidFlags { get; }

	[Fact]
	public void FlagsTest()
	{
		T vertex = new T();
		foreach (var flag in this.ValidFlags)
		{
			Assert.True(vertex.Flags.HasFlag(flag));
		}
	}

	[Fact]
	public override void GetBoundingBoxTest()
	{
		var location = this._random.NextXYZ();
		T vertex = new T();
		vertex.Location = location;

		var box = vertex.GetBoundingBox();

		Assert.Equal(CSMath.BoundingBoxExtent.Point, box.Extent);
		Assert.Equal(location, box.Center);
	}
}