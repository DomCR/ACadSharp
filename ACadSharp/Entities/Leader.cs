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
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LEADER;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityLeader;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Leader;

		/// <summary>
		/// Dimension Style
		/// </summary>
		[DxfCodeValue(3)]
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
					this._style = this.updateTable(value, this.Document.DimensionStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <summary>
		/// Arrowhead flag
		/// </summary>
		[DxfCodeValue(71)]
		public bool ArrowHeadEnabled { get; set; }

		/// <summary>
		/// Leader Path Type
		/// </summary>
		/// <value>
		/// 0 = straight lines
		/// 1 = spline
		/// </value>
		[DxfCodeValue(72)]
		public LeaderPathType PathType { get; set; }

		/// <summary>
		/// Leader creation flag, AssociatedAnnotation type
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
		/// Hookline direction flag
		/// </summary>
		/// <value>
		/// 0 = Hookline(or end of tangent for a splined leader) is the opposite direction from the horizontal vector <br/>
		/// 1 = Hookline(or end of tangent for a splined leader) is the same direction as horizontal vector(see code 75)
		/// </value>
		[DxfCodeValue(74)]
		public bool HookLineDirection { get; set; }

		/// <summary>
		/// Hookline flag
		/// </summary>
		[DxfCodeValue(75)]
		public bool HasHookline { get; set; }

		/// <summary>
		/// Text annotation height
		/// </summary>
		[DxfCodeValue(40)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Text annotation width
		/// </summary>
		[DxfCodeValue(41)]
		public double TextWidth { get; set; }

		/// <summary>
		/// Vertices in leader
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 76)]
		[DxfCollectionCodeValue(10, 20, 30)]
		public List<XYZ> Vertices { get; set; } = new List<XYZ>();

		//77	Color to use if leader's DIMCLRD = BYBLOCK

		/// <summary>
		/// Hard reference to associated annotation (mtext, tolerance, or insert entity)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public Entity AssociatedAnnotation { get; internal set; }


		/// <summary>
		/// Normal vector
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Horizontal direction for leader
		/// </summary>
		[DxfCodeValue(211, 221, 231)]
		public XYZ HorizontalDirection { get; set; } = XYZ.AxisX;

		/// <summary>
		/// Offset of last leader vertex from block reference insertion point
		/// </summary>
		[DxfCodeValue(212, 222, 232)]
		public XYZ BlockOffset { get; set; } = XYZ.Zero;

		/// <summary>
		/// Offset of last leader vertex from annotation placement point
		/// </summary>
		[DxfCodeValue(213, 223, 233)]
		public XYZ AnnotationOffset { get; set; } = XYZ.Zero;


		private DimensionStyle _style = DimensionStyle.Default;


		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = this.updateTable(this.Style, doc.DimensionStyles);

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

		public override CadObject Clone()
		{
			Leader clone = (Leader)base.Clone();
			clone.Style = (DimensionStyle)(this.Style?.Clone());
			return clone;
		}
	}
}
