using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests
{
	public class DxfPropertyTests
	{
		[Fact]
		public void SetValueTest()
		{
			Line line = new Line();

			XYZ manual = new XYZ(1, 1, 1);
			XYZ dxfProp = new XYZ(2, 2, 2);

			line.StartPoint = manual;

			DxfClassMap map = DxfClassMap.Create<Line>();

			map.DxfProperties[10].SetValue(line, dxfProp.X);

			Assert.Equal(dxfProp.X, line.StartPoint.X);
		}
	}
}
