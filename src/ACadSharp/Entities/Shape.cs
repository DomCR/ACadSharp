using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Tables;
using CSMath;
using CSMath.Extensions;
using System;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="Shape"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityShape"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Shape"/>
/// </remarks>
[DxfName(DxfFileToken.EntityShape)]
[DxfSubClass(DxfSubclassMarker.Shape)]
public class Shape : Entity, IOrientable
{
	/// <summary>
	/// Insertion point (in WCS).
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ InsertionPoint { get; set; }

	/// <inheritdoc/>
	[DxfCodeValue(210, 220, 230)]
	public XYZ Normal { get; set; } = XYZ.AxisZ;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityShape;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.SHAPE;

	/// <summary>
	/// Oblique angle.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
	public double ObliqueAngle { get; set; } = 0;

	/// <summary>
	/// Relative X scale factor.
	/// </summary>
	[DxfCodeValue(41)]
	public double RelativeXScale { get; set; } = 1;

	/// <summary>
	/// Rotation angle.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
	public double Rotation { get; set; } = 0;

	/// <summary>
	/// Shape name.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Name, 2)]
	public TextStyle ShapeStyle
	{
		get { return this._style; }
		set
		{
			if (value == null || !value.IsShapeFile)
			{
				throw new ArgumentNullException(nameof(value));
			}

			this._style = this.updateTableEntry(value, s => this._style = s, this.Document?.TextStyles);
		}
	}

	/// <summary>
	/// Size.
	/// </summary>
	[DxfCodeValue(40)]
	public double Size { get; set; } = 1.0;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Shape;

	/// <summary>
	/// Thickness.
	/// </summary>
	[DxfCodeValue(39)]
	public double Thickness { get; set; } = 0.0;

	internal ushort ShapeIndex { get; set; }

	private TextStyle _style;

	/// <summary>
	/// Initializes a shape by the <see cref="TextStyle"/>
	/// </summary>
	/// <param name="textStyle">Text style with the flag <see cref="TextStyle.IsShapeFile"/></param>
	public Shape(TextStyle textStyle)
	{
		this.ShapeStyle = textStyle;
	}

	internal Shape() : base()
	{
	}

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
		XYZ newPosition = transform.ApplyTransform(this.InsertionPoint);
		XYZ newNormal = this.transformNormal(transform, this.Normal);

		Matrix3 transOW = Matrix3.ArbitraryAxis(this.Normal);
		transOW *= Matrix3.RotationZ(this.Rotation);

		Matrix3 transWO = Matrix3.ArbitraryAxis(newNormal);
		transWO = transWO.Transpose();

		var transformation = new Matrix3(transform.Matrix);
		XYZ v = transOW * XYZ.AxisX;
		v = transformation * v;
		v = transWO * v;
		double newRotation = new XY(v.X, v.Y).GetAngle();

		transWO = Matrix3.RotationZ(newRotation).Transpose() * transWO;

		// Effective scales: X = Size * RelativeXScale, Y = Size, Z = Size
		XYZ s = transOW * new XYZ(this.Size * this.RelativeXScale, this.Size, this.Size);
		s = transformation * s;
		s = transWO * s;
		XYZ newScale = new XYZ(
			s.X.IsZero() ? MathHelper.Epsilon : s.X,
			s.Y.IsZero() ? MathHelper.Epsilon : s.Y,
			s.Z.IsZero() ? MathHelper.Epsilon : s.Z);

		this.Size = newScale.Y;
		this.RelativeXScale = newScale.X / newScale.Y;

		this.Normal = newNormal;
		this.InsertionPoint = newPosition;
		this.Rotation = newRotation;
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		Shape clone = (Shape)base.Clone();

		clone.ShapeStyle = (TextStyle)(this.ShapeStyle?.Clone());

		return clone;
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		return new BoundingBox(this.InsertionPoint);
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		this._style = this.updateTableEntry(this._style, s => this._style = s, doc?.TextStyles);
	}

	internal override void UnassignDocument()
	{
		this.Document.TextStyles.RemoveReference(this._style?.Name, this);

		base.UnassignDocument();

		this._style = this._style?.CloneTyped();
	}
}