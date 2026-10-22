using ACadSharp.Attributes;
using ACadSharp.Extensions;
using CSMath;
using CSMath.Extensions;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="LwPolyline"/> entity
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityLwPolyline"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.LwPolyline"/>
/// </remarks>
[DxfName(DxfFileToken.EntityLwPolyline)]
[DxfSubClass(DxfSubclassMarker.LwPolyline)]
public partial class LwPolyline : Entity, IPolyline
{
	/// <summary>
	/// Constant width
	/// </summary>
	/// <remarks>
	/// Not used if variable width (codes 40 and/or 41) is set
	/// </remarks>
	[DxfCodeValue(43)]
	public double ConstantWidth { get; set; } = 0.0;

	/// <summary>
	/// True when the source data explicitly carried a constant width (DXF 43), even if
	/// the value is 0, so the writer can round-trip the code exactly as AutoCAD does.
	/// </summary>
	public bool ConstantWidthSpecified { get; set; }

	/// <summary>
	/// The current elevation of the object.
	/// </summary>
	[DxfCodeValue(38)]
	public double Elevation { get; set; } = 0.0;

	/// <summary>
	/// Polyline flags.
	/// </summary>
	[DxfCodeValue(70)]
	public LwPolylineFlags Flags { get => this._flags; set => this._flags = value; }

	/// <inheritdoc/>
	public bool IsClosed
	{
		get
		{
			return this.Flags.HasFlag(LwPolylineFlags.Closed);
		}
		set
		{
			if (value)
			{
				_flags.AddFlag(LwPolylineFlags.Closed);
			}
			else
			{
				_flags.RemoveFlag(LwPolylineFlags.Closed);
			}
		}
	}

	/// <inheritdoc/>
	[DxfCodeValue(210, 220, 230)]
	public XYZ Normal { get; set; } = XYZ.AxisZ;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityLwPolyline;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.LWPOLYLINE;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.LwPolyline;

	/// <summary>
	/// Specifies the distance a 2D object is extruded above or below its elevation.
	/// </summary>
	[DxfCodeValue(39)]
	public double Thickness { get; set; } = 0.0;

	/// <summary>
	/// Vertices that form this LwPolyline
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 90)]
	public List<Vertex> Vertices { get; private set; } = new List<Vertex>();

	/// <inheritdoc/>
	IEnumerable<IVertex> IPolyline.Vertices { get { return this.Vertices; } }

	private LwPolylineFlags _flags;

	/// <inheritdoc/>
	public LwPolyline() : base() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="LwPolyline"/> class with the specified vertices.
	/// </summary>
	/// <remarks>The provided <paramref name="vertices"/> are added to the polyline in the order they appear in
	/// the collection.</remarks>
	/// <param name="vertices">A collection of <see cref="Vertex"/> objects that define the vertices of the polyline.</param>
	public LwPolyline(params IEnumerable<Vertex> vertices)
	{
		this.Vertices.AddRange(vertices);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LwPolyline"/> class with the specified vertices.
	/// </summary>
	/// <remarks>This constructor allows you to create a lightweight polyline by specifying its vertices as a
	/// collection of <see cref="IVector"/> points. The vertices are internally converted to <see cref="Vertex"/>
	/// objects.</remarks>
	/// <param name="vertices">A collection of <see cref="IVector"/> objects that define the vertices of the polyline.</param>
	public LwPolyline(params IEnumerable<IVector> vertices)
		: this(vertices.Select(v => new Vertex(v.Convert<XY>()))) { }

	/// <inheritdoc/>
	/// <remarks>
	/// AutoCAD TransformBy semantics (verified against sample_VBAtransformed.dxf):
	/// vertex locations move with the full transform, both widths (DXF 40 start, 41 end)
	/// are multiplied by the uniform scale (5 → 4.99, 0.8485775377005793 → 0.8468803826251781
	/// for a 0.998 scale) and the bulge (DXF 42) is left untouched (-0.52056705 stays —
	/// a bulge is tan(θ/4), an angle factor, invariant under rotation + uniform scale).
	/// Upstream moved only the vertex locations, leaving widths unscaled.
	/// </remarks>
	public override void ApplyTransform(Transform transform)
	{
		var newNormal = this.transformNormal(transform, this.Normal);

		this.getWorldMatrix(transform, this.Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

		// [PATCH] Uniform scale = average of the decomposed scale components
		// (transform.Scale is a vector; its GetLength() would give s·√3, not s).
		XYZ vsc = transform.Scale;
		double scaleFactor = (Math.Abs(vsc.X) + Math.Abs(vsc.Y) + Math.Abs(vsc.Z)) / 3.0;

		foreach (var vertex in this.Vertices)
		{
			XYZ v = transOW * vertex.Location.Convert<XYZ>();
			v = transform.ApplyTransform(v);
			v = transWO * v;
			vertex.Location = v.Convert<XY>();
			vertex.StartWidth *= scaleFactor;
			vertex.EndWidth *= scaleFactor;
		}

		this.ConstantWidth *= scaleFactor;
		this.Normal = newNormal;
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		LwPolyline clone = (LwPolyline)base.Clone();

		clone.Vertices = new List<Vertex>();
		foreach (var v in this.Vertices)
		{
			clone.Vertices.Add(v.Clone());
		}
		return clone;
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		if (this.Vertices.Any(v => v.Bulge != 0))
		{
			return BoundingBox.FromPoints(this.GetPoints<XYZ>(byte.MaxValue));
		}

		return BoundingBox.FromPoints(this.Vertices.Select(v => v.Location.Convert<XYZ>()));
	}
}