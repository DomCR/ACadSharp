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

		// [PATCH] Rebuild the shape parameters by mapping its two in-plane local axes
		// through the transform's linear part and decomposing the result:
		//   - new rotation = angle of the local X-axis image vs the new frame's base X axis
		//   - new Size (base scale) = length of the local Y-axis image
		//   - new RelativeXScale = (|imgX| / |imgY|) * old RelativeXScale
		// For a conformal transform (rotation + uniform scale) this is exact: Size scales
		// by s and RelativeXScale is preserved (verified against the VBA reference:
		// Size 0.499 = 0.5*0.998, RelativeXScale 1.0; the upstream frame-conjugation code
		// double-applied the new rotation to the scale vector and produced Size -0.6137 /
		// RelativeXScale -0.5675).
		Matrix3 linear = new Matrix3(transform.Matrix);
		XYZ n = this.Normal;

		// Original local frame (in-plane axes, orthonormal, Z = normal).
		Matrix3 oldFrame = Matrix3.ArbitraryAxis(n);
		XYZ oldX = new XYZ(oldFrame.M00, oldFrame.M10, oldFrame.M20);
		XYZ oldY = new XYZ(oldFrame.M01, oldFrame.M11, oldFrame.M21);

		// Rotate the frame in-plane by the shape rotation (Rodrigues about n).
		XYZ axX = RotateAboutAxis(oldX, n, this.Rotation);
		XYZ axY = RotateAboutAxis(oldY, n, this.Rotation);

		// Map the local axes into world space.
		XYZ imgX = linear * axX;
		XYZ imgY = linear * axY;

		// New rotation: angle of the X-axis image relative to the new frame's base X axis.
		Matrix3 newFrame = Matrix3.ArbitraryAxis(newNormal);
		XYZ newX = new XYZ(newFrame.M00, newFrame.M10, newFrame.M20);
		XYZ imgXProjected = imgX - newNormal * imgX.Dot(newNormal);
		double newRotation;
		if (imgXProjected.GetLength() > MathHelper.Epsilon)
		{
			imgXProjected = imgXProjected.Normalize();
			double cosA = newX.Dot(imgXProjected);
			double sinA = newNormal.Dot(XYZ.Cross(newX, imgXProjected));
			newRotation = Math.Atan2(sinA, cosA);
		}
		else
		{
			newRotation = this.Rotation;
		}

		// New scales from the axis image lengths: the axis image lengths ARE the
		// scale factors, so the old sizes must be multiplied by them.
		double scaleX = imgX.GetLength();
		double scaleY = imgY.GetLength();
		if (scaleY > MathHelper.Epsilon)
		{
			this.Size *= scaleY;
			this.RelativeXScale *= scaleX / scaleY;
		}
		else if (scaleX > MathHelper.Epsilon)
		{
			// Degenerate Y axis (e.g. planar projection): keep a positive base scale.
			this.Size *= scaleX;
		}

		this.Normal = newNormal;
		this.InsertionPoint = newPosition;
		// [PATCH] AutoCAD stores the shape rotation normalized in [0, 2π).
		this.Rotation = MathHelper.NormalizeAngleRadians(newRotation, true);
	}

	/// <summary>
	/// Rotates a vector about an arbitrary unit axis (Rodrigues' formula).
	/// </summary>
	private static XYZ RotateAboutAxis(XYZ v, XYZ axis, double angle)
	{
		double cos = Math.Cos(angle);
		double sin = Math.Sin(angle);
		XYZ k = axis.Normalize();
		XYZ cross = XYZ.Cross(k, v);
		XYZ parallel = k * k.Dot(v);
		return v * cos + cross * sin + parallel * (1.0 - cos);
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