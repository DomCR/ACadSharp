using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class PolygonMeshTests : CommonPolylineTests<PolygonMesh, PolygonMeshVertex>
	{
		[Fact]
		public void ContainsTypeFlagTest()
		{
			PolygonMesh polyline = new PolygonMesh();

			Assert.False(polyline.Flags.HasFlag(PolylineFlags.Polyline3D));
			Assert.True(polyline.Flags.HasFlag(PolylineFlags.PolygonMesh));
			Assert.False(polyline.Flags.HasFlag(PolylineFlags.PolyfaceMesh));
		}
	}
}