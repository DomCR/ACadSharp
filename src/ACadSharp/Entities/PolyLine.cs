using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Polyline{T}"/> entity.
	/// </summary>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(null, true)]
	public abstract class Polyline<T> : Entity, IPolyline
		where T : Entity, IVertex
	{
		/// <inheritdoc/>
		[DxfCodeValue(30)]
		public double Elevation { get; set; } = 0.0;

		/// <summary>
		/// End width.
		/// </summary>
		[DxfCodeValue(41)]
		public double EndWidth { get; set; } = 0.0;

		/// <summary>
		/// Polyline flags.
		/// </summary>
		[DxfCodeValue(70)]
		public PolylineFlags Flags { get => this._flags; set => this._flags = value; }

		/// <inheritdoc/>
		public bool IsClosed
		{
			get
			{
				return this.Flags.HasFlag(PolylineFlags.ClosedPolylineOrClosedPolygonMeshInM) || this.Flags.HasFlag(PolylineFlags.ClosedPolygonMeshInN);
			}
			set
			{
				if (value)
				{
					this._flags.AddFlag(PolylineFlags.ClosedPolylineOrClosedPolygonMeshInM);
					this._flags.AddFlag(PolylineFlags.ClosedPolygonMeshInN);
				}
				else
				{
					this._flags.RemoveFlag(PolylineFlags.ClosedPolylineOrClosedPolygonMeshInM);
					this._flags.RemoveFlag(PolylineFlags.ClosedPolygonMeshInN);
				}
			}
		}

		/// <inheritdoc/>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPolyline;

		/// <summary>
		/// Curves and smooth surface type.
		/// </summary>
		[DxfCodeValue(75)]
		public SmoothSurfaceType SmoothSurface { get; set; }

		/// <summary>
		/// Start width.
		/// </summary>
		[DxfCodeValue(40)]
		public double StartWidth { get; set; } = 0.0;

		/// <inheritdoc/>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Vertices that form this polyline.
		/// </summary>
		/// <remarks>
		/// Each <see cref="Vertex"/> has it's own unique handle.
		/// </remarks>
		public SeqendCollection<T> Vertices { get; private set; }

		/// <inheritdoc/>
		IEnumerable<IVertex> IPolyline.Vertices { get { return this.Vertices; } }

		private PolylineFlags _flags;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Polyline() : base()
		{
			this.Vertices = new SeqendCollection<T>(this);
		}

		public Polyline(IEnumerable<T> vertices, bool isClosed) : this()
		{
			if (vertices == null)
				throw new System.ArgumentException("The vertices enumerable cannot be null or empty", nameof(vertices));

			this.Vertices.AddRange(vertices);
			this.IsClosed = isClosed;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			var newNormal = this.transformNormal(transform, this.Normal);

			this.getWorldMatrix(transform, this.Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

			foreach (var vertex in this.Vertices)
			{
				XYZ v = transOW * vertex.Location.Convert<XYZ>();
				v = transform.ApplyTransform(v);
				v = transWO * v;
				vertex.Location = v;
			}

			this.Normal = newNormal;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Polyline<T> clone = (Polyline<T>)base.Clone();

			clone.Vertices = new SeqendCollection<T>(clone);
			foreach (T v in this.Vertices)
			{
				clone.Vertices.Add((T)v.Clone());
			}

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			//TODO: can a polyline have only 1 vertex?
			if (this.Vertices.Count < 2)
			{
				return BoundingBox.Null;
			}

			XYZ first = this.Vertices[0].Location.Convert<XYZ>();
			XYZ second = this.Vertices[1].Location.Convert<XYZ>();

			XYZ min = new XYZ(System.Math.Min(first.X, second.X), System.Math.Min(first.Y, second.Y), System.Math.Min(first.Z, second.Z));
			XYZ max = new XYZ(System.Math.Max(first.X, second.X), System.Math.Max(first.Y, second.Y), System.Math.Max(first.Z, second.Z));

			foreach (T v in this.Vertices.Skip(2))
			{
				XYZ curr = v.Location.Convert<XYZ>();

				min = new XYZ(System.Math.Min(min.X, curr.X), System.Math.Min(min.Y, curr.Y), System.Math.Min(min.Z, curr.Z));
				max = new XYZ(System.Math.Max(max.X, curr.X), System.Math.Max(max.Y, curr.Y), System.Math.Max(max.Z, curr.Z));
			}

			return new BoundingBox(min, max);
		}

		internal static IEnumerable<Entity> Explode(IPolyline polyline)
		{
			//Generic explode method for Polyline2D and LwPolyline
			List<Entity> entities = new List<Entity>();

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

				Entity e = null;
				if (curr.Bulge == 0)
				{
					//Is a line
					e = new Line
					{
						StartPoint = curr.Location.Convert<XYZ>(),
						EndPoint = next.Location.Convert<XYZ>(),
						Normal = polyline.Normal,
						Thickness = polyline.Thickness,
					};
				}
				else
				{
					XY p1 = curr.Location.Convert<XY>();
					XY p2 = next.Location.Convert<XY>();

					//Is an arc
					Arc arc = Arc.CreateFromBulge(p1, p2, curr.Bulge);
					arc.Center = new XYZ(arc.Center.X, arc.Center.Y, polyline.Elevation);
					arc.Normal = polyline.Normal;
					arc.Thickness = polyline.Thickness;

					e = arc;
				}

				polyline.MatchProperties(e);

				entities.Add(e);
			}

			return entities;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);
			doc.RegisterCollection(this.Vertices);
		}

		internal override void UnassignDocument()
		{
			this.Document.UnregisterCollection(this.Vertices);
			base.UnassignDocument();
		}
	}
}