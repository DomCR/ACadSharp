using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
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

					clone.Vertices = new List<XYZ>(Vertices);

					return clone;
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return BoundingBox.FromPoints(this.Vertices);
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
}