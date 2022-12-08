using CSMath;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ACadSharp.Entities
{
	public interface IPolyline : IEntity
	{
		bool IsClosed { get; }

		IEnumerable<IVertex> Vertices { get; }

		double Elevation { get; }

		XYZ Normal { get; }

		double Thickness { get; }
#if NET48

#else
		IEnumerable<Entity> Explode()
		{
			List<Entity> entities = new List<Entity>();

			for (int i = 0; i < Vertices.Count(); i++)
			{
				IVertex curr = this.Vertices.ElementAt(i);
				IVertex next = this.Vertices.ElementAtOrDefault(i + 1);

				if (next == null && this.IsClosed)
				{
					next = Vertices.First();
				}
				else if (next == null)
				{
					break;
				}

				Entity e = null;
				if (curr.Bulge == 0)
				{
					//Is a line
					e = new Line
					{
						StartPoint = new XYZ(curr.Location.GetComponents()),
						EndPoint = new XYZ(next.Location.GetComponents())
					};
				}
				else
				{
					XY p1 = new XY(curr.Location.GetComponents());
					XY p2 = new XY(next.Location.GetComponents());

					XY center = MathUtils.GetCenter(p1, p2, curr.Bulge, out double r);

					double startAngle;
					double endAngle;
					if (curr.Bulge > 0)
					{
						startAngle = p2.Substract(center).GetAngle();
						endAngle = p1.Substract(center).GetAngle();
					}
					else
					{
						startAngle = p1.Substract(center).GetAngle();
						endAngle = p2.Substract(center).GetAngle();
					}

					//Is an arc
					e = new Arc
					{
						Center = new XYZ(center.X, center.Y, this.Elevation),
						Normal = this.Normal,
						Radius = r,
						StartAngle = startAngle,
						EndAngle = endAngle,
						Thickness = this.Thickness,
					};
				}

				this.MatchProperties(e);

				entities.Add(e);
			}

			return entities;
		}
#endif
	}
}
