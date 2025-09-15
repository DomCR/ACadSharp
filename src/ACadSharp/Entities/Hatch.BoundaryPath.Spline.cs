using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;
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
				public List<XYZ> ControlPoints { get; set; } = new List<XYZ>();

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
				public List<XY> FitPoints { get; set; } = new List<XY>();

				/// <summary>
				/// Number of knots.
				/// </summary>
				[DxfCodeValue(95)]
				public List<double> Knots { get; set; } = new List<double>();

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

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					throw new System.NotImplementedException();
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return BoundingBox.FromPoints(this.ControlPoints);
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
					spline.Weights.AddRange(this.ControlPoints.Select(x => x.Z));
					spline.FitPoints.AddRange(this.FitPoints.Select(x => x.Convert<XYZ>()));

					return spline;
				}
			}
		}
	}
}