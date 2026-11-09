using ACadSharp.Objects;
using ACadSharp.Types.Units;

namespace ACadSharp.Extensions;

public static class UnitExtensions
{
	/// <summary>
	/// Converts a <see cref="PlotPaperUnits"/> to a <see cref="UnitsType"/>.
	/// </summary>
	/// <param name="units">The plot paper units to convert.</param>
	/// <returns>The corresponding units type.</returns>
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
