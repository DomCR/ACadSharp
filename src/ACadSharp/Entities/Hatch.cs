using ACadSharp.Attributes;
using CSMath;
using CSMath.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="Hatch"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityHatch"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Hatch"/>
/// </remarks>
[DxfName(DxfFileToken.EntityHatch)]
[DxfSubClass(DxfSubclassMarker.Hatch)]
public partial class Hatch : Entity
{
	/// <summary>
	/// The current elevation of the object.
	/// </summary>
	[DxfCodeValue(30)]
	public double Elevation { get; set; }

	/// <summary>
	/// Gradient color pattern, if exists.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Name, 470)]
	public HatchGradientPattern GradientColor { get; set; } = new HatchGradientPattern();

	/// <summary>
	/// Associativity flag.
	/// </summary>
	[DxfCodeValue(71)]
	public bool IsAssociative { get; set; }

	/// <summary>
	/// Hatch pattern double flag (pattern fill only)
	/// </summary>
	[DxfCodeValue(77)]
	public bool IsDouble { get; set; }

	/// <summary>
	/// Solid fill flag
	/// </summary>
	[DxfCodeValue(70)]
	public bool IsSolid { get; set; }

	/// <summary>
	/// Specifies the three-dimensional normal unit vector for the object.
	/// </summary>
	[DxfCodeValue(210, 220, 230)]
	public XYZ Normal { get; set; } = XYZ.AxisZ;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityHatch;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.HATCH;

