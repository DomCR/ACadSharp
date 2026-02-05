using ACadSharp.Objects;
using ACadSharp.Types.Units;

namespace ACadSharp.Extensions
{
	public static class UnitExtensions
	{
		public static UnitsType ToUnits(this PlotPaperUnits units)
		{
			switch (units)
			{
				case PlotPaperUnits.Inches:
					return UnitsType.Inches;
				case PlotPaperUnits.Millimeters:
					return UnitsType.Millimeters;
				case PlotPaperUnits.Pixels:
				default:
					return UnitsType.Unitless;
			}
		}
	}
}
