using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities
{
	public class DimensionRadiusTests : CommonDimensionTests<DimensionRadius>
	{
		public override DimensionType Type => DimensionType.Radius;

		public override void CloneTest()
		{
			throw new System.NotImplementedException();
		}

		public override void GetBoundingBoxTest()
		{
			throw new System.NotImplementedException();
		}

		protected override DimensionRadius createDim()
		{
			var dim = new DimensionRadius()
			{
				AngleVertex = new CSMath.XYZ(5, 5, 0)
			};

			return dim;
		}
	}
}
