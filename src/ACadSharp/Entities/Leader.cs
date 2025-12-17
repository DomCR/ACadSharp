using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Leader"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityLeader"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Leader"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityLeader)]
	[DxfSubClass(DxfSubclassMarker.Leader)]
	public class Leader : Entity
	{
		/// <summary>
		/// Offset of last leader vertex from annotation placement point.
		/// </summary>
		[DxfCodeValue(213, 223, 233)]
		public XYZ AnnotationOffset { get; set; } = XYZ.Zero;

		/// <summary>
		/// Arrowhead flag.
		/// </summary>
		[DxfCodeValue(71)]
		public bool ArrowHeadEnabled { get; set; }

		/// <summary>
		/// Hard reference to associated annotation (mtext, tolerance, or insert entity).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public Entity AssociatedAnnotation { get; internal set; }

		/// <summary>
		/// Offset of last leader vertex from block reference insertion point.
		/// </summary>
		[DxfCodeValue(212, 222, 232)]
		public XYZ BlockOffset { get; set; } = XYZ.Zero;

		/// <summary>
		/// Leader creation flag, AssociatedAnnotation type.
		/// </summary>
		/// <value>
		/// 0 = mtext
		/// 1 = tolerance
		/// 2 = insert
		/// 3 = none (default)
		/// </value>
		[DxfCodeValue(73)]
		public LeaderCreationType CreationType { get; set; } = LeaderCreationType.CreatedWithoutAnnotation;

		/// <summary>
		/// Hook line flag.
		/// </summary>
		[DxfCodeValue(75)]
		public bool HasHookline
		{
			get
			{
				bool result = false;
				if (this.Vertices.Count <= 1)
				{
					return result;
				}

				double angle = (this.Vertices[this.Vertices.Count - 2] - this.Vertices[this.Vertices.Count - 1]).AngleBetweenVectors(this.HorizontalDirection);
				return MathHelper.IsZero(angle);
			}
		}

		/// <summary>
		/// Hook line direction.
		/// </summary>
		[DxfCodeValue(74)]
		public HookLineDirection HookLineDirection { get; set; }

		/// <summary>
		/// Horizontal direction for leader.
		/// </summary>
		[DxfCodeValue(211, 221, 231)]
		public XYZ HorizontalDirection { get; set; } = XYZ.AxisX;

		//77	Color to use if leader's DIMCLRD = BYBLOCK
		/// <summary>
		/// Normal vector.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityLeader;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LEADER;

		/// <summary>
		/// Leader Path Type.
		/// </summary>
		/// <value>
		/// 0 = straight lines
		/// 1 = spline
		/// </value>
		[DxfCodeValue(72)]
		public LeaderPathType PathType { get; set; }

		/// <summary>
		/// Dimension Style.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 3)]
		public DimensionStyle Style
		{
			get { return this._style; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._style = CadObject.updateCollection(value, this.Document.DimensionStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Leader;

		/// <summary>
		/// Text annotation height.
		/// </summary>
		[DxfCodeValue(40)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Text annotation width.
		/// </summary>
		[DxfCodeValue(41)]
		public double TextWidth { get; set; }

		/// <summary>
		/// Vertices in leader.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 76)]
		[DxfCollectionCodeValue(10, 20, 30)]
		public List<XYZ> Vertices { get; set; } = new List<XYZ>();

		private DimensionStyle _style = DimensionStyle.Default;

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.Normal = this.transformNormal(transform, this.Normal);

			for (int i = 0; i < this.Vertices.Count; i++)
			{
				this.Vertices[i] = transform.ApplyTransform(this.Vertices[i]);
			}

			this.HorizontalDirection = transform.ApplyRotation(this.HorizontalDirection).Normalize();
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Leader clone = (Leader)base.Clone();
			clone.Style = (DimensionStyle)(this.Style?.Clone());
			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.FromPoints(this.Vertices);
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = CadObject.updateCollection(this.Style, doc.DimensionStyles);

			doc.DimensionStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.DimensionStyles.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Style = (DimensionStyle)this.Style.Clone();
		}

		protected override void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			base.tableOnRemove(sender, e);

			if (e.Item.Equals(this.Style))
			{
				this.Style = this.Document.DimensionStyles[DimensionStyle.DefaultName];
			}
		}
	}
}