	/// <summary>
	/// Boundary paths (loops).
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 91)]
	public List<BoundaryPath> Paths { get; set; } = new List<BoundaryPath>();

	/// <summary>
	/// Pattern of this hatch.
	/// </summary>
	/// <value>
	/// Default value: SOLID pattern.
	/// </value>
	[DxfCodeValue(DxfReferenceType.Name, 2)]
	public HatchPattern Pattern { get; set; } = HatchPattern.Solid;

	/// <summary>
	/// Hatch pattern angle (pattern fill only).
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 52)]
	public double PatternAngle
	{
		get
		{
			return this._patternAngle;
		}

		set
		{
			this._patternAngle = value;
			this.Pattern?.Update(XY.Zero, this._patternAngle, 1);
		}
	}

	/// <summary>
	/// Hatch pattern scale or spacing(pattern fill only).
	/// </summary>
	[DxfCodeValue(41)]
	public double PatternScale
	{
		get
		{
			return this._patternScale;
		}

		set
		{
			this._patternScale = value;
			this.Pattern?.Update(XY.Zero, 0, this._patternScale);
		}
	}

	/// <summary>
	/// Hatch pattern type
	/// </summary>
	[DxfCodeValue(76)]
	public HatchPatternType PatternType { get; set; }

	/// <summary>
	/// Pixel size used to determine the density to perform various intersection and ray casting operations in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching
	/// </summary>
	[DxfCodeValue(47)]
	public double PixelSize { get; set; }

	/// <summary>
	/// Seed points codes (in OCS)
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 98)]
	[DxfCollectionCodeValue(10, 20)]
	public List<XY> SeedPoints { get; set; } = new List<XY>();

	//63	For MPolygon, pattern fill color as the ACI
	/// <summary>
	/// Hatch style.
	/// </summary>
	[DxfCodeValue(75)]
	public HatchStyleType Style { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Hatch;

	private double _patternAngle;

	private double _patternScale;

	//73	For MPolygon, boundary annotation flag:
	//0 = boundary is not an annotated boundary
	//1 = boundary is an annotated boundary
	//78	Number of pattern definition lines
	//varies
	//Pattern line data.Repeats number of times specified by code 78. See Pattern Data
	//11	For MPolygon, offset vector
	//99	For MPolygon, number of degenerate boundary paths(loops), where a degenerate boundary path is a border that is ignored by the hatch
	/// <inheritdoc/>
	public Hatch() : base() { }

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
		if (this.IsAssociative)
		{
			//Not sure how this effects the hatch
		}

		var newNormal = this.transformNormal(transform, this.Normal);
		var worldTransform = this.getWorldMatrix(transform, this.Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

		foreach (BoundaryPath p in this.Paths)
		{
			p.ApplyTransform(transform);
		}

		XY refAxis = XY.Rotate(XY.AxisX, this._patternAngle);
		refAxis = this._patternScale * refAxis;
		XYZ v = transOW * new XYZ(refAxis.X, refAxis.Y, 0.0);
		v = worldTransform * v;
		v = transWO * v;
		XY axis = new XY(v.X, v.Y);
		this._patternAngle = axis.GetAngle();

		double patScale = axis.GetLength();
		this._patternScale = MathHelper.IsZero(patScale) ? MathHelper.Epsilon : patScale;

		this.Pattern?.Update(transform.Translation.Convert<XY>(), this._patternAngle, this._patternScale);
		this.Normal = newNormal;
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		Hatch clone = base.Clone() as Hatch;

		clone.GradientColor = this.GradientColor?.Clone();
		clone.Pattern = this.Pattern?.Clone();

		clone.Paths = new List<BoundaryPath>();
		foreach (BoundaryPath item in this.Paths)
		{
			clone.Paths.Add(item.Clone());
		}

		return clone;
	}

	/// <summary>
	/// Explode the hatch edges into the equivalent entities.
	/// </summary>
	/// <returns>A collection of entities representing the exploded hatch edges.</returns>
	public IEnumerable<Entity> Explode()
	{
		List<Entity> entities = new List<Entity>();

		foreach (BoundaryPath b in this.Paths)
		{
			foreach (BoundaryPath.Edge e in b.Edges)
			{
				entities.Add(e.ToEntity());
			}
		}

		return entities;
	}

	/// <summary>
	/// Explode the hatch pattern into the equivalent entities.
	/// </summary>
	/// <returns>A collection of entities representing the exploded hatch pattern.</returns>
	public IEnumerable<Entity> ExplodePattern()
	{
		List<Entity> entities = new();

		if (this.Pattern == null
			|| this.Pattern.Lines.Count == 0
			|| this.Paths.Count == 0)
		{
			return entities;
		}

		BoundingBox box = this.GetBoundingBox();
		XY[] corners =
		[
			new XY(box.Min.X, box.Min.Y),
			new XY(box.Min.X, box.Max.Y),
			new XY(box.Max.X, box.Min.Y),
			new XY(box.Max.X, box.Max.Y)
		];

		foreach (var patLine in this.Pattern.Lines)
		{
			if (patLine.Direction.IsZero())
			{
				continue;
			}

			XY normal = new XY(-patLine.Direction.Y, patLine.Direction.X);

			double minProj = double.PositiveInfinity;
			double maxProj = double.NegativeInfinity;
			for (int i = 0; i < corners.Length; i++)
			{
				double p = corners[i].X * normal.X + corners[i].Y * normal.Y;
				if (p < minProj) minProj = p;
				if (p > maxProj) maxProj = p;
			}

			double c0 = patLine.BasePoint.X * normal.X + patLine.BasePoint.Y * normal.Y;

			int kStart;
			int kEnd;
			if (System.Math.Abs((double)patLine.LineOffset) <= MathHelper.Epsilon)
			{
				kStart = 0;
				kEnd = 0;
			}
			else
			{
				double k1 = (minProj - c0) / (double)patLine.LineOffset;
				double k2 = (maxProj - c0) / (double)patLine.LineOffset;

				double kMin = System.Math.Min(k1, k2);
				double kMax = System.Math.Max(k1, k2);

				kStart = (int)System.Math.Floor(kMin) - 1;
				kEnd = (int)System.Math.Ceiling(kMax) + 1;
			}

			for (int k = kStart; k <= kEnd; k++)
			{
				XY basePoint = new XY(
					patLine.BasePoint.X + patLine.Offset.X * k,
					patLine.BasePoint.Y + patLine.Offset.Y * k);

				Line2D geomLine = new Line2D(basePoint, patLine.Direction);

				List<XY> intersections = new List<XY>();
				foreach (BoundaryPath boundary in this.Paths)
				{
					intersections.AddRange(boundary.FindIntersections(geomLine).Distinct());
				}

				if (intersections.Count < 2)
				{
					continue;
				}

				intersections.Sort((a, b) =>
				{
					double ta = (a.X - basePoint.X) * patLine.Direction.X + (a.Y - basePoint.Y) * patLine.Direction.Y;
					double tb = (b.X - basePoint.X) * patLine.Direction.X + (b.Y - basePoint.Y) * patLine.Direction.Y;
					return ta.CompareTo(tb);
				});

				for (int i = 0; i + 1 < intersections.Count; i += 2)
				{
					XY a = intersections[i];
					XY b = intersections[i + 1];

					double tA = (a.X - basePoint.X) * patLine.Direction.X + (a.Y - basePoint.Y) * patLine.Direction.Y;
					double tB = (b.X - basePoint.X) * patLine.Direction.X + (b.Y - basePoint.Y) * patLine.Direction.Y;

					if (tB < tA)
					{
						(tA, tB) = (tB, tA);
					}

					if ((tB - tA) <= MathHelper.Epsilon)
					{
						continue;
					}

					Line2D line = new Line2D(basePoint, patLine.Direction);
					entities.AddRange(emitDashedSegment(line, tA, tB, patLine.DashLengths));
				}
			}
		}

		return entities;
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		BoundingBox box = BoundingBox.Null;

		foreach (BoundaryPath bp in this.Paths)
		{
			box = box.Merge(bp.GetBoundingBox());
		}

		return box;
	}

	private IEnumerable<Line> emitDashedSegment(Line2D line, double tStart, double tEnd, List<double> dashLengths)
	{
		if (dashLengths == null || dashLengths.Count == 0)
		{
			return new[] { new Line(line.PointInLine(tStart), line.PointInLine(tEnd)) };
		}

		double[] abs = dashLengths.Select(d => System.Math.Abs(d)).ToArray();
		double cycle = abs.Sum();
		if (cycle <= MathHelper.Epsilon)
		{
			return Enumerable.Empty<Line>();
		}

		double m = tStart % cycle;
		double pos = m < 0 ? m + cycle : m;
		int idx = 0;
		double acc = 0.0;

		while (idx < abs.Length && acc + abs[idx] <= pos + MathHelper.Epsilon)
		{
			acc += abs[idx];
			idx++;
		}

		if (idx >= abs.Length)
		{
			idx = 0;
			acc = 0.0;
			pos = 0.0;
		}

		var entities = new List<Line>();

		double cursor = tStart;
		double remaining = abs[idx] - (pos - acc);
		while (cursor < tEnd - MathHelper.Epsilon)
		{
			int guard = 0;
			while (remaining <= MathHelper.Epsilon && guard < abs.Length + 1)
			{
				idx = (idx + 1) % abs.Length;
				remaining = abs[idx];
				guard++;
			}

			if (remaining <= MathHelper.Epsilon)
			{
				break;
			}

			double step = System.Math.Min(remaining, tEnd - cursor);
			if (dashLengths[idx] > 0.0 && step > MathHelper.Epsilon)
			{
				double t0 = cursor;
				double t1 = cursor + step;

				var entity = new Line(line.PointInLine(t0), line.PointInLine(t1));
				entity.MatchProperties(this);
				entity.LineType = Tables.LineType.Continuous;
				entities.Add(entity);
			}

			cursor += step;
			remaining -= step;

			if (remaining <= MathHelper.Epsilon)
			{
				idx = (idx + 1) % abs.Length;
				remaining = abs[idx];
			}
		}

		return entities;
	}
}