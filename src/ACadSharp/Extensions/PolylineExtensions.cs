using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Extensions
{
	public static class PolylineExtensions
	{
		public static IEnumerable<XYZ> GetPoints(this IPolyline polyline)
		{
			return polyline.Vertices.Select(v => v.Location.Convert<XYZ>());
		}
	}
}
