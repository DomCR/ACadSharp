using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Extensions
{
	public static class PolylineExtensions
	{
		public static IEnumerable<T> GetPoints<T>(this IPolyline polyline)
			where T : IVector, new()
		{
			return polyline.Vertices.Select(v => v.Location.Convert<T>());
		}

		public static IEnumerable<T> GetPoints<T>(this IPolyline polyline, int precision)
			where T : IVector, new()
		{
			if (precision < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(precision), precision, "The arc precision must be equal or greater than two.");
			}

			var points = new List<T>();
			for (int i = 0; i < polyline.Vertices.Count(); i++)
			{
				IVertex curr = polyline.Vertices.ElementAt(i);
				IVertex next = polyline.Vertices.ElementAtOrDefault(i + 1);

				if (next == null && polyline.IsClosed)
				{
					next = polyline.Vertices.First();
				}
				else if (next == null)
				{
					break;
				}

				if (curr.Bulge == 0)
				{
					if (i == 0)
					{
						points.Add((T)curr.Location);
					}

					points.Add((T)next.Location);
				}
				else
				{
					XY p1 = curr.Location.Convert<XY>();
					XY p2 = next.Location.Convert<XY>();

					IEnumerable<T> lst = Arc.CreateFromBulge(p1, p2, curr.Bulge)
						.PolygonalVertexes(precision)
						.Select(p => p.Convert<T>());

					var f = lst.First().Round(8);
					var l = lst.Last().Round(8);

					if (i == 0)
					{
						points.AddRange(lst);
					}
					else if (f.Equals(curr.Location.Convert<T>().Round(8)))
					{
						points.AddRange(lst.Skip(1));
					}
					else if (l.Equals(curr.Location.Convert<T>().Round(8)))
					{
						lst = lst.Reverse();
						points.AddRange(lst.Skip(1));
					}
				}
			}

			return points;
		}
	}
}
