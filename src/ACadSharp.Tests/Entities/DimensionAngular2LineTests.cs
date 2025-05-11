using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Tests.Entities
{
	public class DimensionAngular2LineTests : CommonDimensionTests<DimensionAngular2Line>
	{
		public override DimensionType Type => DimensionType.Angular;

		protected override DimensionAngular2Line createDim()
		{
			DimensionAngular2Line angular = new DimensionAngular2Line();
			angular.FirstPoint = XYZ.Zero;
			angular.SecondPoint = XYZ.AxisX;

			angular.DefinitionPoint = XYZ.Zero;
			angular.AngleVertex = XYZ.AxisY;

			return angular;
		}
	}
}
