using ACadSharp.Attributes;
using ACadSharp.Extensions;
using CSMath;
using CSMath.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities;

public partial class Hatch
{
	public partial class BoundaryPath
	{
		public class Polyline : Edge
		{
			/// <summary>
			/// Bulges applied to each vertice, the number of bulges must be equal to the vertices or empty.
			/// </summary>
			/// <remarks>
			/// default value, 0 if not set
			/// </remarks>
			[DxfCodeValue(DxfReferenceType.Optional, 42)]
			public IEnumerable<double> Bulges { get { return this.Vertices.Select(v => v.Z); } }

			/// <summary>
			/// The polyline has bulges with value different than 0.
			/// </summary>
			[DxfCodeValue(72)]
			public bool HasBulge => this.Bulges.Any(b => b != 0);

			/// <summary>
			/// Is closed flag.
			/// </summary>
			[DxfCodeValue(73)]
			public bool IsClosed { get; set; }

			/// <inheritdoc/>
			public override EdgeType Type => EdgeType.Polyline;

			/// <summary>
			/// Position values are only X and Y.
			/// </summary>
			/// <remarks>
			/// The vertex bulge is stored in the Z component.
			/// </remarks>
			[DxfCodeValue(DxfReferenceType.Count, 93)]
			public List<XYZ> Vertices { get; private set; } = new();

			/// <summary>
			/// Initializes a new instance of the Polyline class.
			/// </summary>
			public Polyline()
			{ }

			/// <summary>
			/// Initializes a new instance of the Polyline class with the specified vertices and closure state.
			/// </summary>
			/// <param name="vertices">The collection of points that define the vertices of the polyline. The order of the points determines the
			/// sequence of the polyline's segments. Cannot be null or empty.</param>
			/// <param name="isClosed">true to create a closed polyline where the last vertex connects to the first; otherwise, false.</param>
			public Polyline(IEnumerable<XYZ> vertices, bool isClosed = true)
			{
				this.Vertices.AddRange(vertices);
				this.IsClosed = isClosed;
			}

			/// <summary>
			/// Initializes a new instance of the Polyline class by copying the vertices and closed state from the specified
			/// polyline.
			/// </summary>
			/// <param name="polyline">The source polyline whose vertices and closed state are used to initialize the new instance. Cannot be null.</param>
			public Polyline(IPolyline polyline)
			{
				foreach (var v in polyline.Vertices)
				{
					XY xy = v.Location.Convert<XY>();
					this.Vertices.Add(new XYZ(xy.X, xy.Y, v.Bulge));
				}

				this.IsClosed = polyline.IsClosed;
			}

			/// <inheritdoc/>
			public override void ApplyTransform(Transform transform)
			{
				var arr = this.Vertices.ToArray();
				this.Vertices.Clear();
				for (int i = 0; i < arr.Length; i++)
				{
					var bulge = arr[i].Z;
					var v = transform.ApplyTransform(arr[i]);
					v.Z = bulge;

					this.Vertices.Add(v);
				}
			}

			/// <inheritdoc/>
			public override Edge Clone()
			{
				Polyline clone = (Polyline)base.Clone();

				clone.Vertices = new List<XYZ>(this.Vertices);

				return clone;
			}

			/// <inheritdoc/>
			public override IEnumerable<XY> FindIntersections(Line2D line)
			{
				var pline = this.ToEntity() as Polyline2D;
				var entities = pline.Explode();

				foreach (var entity in entities)
				{
					if (entity is Entities.Line l)
					{
						var s = l.ToSegment3D();
						var seg2d = new Segment2D(s.Origin.Convert<XY>(), s.End.Convert<XY>());
						if (seg2d.TryFindIntersection(line, out XY intersection))
						{
							yield return intersection;
						}
					}
					else if (entity is Entities.Arc arc)
					{
						var a = arc.ToArc2D();
						foreach (var intersection in a.FindIntersections(line))
						{
							yield return intersection;
						}
					}
				}
			}

			/// <inheritdoc/>
			public override BoundingBox GetBoundingBox()
			{
				//"The vertex bulge is stored in the Z component" - the remark on Vertices, a few
				//lines up. Handing them to FromPoints as if they were points built a box whose Z
				//range was a range of BULGES, so a hatch with a polyline boundary reported a height
				//it does not have. The spline edge carried the identical fault with its weights and
				//was fixed; this one was missed. Measured on the client corpus: 2,106 of 17,315
				//polyline boundary edges reported such a range.
				//
				//The bulges are not merely discarded either: a bulged segment bows outside its own
				//chord, so a box taken from the vertices alone is too small in X and Y as well. Each
				//one is measured through Arc, which is how the rest of the library measures an arc.
				BoundingBox box = BoundingBox.FromPoints(this.Vertices.Select(v => new XYZ(v.X, v.Y, 0)));

				for (int i = 0; i < this.Vertices.Count; i++)
				{
					XYZ curr = this.Vertices[i];
					bool last = i == this.Vertices.Count - 1;
					if (last && !this.IsClosed)
					{
						break;
					}

					XYZ next = this.Vertices[last ? 0 : i + 1];

					//A repeated vertex leaves no chord for an arc to span, and Arc refuses the zero
					//radius its own maths then produces.
					if (curr.Z == 0 || (curr.X == next.X && curr.Y == next.Y))
					{
						continue;
					}

					BoundingBox arc = ACadSharp.Entities.Arc
						.CreateFromBulge(new XY(curr.X, curr.Y), new XY(next.X, next.Y), curr.Z)
						.GetBoundingBox();

					box = box.Merge(new BoundingBox(
						new XYZ(arc.Min.X, arc.Min.Y, 0),
						new XYZ(arc.Max.X, arc.Max.Y, 0)));
				}

				return box;
			}

			private IEnumerable<XYZ> arcAwarePoints()
			{
				for (int i = 0; i < this.Vertices.Count; i++)
				{
					XYZ curr = this.Vertices[i];
					yield return new XYZ(curr.X, curr.Y, 0);

					bool last = i == this.Vertices.Count - 1;
					if (last && !this.IsClosed)
					{
						continue;
					}

					XYZ next = this.Vertices[last ? 0 : i + 1];

					//A repeated vertex leaves no chord for an arc to span, and Arc refuses the zero
					//radius its own maths then produces.
					if (curr.Z == 0 || (curr.X == next.X && curr.Y == next.Y))
					{
						continue;
					}

					foreach (XYZ p in ACadSharp.Entities.Arc
						.CreateFromBulge(new XY(curr.X, curr.Y), new XY(next.X, next.Y), curr.Z)
						.PolygonalVertexes(16))
					{
						yield return new XYZ(p.X, p.Y, 0);
					}
				}
			}

			/// <inheritdoc/>
			public override Entity ToEntity()
			{
				List<Vertex> vertices = new();
				foreach (XYZ v in this.Vertices)
				{
					var vertex = new Vertex2D(v.Convert<XY>())
					{
						Bulge = v.Z,
					};
					vertices.Add(vertex);
				}

				return new Polyline2D(vertices.Cast<Vertex2D>(), this.IsClosed);
			}
		}
	}
}