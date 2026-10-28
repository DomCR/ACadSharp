using ACadSharp.Attributes;
using ACadSharp.Entities.ProxyGraphics;
using ACadSharp.Extensions;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using CSMath.Extensions;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities;

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
			this._bookColor = this.updateCollectionEntry(value, c => this._bookColor = c, this.Document?.Colors);
		}
	}

	/// <inheritdoc/>
	[DxfCodeValue(62, 420)]
	public Color Color { get; set; } = Color.ByLayer;

	/// <summary>
	/// Gets a value indicating whether any proxy geometries are present.
	/// </summary>
	public bool HasProxyGeometries
	{
		get
		{
			return this._proxyGeometries != null && this._proxyGeometries.Count > 0;
		}
	}

	/// <inheritdoc/>
	[DxfCodeValue(60)]
	public bool IsInvisible { get; set; } = false;

	/// <inheritdoc/>
	[DxfCodeValue(DxfReferenceType.Name, 8)]
	public virtual Layer Layer
	{
		get
		{
			if (this._layer == null)
			{
				this._layer = Layer.Default;
			}

			return this._layer;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			this._layer = this.updateTableEntry(value, l => this._layer = l, this.Document?.Layers);
		}
	}

	/// <inheritdoc/>
	[DxfCodeValue(DxfReferenceType.Name, 6)]
	public virtual LineType LineType
	{
		get
		{
			if (this._lineType == null)
			{
				this._lineType = LineType.ByLayer;
			}

			return this._lineType;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			this._lineType = this.updateTableEntry(value, l => this._lineType = l, this.Document?.LineTypes);
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
	public Material Material
	{
		get { return this._material; }
		set
		{
			this._material = this.updateCollectionEntry(value, m => this._material = m, this.Document?.Materials);
		}
	}

	/// <summary>
	/// Gets the list of proxy geometries for this entity.
	/// </summary>
	public List<IProxyGeometry> ProxyGeometries
	{
		get
		{
			if (this._proxyGeometries == null)
			{
				this._proxyGeometries = new List<IProxyGeometry>();
			}

			return this._proxyGeometries;
		}
	}

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Entity;

	/// <inheritdoc/>
	[DxfCodeValue(440)]
	public Transparency Transparency { get; set; } = Transparency.ByLayer;

	private BookColor _bookColor = null;

	private Layer _layer = null;

	private LineType _lineType = null;

	private Material _material = null;

	private List<IProxyGeometry> _proxyGeometries = null;

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

		clone._layer = this._layer?.CloneTyped();
		clone._lineType = this._lineType?.CloneTyped();
		clone._material = this._material?.CloneTyped();

		if (this._proxyGeometries != null)
		{
			clone._proxyGeometries = new List<IProxyGeometry>();
			foreach (var item in this._proxyGeometries)
			{
				clone._proxyGeometries.Add(item.Clone());
			}
		}

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
			if (color.IsByLayer)
			{
				color = this.Layer.Color;
			}
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
					return record.BlockEntity.GetActiveLineWeightType();
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

		if (entity.Handle == 0 || this.Document != entity.Document)
		{
			this.Layer = (Layer)entity.Layer.Clone();
			this.LineType = (LineType)entity.LineType.Clone();
			this.Material = (Material)entity.Material?.Clone();
		}
		else
		{
			this.Layer = entity.Layer;
			this.LineType = entity.LineType;
			this.Material = entity.Material;
		}

		this.Color = entity.Color;
		this.LineWeight = entity.LineWeight;
		this.LineTypeScale = entity.LineTypeScale;
		this.IsInvisible = entity.IsInvisible;
		this.Transparency = entity.Transparency;
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		this._bookColor = this.updateCollectionEntry(this._bookColor, c => this._bookColor = c, doc.Colors);
		this._material = this.updateCollectionEntry(this._material, m => this._material = m, doc.Materials);

		this._layer = this.updateTableEntry(this._layer ?? Layer.Default, l => this._layer = l, doc.Layers);
		this._lineType = this.updateTableEntry(this._lineType ?? LineType.ByLayer, l => this._lineType = l, doc.LineTypes);
	}

	internal override void UnassignDocument()
	{
		this.Document.Colors.RemoveReference(this._bookColor?.Name, this);
		this.Document.Materials.RemoveReference(this._material?.Name, this);

		this.Document.Layers.RemoveReference(this._layer?.Name, this);
		this.Document.LineTypes.RemoveReference(this._lineType?.Name, this);

		base.UnassignDocument();

		this._layer = (Layer)this._layer?.Clone();
		this._lineType = (LineType)this._lineType?.Clone();

		this._bookColor = (BookColor)this._bookColor?.Clone();
		this._material = (Material)this._material?.Clone();
	}

	protected List<XY> applyRotation(IEnumerable<XY> points, double rotation)
	{
		if (points == null)
		{
			throw new ArgumentNullException(nameof(points));
		}

		if (rotation.IsZero())
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

	protected XYZ applyWorldMatrix(XYZ xyz, Transform transform, Matrix3 transOW, Matrix3 transWO)
	{
		XYZ v = transOW * xyz;
		v = transform.ApplyTransform(v);
		v = transWO * v;
		return v;
	}

	protected Matrix3 getWorldMatrix(Transform transform, XYZ normal, XYZ newNormal, out Matrix3 transOW, out Matrix3 transWO)
	{
		transOW = Matrix3.ArbitraryAxis(normal);
		transWO = Matrix3.ArbitraryAxis(newNormal).Transpose();
		return new Matrix3(transform.Matrix);
	}

	protected XYZ transformNormal(Transform transform, XYZ normal)
	{
		return transform.ApplyRotation(normal).Normalize();
	}
}