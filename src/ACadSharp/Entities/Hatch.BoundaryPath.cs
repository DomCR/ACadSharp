using ACadSharp.Attributes;
using CSUtilities.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public bool IsPolyline { get { return this.Edges.OfType<Polyline>().Any(); } }

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
						this._flags = this._flags.AddFlag(BoundaryPathFlags.Polyline);
					}
					else
					{
						this._flags = this._flags.RemoveFlag(BoundaryPathFlags.Polyline);
					}

					return this._flags;
				}
				set
				{
					this._flags = value;
				}
			}

			/// <summary>
			/// Number of edges in this boundary path
			/// </summary>
			/// <remarks>
			/// only if boundary is not a polyline
			/// </remarks>
			[DxfCodeValue(DxfReferenceType.Count, 93)]
			public ObservableCollection<Edge> Edges { get; } = new();

			/// <summary>
			/// Source boundary objects
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 97)]
			public List<Entity> Entities { get; set; } = new List<Entity>();

			private BoundaryPathFlags _flags;

			public BoundaryPath()
			{
				Edges.CollectionChanged += this.onEdgesCollectionChanged;
			}

			public BoundaryPath Clone()
			{
				throw new System.NotImplementedException();
			}

			private void onEdgesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				switch (e.Action)
				{
					case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
						onAdd(e);
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
		}
	}
}
