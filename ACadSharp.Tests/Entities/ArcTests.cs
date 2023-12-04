using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class ArcTests
	{
		[Fact]
		public void CreateFromBulgeTest()
		{
			XY p1 = new XY();
			XY p2 = new XY(1, 0);
			double bulge = 0.5;

			var a = Arc.CreateFromBulge(p1, p2, bulge);

			a.GetEndVertices(out XYZ s, out XYZ e);
		}

		[Fact]
		public void FromBulgeTest()
		{
			Arc arc = new Arc();
			arc.GetEndVertices(out XYZ s1, out XYZ e1);
		
			XY p1 = new XY();
			XY p2 = new XY(1, 0);
			double bulge = 0.5;

			var a = Arc.CreateFromBulge(p1, p2, bulge);

			a.GetEndVertices(out XYZ s, out XYZ e);
		}
	}
}
