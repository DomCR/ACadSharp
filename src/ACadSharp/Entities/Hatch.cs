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

				List<double> tHits = new();
				foreach (BoundaryPath boundary in this.Paths)
				{
					foreach (XY p in boundary.FindIntersections(geomLine))
					{
						double t = (p.X - basePoint.X) * patLine.Direction.X + (p.Y - basePoint.Y) * patLine.Direction.Y;
						tHits.Add(t);
					}
				}

				var ts = mergeParameters(tHits).ToArray();
				if (ts.Length < 2)
				{
					continue;
				}

				for (int i = 0; i + 1 < ts.Length; i++)
				{
					double tA = ts[i];
					double tB = ts[i + 1];

					if ((tB - tA) <= MathHelper.Epsilon)
					{
						continue;
					}

					double tMid = 0.5 * (tA + tB);
					XY mid = geomLine.PointInLine(tMid);

					// Prevents false segments caused by corner-touch intersections
					if (!this.isPointInsideByParity(mid))
					{
						continue;
					}

					entities.AddRange(emitDashedSegment(geomLine, tA, tB, patLine.DashLengths));
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

	private static IEnumerable<double> mergeParameters(IEnumerable<double> values, double tolerance = MathHelper.Epsilon)
	{
		double[] sorted = values.OrderBy(x => x).ToArray();
		if (sorted.Length == 0)
		{
			return sorted;
		}

		List<double> merged = new() { sorted[0] };
		for (int i = 1; i < sorted.Length; i++)
		{
			if (System.Math.Abs(sorted[i] - merged.Last()) <= tolerance)
			{
				merged[merged.Count - 1] = 0.5 * (merged.Last() + sorted[i]);
			}
			else
			{
				merged.Add(sorted[i]);
			}
		}

		return merged;
	}

	private IEnumerable<Entity> emitDashedSegment(Line2D line, double tStart, double tEnd, List<double> dashLengths)
	{
		if (dashLengths == null || dashLengths.Count == 0)
		{
			var solid = new Line(line.PointInLine(tStart), line.PointInLine(tEnd));
			solid.MatchProperties(this);
			solid.LineType = Tables.LineType.Continuous;
			return new Entity[] { solid };
		}

		double cycle = dashLengths.Sum(d => System.Math.Abs(d));
		if (cycle <= MathHelper.Epsilon)
		{
			return Enumerable.Empty<Entity>();
		}

		double pos = tStart % cycle;
		if (pos < 0)
		{
			pos += cycle;
		}

		int idx = 0;
		double acc = 0.0;
		while (idx < dashLengths.Count && acc + System.Math.Abs(dashLengths[idx]) <= pos + MathHelper.Epsilon)
		{
			acc += System.Math.Abs(dashLengths[idx]);
			idx++;
		}

		if (idx >= dashLengths.Count)
		{
			idx = 0;
			acc = 0.0;
			pos = 0.0;
		}

		var result = new List<Entity>();
		double cursor = tStart;
		double remaining = System.Math.Abs(dashLengths[idx]) - (pos - acc);

		while (cursor < tEnd - MathHelper.Epsilon)
		{
			int guard = 0;
			while (remaining <= MathHelper.Epsilon && guard < dashLengths.Count + 1)
			{
				if (dashLengths[idx] == 0.0)
				{
					//Draw a point for 0 dash value
					var dot = new Line(line.PointInLine(cursor), line.PointInLine(cursor + MathHelper.Epsilon));
					dot.MatchProperties(this);
					result.Add(dot);
				}

				idx = (idx + 1) % dashLengths.Count;
				remaining = System.Math.Abs(dashLengths[idx]);
				guard++;
			}

			if (remaining <= MathHelper.Epsilon)
			{
				break;
			}

			double step = System.Math.Min(remaining, tEnd - cursor);

			if (dashLengths[idx] > MathHelper.Epsilon && step > MathHelper.Epsilon)
			{
				//Draw a dash segment
				var dash = new Line(line.PointInLine((double)cursor), line.PointInLine((double)(cursor + step)));
				dash.MatchProperties(this);
				dash.LineType = Tables.LineType.Continuous;
				result.Add(dash);
			}

			cursor += step;
			remaining -= step;

			if (remaining <= MathHelper.Epsilon)
			{
				idx = (idx + 1) % dashLengths.Count;
				remaining = System.Math.Abs(dashLengths[idx]);
			}
		}

		return result;
	}

	private bool isPointInsideByParity(XY point, double jitter = 1e-7)
	{
		Line2D ray = new Line2D(new XY(point.X, point.Y + jitter), XY.AxisX);

		List<double> hits = new();
		foreach (BoundaryPath boundary in this.Paths)
		{
			foreach (XY p in boundary.FindIntersections(ray))
			{
				if (p.X >= point.X - MathHelper.Epsilon)
				{
					hits.Add(p.X);
				}
			}
		}

		List<double> merged = mergeParameters(hits).ToList();
		return merged.Count.IsOdd();
	}
}