using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// The standard class for a basic CAD entity or a graphical object.
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.Entity)]
	public abstract class Entity : CadObject, IEntity
	{
		/// <summary>
		/// Book color for this entity.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 430)]
		public BookColor BookColor
		{
			get { return this._bookColor; }
			set
			{
				if (this.Document != null)
				{
					this._bookColor = updateCollection(value, this.Document.Colors);
				}
				else
				{
					this._bookColor = value;
				}
			}
		}

		/// <inheritdoc/>
		[DxfCodeValue(62, 420)]
		public Color Color { get; set; } = Color.ByLayer;

		/// <inheritdoc/>
		[DxfCodeValue(60)]
		public bool IsInvisible { get; set; } = false;

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Name, 8)]
		public Layer Layer
		{
			get { return this._layer; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				this._layer = updateCollection(value, this.Document?.Layers);
			}
		}

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Name, 6)]
		public LineType LineType
		{
			get { return this._lineType; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				this._lineType = CadObject.updateCollection(value, this.Document?.LineTypes);
			}
		}

		/// <inheritdoc/>
		[DxfCodeValue(48)]
		public double LineTypeScale { get; set; } = 1.0;

		/// <inheritdoc/>
		[DxfCodeValue(370)]
		public LineWeightType LineWeight { get; set; } = LineWeightType.ByLayer;

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Handle, 347)]
		public Material Material { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Entity;

		/// <inheritdoc/>
		[DxfCodeValue(440)]
		public Transparency Transparency { get; set; } = Transparency.ByLayer;

		private BookColor _bookColor = null;

		private Layer _layer = Layer.Default;

		private LineType _lineType = LineType.ByLayer;

		/// <inheritdoc/>
		public Entity() : base() { }

		/// <summary>
		/// Apply a rotation to this entity.
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="rotation">The angle to rotate around the given axis, in radians.</param>
		public void ApplyRotation(XYZ axis, double rotation)
		{
			Transform transform = Transform.CreateRotation(axis, rotation);
			this.ApplyTransform(transform);
		}

		/// <summary>
		/// Apply a scaling transformation to this entity.
		/// </summary>
		/// <param name="scale"></param>
		public void ApplyScaling(XYZ scale)
		{
			Transform transform = Transform.CreateScaling(scale);
			this.ApplyTransform(transform);
		}

		/// <summary>
		/// Apply a scaling transformation to this entity.
		/// </summary>
		/// <param name="scale"></param>
		/// <param name="origin"></param>
		public void ApplyScaling(XYZ scale, XYZ origin)
		{
			Transform transform = Transform.CreateScaling(scale, origin);
			this.ApplyTransform(transform);
		}

		/// <inheritdoc/>
		public abstract void ApplyTransform(Transform transform);

		/// <summary>
		/// Apply a translation to this entity.
		/// </summary>
		/// <param name="translation"></param>
		public void ApplyTranslation(XYZ translation)
		{
			Transform transform = Transform.CreateTranslation(translation);
			this.ApplyTransform(transform);
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Entity clone = (Entity)base.Clone();

			clone.Layer = (Layer)this.Layer.Clone();
			clone.LineType = (LineType)this.LineType.Clone();
			clone.Material = (Material)this.Material?.Clone();

			return clone;
		}

		/// <inheritdoc/>
		public Color GetActiveColor()
		{
			Color color;
			if (this.Color.IsByLayer)
			{
				color = this.Layer.Color;
			}
			else if (this.Color.IsByBlock && this.Owner is BlockRecord record)
			{
				color = record.BlockEntity.Color;
			}
			else
			{
				color = this.Color;
			}

			return color;
		}

		/// <inheritdoc/>
		public LineType GetActiveLineType()
		{
			if (this.LineType.Name.Equals(LineType.ByLayerName, StringComparison.InvariantCultureIgnoreCase))
			{
				return this.Layer.LineType;
			}
			else if (this.LineType.Name.Equals(LineType.ByBlockName, StringComparison.InvariantCultureIgnoreCase)
				&& this.Owner is BlockRecord record)
			{
				return record.BlockEntity.LineType;
			}

			return this.LineType;
		}

		/// <inheritdoc/>
		public LineWeightType GetActiveLineWeightType()
		{
			switch (this.LineWeight)
			{
				case LineWeightType.ByLayer:
					return this.Layer.LineWeight;
				case LineWeightType.ByBlock:
					if (this.Owner is BlockRecord record)
					{
						return record.BlockEntity.LineWeight;
					}
					else
					{
						return this.LineWeight;
					}
				default:
					return this.LineWeight;
			}
		}

		/// <inheritdoc/>
		public abstract BoundingBox GetBoundingBox();

		/// <inheritdoc/>
		public void MatchProperties(IEntity entity)
		{
			if (entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (entity.Handle == 0)
			{
				entity.Layer = (Layer)this.Layer.Clone();
				entity.LineType = (LineType)this.LineType.Clone();
			}
			else
			{
				entity.Layer = this.Layer;
				entity.LineType = this.LineType;
			}

			entity.Color = this.Color;
			entity.LineWeight = this.LineWeight;
			entity.LineTypeScale = this.LineTypeScale;
			entity.IsInvisible = this.IsInvisible;
			entity.Transparency = this.Transparency;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._layer = CadObject.updateCollection(this.Layer, doc.Layers);
			this._lineType = CadObject.updateCollection(this.LineType, doc.LineTypes);

			doc.Layers.OnRemove += this.tableOnRemove;
			doc.LineTypes.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.Layers.OnRemove -= this.tableOnRemove;
			this.Document.LineTypes.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Layer = (Layer)this.Layer.Clone();
			this.LineType = (LineType)this.LineType.Clone();
		}

		protected List<XY> applyRotation(IEnumerable<XY> points, double rotation)
		{
			if (points == null)
			{
				throw new ArgumentNullException(nameof(points));
			}

			if (MathHelper.IsZero(rotation))
			{
				return new List<XY>(points);
			}

			double sin = Math.Sin(rotation);
			double cos = Math.Cos(rotation);

			List<XY> transPoints;

			transPoints = new List<XY>();
			foreach (XY p in points)
			{
				transPoints.Add(new XY(p.X * cos - p.Y * sin, p.X * sin + p.Y * cos));
			}
			return transPoints;
		}

		protected List<XYZ> applyRotation(IEnumerable<XYZ> points, XYZ zAxis)
		{
			if (points == null)
			{
				throw new ArgumentNullException(nameof(points));
			}

			Matrix3 trans = Matrix3.ArbitraryAxis(zAxis);
			List<XYZ> transPoints;
			transPoints = new List<XYZ>();
			foreach (XYZ p in points)
			{
				transPoints.Add(trans * p);
			}
			return transPoints;
		}

		protected XYZ applyRotation(XYZ points, XYZ zAxis)
		{
			Matrix4 trans = Matrix4.GetArbitraryAxis(zAxis).Transpose();
			return trans * points;
		}

		protected XYZ applyWorldMatrix(XYZ xyz, Transform transform, Matrix3 transOW, Matrix3 transWO)
		{
			XYZ v = transOW * xyz;
			v = transform.ApplyTransform(v);
			v = transWO * v;
			return v;
		}

		protected XYZ applyWorldMatrix(XYZ xyz, XYZ normal, XYZ newNormal)
		{
			var transOW = Matrix3.ArbitraryAxis(normal).Transpose();
			var transWO = Matrix3.ArbitraryAxis(newNormal);
			XYZ v = transOW * xyz;
			v = transWO * v;
			return v;
		}

		protected Matrix3 getWorldMatrix(Transform transform, XYZ normal, XYZ newNormal, out Matrix3 transOW, out Matrix3 transWO)
		{
			transOW = Matrix3.ArbitraryAxis(normal);
			transWO = Matrix3.ArbitraryAxis(newNormal).Transpose();
			return new Matrix3(transform.Matrix);
		}

		protected virtual void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.Layer))
			{
				this.Layer = this.Document.Layers[Layer.DefaultName];
			}

			if (e.Item.Equals(this.LineType))
			{
				this.LineType = this.Document.LineTypes[LineType.ByLayerName];
			}
		}

		protected XYZ transformNormal(Transform transform, XYZ normal)
		{
			return transform.ApplyRotation(normal).Normalize();
		}
	}
}