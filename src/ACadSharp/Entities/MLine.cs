using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="MLine"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMLine"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLine"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMLine)]
	[DxfSubClass(DxfSubclassMarker.MLine)]
	public partial class MLine : Entity
	{
		/// <summary>
		/// Flags.
		/// </summary>
		[DxfCodeValue(71)]
		public MLineFlags Flags { get; set; }

		/// <summary>
		/// Justification.
		/// </summary>
		[DxfCodeValue(70)]
		public MLineJustification Justification { get; set; }

		/// <summary>
		/// Extrusion direction.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMLine;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.MLINE;

		/// <summary>
		/// Scale factor.
		/// </summary>
		[DxfCodeValue(40)]
		public double ScaleFactor { get; set; } = 1;

		/// <summary>
		/// Start point(in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; }

		/// <summary>
		/// MLine Style.
		/// </summary>
		/// <remarks>
		/// Name reference: <br/>
		/// String of up to 32 characters.The name of the style used for this mline. An entry for this style must exist in the MLINESTYLE dictionary.
		/// Do not modify this field without also updating the associated entry in the MLINESTYLE dictionary
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle | DxfReferenceType.Name, 340)]
		public MLineStyle Style
		{
			get { return this._style; }
			set
			{
				if (value == null)
				{
					throw new System.ArgumentNullException(nameof(value), "Multi line style cannot be null");
				}

				if (this.Document != null)
				{
					this._style = updateCollection(value, this.Document.MLineStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MLine;

		/// <summary>
		/// Vertices in the MLine.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 72)]
		public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		private MLineStyle _style = MLineStyle.Default;

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.Normal = this.transformNormal(transform, this.Normal);
			this.StartPoint = transform.ApplyTransform(StartPoint);

			foreach (var item in this.Vertices)
			{
				item.ApplyTransform(transform);
			}
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			MLine clone = (MLine)base.Clone();

			clone.Style = (MLineStyle)(this.Style?.Clone());

			clone.Vertices.Clear();
			foreach (var item in this.Vertices)
			{
				clone.Vertices.Add(item.Clone());
			}

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.FromPoints(Vertices.Select(v => v.Position));
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = updateCollection(this.Style, doc.MLineStyles);

			this.Document.MLineStyles.OnRemove += this.mLineStylesOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.MLineStyles.OnRemove -= this.mLineStylesOnRemove;

			base.UnassignDocument();

			this._style = (MLineStyle)this.Style.Clone();
		}

		private void mLineStylesOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.Style))
			{
				this.Style = this.Document.MLineStyles[MLineStyle.DefaultName];
			}
		}
	}
}