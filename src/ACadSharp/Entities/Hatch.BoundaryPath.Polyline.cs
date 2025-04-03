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
				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.Polyline;

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

				/// <summary>
				/// Bulges applied to each vertice, the number of bulges must be equal to the vertices or empty.
				/// </summary>
				/// <remarks>
				/// default value, 0 if not set
				/// </remarks>
				[DxfCodeValue(DxfReferenceType.Optional, 42)]
				public IEnumerable<double> Bulges { get { return this.Vertices.Select(v => v.Z); } }

				/// <summary>
				/// Position values are only X and Y.
				/// </summary>
				/// <remarks>
				/// The vertex bulge is stored in the Z component.
				/// </remarks>
				[DxfCodeValue(DxfReferenceType.Count, 93)]
				public List<XYZ> Vertices { get; set; } = new();

				/// <inheritdoc/>
				public override Entity ToEntity()
				{
					List<Vertex> vertices = new();
					foreach (XYZ v in this.Vertices)
					{
						vertices.Add(new Vertex2D(v));
					}

					return new Polyline2D(vertices.Cast<Vertex2D>(), this.IsClosed);
				}

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					throw new System.NotImplementedException();
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return BoundingBox.FromPoints(this.Vertices);
				}
			}
		}
	}
}
