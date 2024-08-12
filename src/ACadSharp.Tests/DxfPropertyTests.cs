using ACadSharp.Entities;
using CSMath;
using Xunit;

namespace ACadSharp.Tests
{
	public class DxfPropertyTests
	{
		[Fact]
		public void SetXYValueTest()
		{
			Viewport viewport = new Viewport();

			XY manual = new XY(1, 1);
			XY dxfProp = new XY(2, 2);

			viewport.SnapBase = manual;

			DxfClassMap map = DxfClassMap.Create<Viewport>();

			map.DxfProperties[13].SetValue(viewport, dxfProp.X);
			map.DxfProperties[23].SetValue(viewport, dxfProp.Y);

			Assert.Equal(dxfProp, viewport.SnapBase);
		}

		[Fact]
		public void SetXYZValueTest()
		{
			Line line = new Line();

			XYZ manual = new XYZ(1, 1, 1);
			XYZ dxfProp = new XYZ(2, 2, 2);

			line.StartPoint = manual;

			DxfClassMap map = DxfClassMap.Create<Line>();

			map.DxfProperties[10].SetValue(line, dxfProp.X);
			map.DxfProperties[20].SetValue(line, dxfProp.Y);
			map.DxfProperties[30].SetValue(line, dxfProp.Z);

			Assert.Equal(dxfProp, line.StartPoint);
		}
	}
}
