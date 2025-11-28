using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Spline : Edge
			{
				/// <remarks>
				/// Position values are only X and Y, Z represents the weight.
				/// </remarks>
				[DxfCodeValue(96)]
				//42	Weights(optional, default = 1)	??
				public List<XYZ> ControlPoints { get; private set; } = new List<XYZ>();

				/// <summary>
				/// Degree.
				/// </summary>
				[DxfCodeValue(94)]
				public int Degree { get; set; }

				/// <summary>
				/// End tangent.
				/// </summary>
				[DxfCodeValue(13, 23)]
				public XY EndTangent { get; set; }

				/// <remarks>
				/// Number of fit data.
				/// </remarks>
				[DxfCodeValue(97)]
				public List<XY> FitPoints { get; private set; } = new List<XY>();

				/// <summary>
				/// Number of knots.
				/// </summary>
				[DxfCodeValue(95)]
				public List<double> Knots { get; private set; } = new List<double>();

				/// <summary>
				/// Periodic.
				/// </summary>
				[DxfCodeValue(74)]
				public bool Periodic { get; set; }

				/// <summary>
				/// Rational.
				/// </summary>
				[DxfCodeValue(73)]
				public bool Rational { get; set; }

				/// <summary>
				/// Start tangent.
				/// </summary>
				[DxfCodeValue(12, 22)]
				public XY StartTangent { get; set; }

				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.Spline;

				/// <summary>
				/// Gets a collection of weights derived from the Z-coordinates of the control points.
				/// </summary>
				public IEnumerable<double> Weights { get { return this.ControlPoints.Select(c => c.Z); } }

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					var arr = this.ControlPoints.ToArray();
					this.ControlPoints.Clear();
					for (int i = 0; i < arr.Length; i++)
					{
						var weight = arr[i].Z;
						var v = transform.ApplyTransform(arr[i]);
						v.Z = weight;

						this.ControlPoints.Add(v);
					}

					for (int i = 0; i < this.FitPoints.Count; i++)
					{
						this.FitPoints[i] = transform.ApplyTransform(this.FitPoints[i].Convert<XYZ>()).Convert<XY>();
					}
				}

				/// <inheritdoc/>
				public override Edge Clone()
				{
					Spline clone = (Spline)base.Clone();

					clone.ControlPoints = new List<XYZ>(this.ControlPoints);
					clone.FitPoints = new List<XY>(this.FitPoints);
					clone.Knots = new List<double>(this.Knots);

					return clone;
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return BoundingBox.FromPoints(this.ControlPoints);
				}

				/// <summary>
				/// Converts the spline in a list of vertexes.
				/// </summary>
				/// <param name="precision">Number of vertexes generated.</param>
				/// <returns>A list vertexes that represents the spline expressed in object coordinate system.</returns>
				public List<XYZ> PolygonalVertexes(int precision)
				{
					Entities.Spline spline = (Entities.Spline)this.ToEntity();
					return spline.PolygonalVertexes(precision);
				}

				/// <inheritdoc/>
				public override Entity ToEntity()
				{
					Entities.Spline spline = new();

					spline.Degree = this.Degree;
					spline.Flags = this.Periodic ? spline.Flags |= (SplineFlags.Periodic) : spline.Flags;
					spline.Flags = this.Rational ? spline.Flags |= (SplineFlags.Rational) : spline.Flags;

					spline.StartTangent = this.StartTangent.Convert<XYZ>();
					spline.EndTangent = this.EndTangent.Convert<XYZ>();

					spline.ControlPoints.AddRange(this.ControlPoints);

					if (this.Weights.Any(w => w == 0))
					{
						spline.Weights.AddRange(Enumerable.Repeat(1.0d, this.ControlPoints.Count));
					}
					else
					{
						spline.Weights.AddRange(this.Weights);
					}

					spline.FitPoints.AddRange(this.FitPoints.Select(x => x.Convert<XYZ>()));
					spline.Knots.AddRange(this.Knots);

					return spline;
				}
			}
		}
	}
}