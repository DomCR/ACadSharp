using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
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
			/// Number of edges in this boundary path.
			/// </summary>
			/// <remarks>
			/// Only if boundary is not a polyline.
			/// </remarks>
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
			/// Default constructor.
			/// </summary>
			public BoundaryPath()
			{
				this.Edges.CollectionChanged += this.onEdgesCollectionChanged;
			}

			/// <inheritdoc/>
			public void ApplyTransform(Transform transform)
			{
				throw new System.NotImplementedException();
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