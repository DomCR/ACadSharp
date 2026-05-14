using ACadSharp.Attributes;
using ACadSharp.Extensions;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath : IGeometricEntity
		{
			/// <summary>
			/// Edges that form the boundary.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 93)]
			public ObservableCollection<Edge> Edges { get; private set; } = new();

			/// <summary>
			/// Source boundary objects.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 97)]
			public List<Entity> Entities { get; set; } = new List<Entity>();

			/// <summary>
			/// Boundary path type flag
			/// </summary>
			[DxfCodeValue(92)]
			public BoundaryPathFlags Flags
			{
				get
				{
					if (this.IsPolyline)
					{
						this._flags.AddFlag(BoundaryPathFlags.Polyline);
					}
					else
					{
						this._flags.RemoveFlag(BoundaryPathFlags.Polyline);
					}

					return this._flags;
				}
				set
				{
					this._flags = value;
				}
			}

			/// <summary>
			/// Flag that indicates that this boundary path is formed by a polyline.
			/// </summary>
			public bool IsPolyline { get { return this.Edges.OfType<Polyline>().Any(); } }

			private BoundaryPathFlags _flags;

			/// <summary>
			/// Initializes a new instance of the BoundaryPath class.
			/// </summary>
			/// <remarks>Subscribes to changes in the Edges collection to keep the BoundaryPath instance updated when
			/// the collection is modified.</remarks>
			public BoundaryPath()
			{
				this.Edges.CollectionChanged += this.onEdgesCollectionChanged;
			}

			/// <summary>
			/// Initializes a new instance of the BoundaryPath class with the specified collection of edges.
			/// </summary>
			/// <param name="edges">A collection of Edge objects that define the boundary path. Cannot be null.</param>
			public BoundaryPath(IEnumerable<Edge> edges) : this()
			{
				foreach (var edge in edges)
				{
					this.Edges.Add(edge);
				}
			}

			/// <summary>
			/// Initializes a new instance of the BoundaryPath class using the specified collection of entities.
			/// </summary>
			/// <remarks>The entities provided are added to the Entities collection of the BoundaryPath. The path is
			/// marked as derived and external by default.</remarks>
			/// <param name="entities">A collection of Entity objects that define the segments of the boundary path. Cannot be null.</param>
			public BoundaryPath(params IEnumerable<Entity> entities) : this()
			{
				this._flags = BoundaryPathFlags.Derived | BoundaryPathFlags.External;
				this.Entities.AddRange(entities);
				this.UpdateEdges();
			}

			/// <inheritdoc/>
			public void ApplyTransform(Transform transform)
			{
				foreach (var e in this.Edges)
				{
					e.ApplyTransform(transform);
				}
			}

			/// <inheritdoc/>
			public BoundaryPath Clone()
			{
				BoundaryPath path = (BoundaryPath)this.MemberwiseClone();

				path.Entities = new List<Entity>();
				path.Entities.AddRange(this.Entities.Select(e => (Entity)e.Clone()));

				path.Edges = new ObservableCollection<Edge>(
					this.Edges.Select(e => e.Clone()));

				return path;
			}

			/// <inheritdoc/>
			public BoundingBox GetBoundingBox()
			{
				BoundingBox box = BoundingBox.Null;

				foreach (Edge edge in this.Edges)
				{
					box = box.Merge(edge.GetBoundingBox());
				}

				foreach (Entity entity in this.Entities)
				{
					box = box.Merge(entity.GetBoundingBox());
				}

				return box;
			}

			/// <summary>
			/// Retrieves the points of the specified boundary as a sequence of the specified vector type.
			/// </summary>
			/// <param name="precision">The number of points to generate for each arc segment. Must be equal to or greater than 2.</param>
			/// <returns>An <see cref="IEnumerable{T}"/> containing the points of the polyline, including interpolated points for arcs.</returns>
			public IEnumerable<XYZ> GetPoints(int precision = 256)
			{
				List<XYZ> pts = new();
				foreach (Edge edge in this.Edges)
				{
					switch (edge)
					{
						case Arc arc:
							pts.AddRange(arc.PolygonalVertexes(precision));
							break;
						case Ellipse ellipse:
							pts.AddRange(ellipse.PolygonalVertexes(precision));
							break;
						case Line line:
							pts.Add((XYZ)line.Start);
							pts.Add((XYZ)line.End);
							break;
						case Polyline poly:
							Polyline2D pline2d = (Polyline2D)poly.ToEntity();
							pts.AddRange(pline2d.GetPoints<XYZ>(precision));
							break;
						case Spline spline:
							pts.AddRange(spline.PolygonalVertexes(precision));
							break;
					}
				}

				return pts;
			}

			/// <summary>
			/// Updates the collection of edges to reflect the current set of entities in the boundary definition.
			/// </summary>
			/// <remarks>This method clears the existing edges and reconstructs them based on the current entities. It
			/// should be called after modifying the entities collection to ensure the edges remain consistent with the boundary
			/// definition.</remarks>
			/// <exception cref="ArgumentException">Thrown if an entity in the collection is not of a supported type. Only Arc, Circle, Ellipse, Line, Polyline2D,
			/// Polyline3D, and Spline entities are allowed as hatch boundary elements.</exception>
			public void UpdateEdges()
			{
				if (!this.Entities.Any())
				{
					return;
				}

				this.Edges.Clear();

				foreach (var entity in this.Entities)
				{
					switch (entity)
					{
						case Entities.Arc arc:
							this.Edges.Add(new Arc(arc));
							break;
						case Entities.Circle circle:
							this.Edges.Add(new Arc(circle));
							break;
						case Entities.Ellipse ellipse:
							this.Edges.Add(new Ellipse(ellipse));
							break;
						case Entities.Line line:
							this.Edges.Add(new Line(line));
							break;
						case IPolyline polyline:
							this.Edges.Add(new Polyline(polyline));
							break;
						case Entities.Spline spline:
							this.Edges.Add(new Spline(spline));
							break;
						default:
							throw new ArgumentException(($"The entity type {entity.ObjectName} cannot be part of a hatch boundary. Only Arc, Circle, Ellipse, Line, Polyline2D, Polyline3D, and Spline entities are allowed."));
					}
				}
			}

			private void onAdd(NotifyCollectionChangedEventArgs e)
			{
				foreach (Edge edge in e.NewItems)
				{
					if (this.Edges.Count > 1 && this.IsPolyline)
					{
						throw new System.InvalidOperationException();
					}
				}
			}

			private void onEdgesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				switch (e.Action)
				{
					case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
						this.onAdd(e);
						break;
					case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
						break;
					case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
						break;
					case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
						break;
					case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
						break;
				}
			}
		}
	}
}