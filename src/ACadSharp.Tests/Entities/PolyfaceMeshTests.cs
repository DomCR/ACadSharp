using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class PolyfaceMeshTests : CommonPolylineTests<PolyfaceMesh, VertexFaceMesh>
	{
		[Fact]
		public void ContainsTypeFlagTest()
		{
			PolyfaceMesh polyline = new PolyfaceMesh();

			Assert.False(polyline.Flags.HasFlag(PolylineFlags.Polyline3D));
			Assert.False(polyline.Flags.HasFlag(PolylineFlags.PolygonMesh));
			Assert.True(polyline.Flags.HasFlag(PolylineFlags.PolyfaceMesh));
		}
	}
}