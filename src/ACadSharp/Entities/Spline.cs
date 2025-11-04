using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Spline"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntitySpline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Spline"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntitySpline)]
	[DxfSubClass(DxfSubclassMarker.Spline)]
	public class Spline : Entity
	{
		/// <summary>
		/// Number of control points (in WCS).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 73)]
		[DxfCollectionCodeValue(10, 20, 30)]
		public List<XYZ> ControlPoints { get; private set; } = new List<XYZ>();

		/// <summary>
		/// Control-point tolerance.
		/// </summary>
		[DxfCodeValue(43)]
		public double ControlPointTolerance { get; set; } = 0.0000001;

		/// <summary>
		/// Gets or sets the polynomial degree of the resulting spline.
		/// </summary>
		/// <remarks>
		/// Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.
		/// </remarks>
		[DxfCodeValue(71)]
		public int Degree { get; set; } = 3;

		/// <summary>
		/// End tangent—may be omitted in WCS.
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ EndTangent { get; set; }

		/// <summary>
		/// Number of fit points (in WCS).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 74)]
		[DxfCollectionCodeValue(11, 21, 31)]
		public List<XYZ> FitPoints { get; private set; } = new List<XYZ>();

		/// <summary>
		/// Fit tolerance.
		/// </summary>
		[DxfCodeValue(44)]
		public double FitTolerance { get; set; } = 0.0000000001;

		/// <summary>
		/// Spline flags.
		/// </summary>
		[DxfCodeValue(70)]
		public SplineFlags Flags { get => this._flags; set => this._flags = value; }

		/// <summary>
		/// Spline flags1.
		/// </summary>
		/// <remarks>
		/// Only valid for dwg.
		/// </remarks>
		public SplineFlags1 Flags1 { get => this._flags1; set => this._flags1 = value; }

		/// <summary>
		/// Gets a value indicating whether the knot vector has a valid count based on the control points, degree, and closure
		/// status of the curve.
		/// </summary>
		/// <remarks>The expected knot count is calculated as the number of control points plus the degree of the
		/// curve, adjusted for whether the curve is closed.</remarks>
		public bool HasValidKnotCount
		{
			get
			{
				var expected = this.ControlPoints.Count + (this.IsClosed ? 2 : 1) * this.Degree + 1;
				return this.Knots.Count == expected;
			}
		}

		/// <summary>
		/// Flag whether the spline is closed.
		/// </summary>
		public bool IsClosed
		{
			get
			{
				return this.Flags.HasFlag(SplineFlags.Closed);
			}
			set
			{
				if (value)
				{
					this._flags.AddFlag(SplineFlags.Closed);
					this._flags1.AddFlag(SplineFlags1.Closed);
				}
				else
				{
					this._flags.RemoveFlag(SplineFlags.Closed);
					this._flags1.RemoveFlag(SplineFlags1.Closed);
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the spline is periodic.
		/// </summary>
		/// <remarks>A periodic spline forms a closed loop, where the start and end points are connected seamlessly.
		/// Setting this property updates the internal flags to reflect the periodicity of the spline.</remarks>
		public bool IsPeriodic
		{
			get
			{
				return this.Flags.HasFlag(SplineFlags.Periodic);
			}
			set
			{
				if (value)
				{
					this._flags.AddFlag(SplineFlags.Periodic);
				}
				else
				{
					this._flags.RemoveFlag(SplineFlags.Periodic);
				}
			}
		}

		/// <summary>
		/// Knot parameters.
		/// </summary>
		public KnotParametrization KnotParametrization { get; set; }

		/// <summary>
		/// Number of knots.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 72)]
		[DxfCollectionCodeValue(40)]
		public List<double> Knots { get; private set; } = new List<double>();

		/// <summary>
		/// Knot tolerance.
		/// </summary>
		[DxfCodeValue(42)]
		public double KnotTolerance { get; set; } = 0.0000001;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		/// <remarks>
		/// Omitted if the spline is non-planar.
		/// </remarks>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntitySpline;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SPLINE;

		/// <summary>
		/// Start tangent—may be omitted in WCS.
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ StartTangent { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Spline;

		/// <summary>
		/// Weight(if not 1); with multiple group pairs, they are present if all are not 1.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 41)]
		public List<double> Weights { get; private set; } = new List<double>();

		public const short MaxDegree = 10;

		private SplineFlags _flags;

		private SplineFlags1 _flags1;

		/// <inheritdoc/>
		public Spline() : base() { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.Normal = this.transformNormal(transform, this.Normal);

			for (int i = 0; i < this.ControlPoints.Count; i++)
			{
				this.ControlPoints[i] = transform.ApplyTransform(this.ControlPoints[i]);
			}

			for (int i = 0; i < this.FitPoints.Count; i++)
			{
				this.FitPoints[i] = transform.ApplyTransform(this.FitPoints[i]);
			}
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Spline clone = (Spline)base.Clone();

			clone.ControlPoints = new List<XYZ>(this.ControlPoints);
			clone.FitPoints = new List<XYZ>(this.FitPoints);
			clone.Weights = new List<double>(this.Weights);
			clone.Knots = new List<double>(this.Knots);

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			List<XYZ> vertices;
			if (!this.TryPolygonalVertexes(256, out vertices))
			{
				vertices = new List<XYZ>(this.FitPoints);
			}

			return BoundingBox.FromPoints(vertices);
		}

		/// <summary>
		/// Calculates the point on the spline at the specified parametric position.
		/// </summary>
		/// <param name="t">The parametric position along the spline, where <see langword="0"/> represents the start of the spline and <see
		/// langword="1"/> represents the end. Must be a value between <see langword="0"/> and <see langword="1"/>.</param>
		/// <returns>The point on the spline at the specified parametric position, represented as an <see cref="XYZ"/> object.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="t"/> is less than <see langword="0"/> or greater than <see langword="1"/>.</exception>
		public XYZ PointOnSpline(double t)
		{
			if (t < 0 || t > 1)
			{
				throw new ArgumentOutOfRangeException(nameof(t), t, "The parametric position must be a value between 0 and 1.");
			}

			if (t == 1)
			{
				t -= double.Epsilon;
			}

			this.prepare(out XYZ[] controlPts, out double[] weights, out double[] knots);
			this.getStartAndEndKnots(knots, out double uStart, out double uEnd);

			double uDelta = (uEnd - uStart) * t;
			double u = uStart + uDelta;
			return c(controlPts, weights, knots, this.Degree, u);
		}

		/// <summary>
		/// Generates a list of vertex points representing a polygonal approximation of the curve.
		/// </summary>
		/// <remarks>The method calculates the vertex points by dividing the curve into segments based on the specified
		/// precision. If the curve is not closed or periodic, the last control point is explicitly added to the
		/// result.</remarks>
		/// <param name="precision">The number of segments used to approximate the curve. Must be equal to or greater than 2.</param>
		/// <returns>A list of <see cref="XYZ"/> objects representing the vertex points of the polygonal approximation.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="precision"/> is less than 2.</exception>
		public List<XYZ> PolygonalVertexes(int precision)
		{
			if (precision < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(precision), precision, "The precision must be equal or greater than two.");
			}

			List<XYZ> vertexes = new List<XYZ>();

			this.prepare(out XYZ[] controlPts, out double[] weights, out double[] knots);
			this.getStartAndEndKnots(knots, out double uStart, out double uEnd);

			if (!this.IsClosed && !this.IsPeriodic)
			{
				precision -= 1;
			}

			double uDelta = (uEnd - uStart) / precision;

			for (int i = 0; i < precision; i++)
			{
				double u = uStart + uDelta * i;
				vertexes.Add(c(controlPts, weights, knots, this.Degree, u));
			}

			if (!(this.IsClosed || this.IsPeriodic))
			{
				vertexes.Add(controlPts[controlPts.Length - 1]);
			}

			return vertexes;
		}

		/// <summary>
		/// Attempts to calculate a point on the spline at the specified parameter value.
		/// </summary>
		/// <remarks>This method catches any exceptions that occur during the calculation and returns <see
		/// langword="false"/> in such cases. Ensure that the parameter <paramref name="t"/> is within the valid range for the
		/// spline to avoid errors.</remarks>
		/// <param name="t">The parameter value along the spline, typically in the range [0, 1].</param>
		/// <param name="point">When this method returns, contains the calculated point on the spline if the operation succeeds; otherwise, <see
		/// cref="XYZ.NaN"/>. This parameter is passed uninitialized.</param>
		/// <returns><see langword="true"/> if the point was successfully calculated; otherwise, <see langword="false"/>.</returns>
		public bool TryPointOnSpline(double t, out XYZ point)
		{
			point = XYZ.NaN;

			try
			{
				point = this.PointOnSpline(t);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Attempts to calculate the polygonal vertexes of the current object with the specified precision.
		/// </summary>
		/// <remarks>This method catches and suppresses any exceptions that occur during the calculation, returning
		/// <see langword="false"/> in such cases. The <paramref name="points"/> parameter will always be initialized, even if
		/// the operation fails.</remarks>
		/// <param name="precision">The level of precision to use when calculating the polygonal vertexes. Must be a positive integer.</param>
		/// <param name="points">When this method returns, contains a list of <see cref="XYZ"/> objects representing the calculated polygonal
		/// vertexes, if the operation succeeds; otherwise, contains an empty list.</param>
		/// <returns><see langword="true"/> if the polygonal vertexes were successfully calculated; otherwise, <see langword="false"/>.</returns>
		public bool TryPolygonalVertexes(int precision, out List<XYZ> points)
		{
			points = new List<XYZ>();

			try
			{
				points = this.PolygonalVertexes(precision);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Updates the spline's control points, knots, and weights based on the current fit points and tangents.
		/// </summary>
		/// <remarks>This method performs the following operations: <list type="bullet"> <item>Validates the spline's
		/// degree, fit points, and knot parametrization.</item> <item>Generates knots and control points based on the fit
		/// points and tangents.</item> <item>Refines the tangents iteratively if necessary, up to the specified iteration
		/// limit.</item> <item>Assigns uniform weights to the control points.</item> </list> The method returns <see
		/// langword="false"/> if the spline's degree is not 3, if there are fewer than two fit points, if the knot
		/// parametrization is set to <see cref="KnotParametrization.Custom"/>, or if tangent refinement fails.</remarks>
		/// <param name="iterationLimit">The maximum number of iterations allowed for refining the tangents. Defaults to <see cref="byte.MaxValue"/>.</param>
		/// <returns><see langword="true"/> if the spline was successfully updated; otherwise, <see langword="false"/>.</returns>
		public bool UpdateFromFitPoints(uint iterationLimit = byte.MaxValue)
		{
			if (this.Degree != 3
				|| this.FitPoints.Count < 2
				|| this.KnotParametrization == KnotParametrization.Custom)
			{
				return false;
			}

			if (this.FitPoints.Count == 2)
			{
				this.straightLine();
				return true;
			}

			this.Knots.Clear();
			this.ControlPoints.Clear();
			this.Weights.Clear();

			//Compute knots
			var fitPoints = this.FitPoints.ToArray();
			double[] knotValues = generateKnotValues(this.KnotParametrization, fitPoints);
			this.Knots = addSideKnots(this.Degree, knotValues);

			if (this.StartTangent.IsEqual(XYZ.Zero) || this.EndTangent.IsEqual(XYZ.Zero))
			{
				//https://dccg.upc.edu/wp-content/uploads/2025/05/3.3-Spline-Interpolation.pdf
				//Try to manually get the first and last tangent
				take3(knotValues, false, out double fisrtK1, out double firstK2, out double firstK3);
				take3(knotValues, true, out double lastK1, out double lastK2, out double lastK3);

				double startScale = 3.0 / (firstK2 - fisrtK1);
				double endScale = 3.0 / (lastK1 - lastK2);

				take3(fitPoints, false, out XYZ fitPt1, out XYZ fitPt2, out XYZ fitPt3);
				XYZ startTangent = startScale * (fitPt2 - fitPt1 - 0.5 * (fitPt3 - fitPt2));
				take3(fitPoints, true, out fitPt1, out fitPt2, out fitPt3);
				XYZ endTangent = endScale * (fitPt1 - fitPt2 - 0.5 * (fitPt2 - fitPt3));

				double startMag = 1.0 / startTangent.ToEnumerable().Sum(v => v * v);
				double endMag = 1.0 / endTangent.ToEnumerable().Sum(v => v * v);
				if (double.IsInfinity(startMag) || double.IsInfinity(endMag))
				{
					//Found this error in some cases
					return false;
				}

				double startDiv = (firstK2 + firstK3 - 2.0 * fisrtK1) / (firstK2 - fisrtK1);
				double endDiv = (2.0 * lastK1 - lastK2 - lastK3) / (lastK1 - lastK2);

				double startDelta = double.MaxValue;
				double endDelta = double.MaxValue;

				XYZ[] controlPoints = null;
				var knots = this.Knots.ToArray();
				do
				{
					XYZ lastV1 = startTangent;
					XYZ lastV2 = endTangent;
					controlPoints = computeControlPoints(this.Degree, knots, fitPoints, startTangent, endTangent);

					// Update tangents based on new control points, only works for degree 3
					take3(controlPoints, false, out XYZ pt1, out XYZ pt2, out XYZ pt3);
					startTangent = 0.5 * (pt2 - pt1 + (pt3 - pt1) / startDiv) * startScale;

					double currStartDelta = startDelta;
					startDelta = startMag * (startTangent - lastV1).ToEnumerable().Sum(v => v * v);

					take3(controlPoints, true, out pt1, out pt2, out pt3);
					endTangent = 0.5 * (pt1 - pt2 + (pt1 - pt3) / endDiv) * endScale;

					double currEndDelta = endDelta;
					endDelta = endMag * (endTangent - lastV2).ToEnumerable().Sum(v => v * v);

					if ((startDelta >= currStartDelta && endDelta >= currEndDelta)
						|| --iterationLimit <= 0)
					{
						return false;
					}
				}
				while (startDelta + endDelta > MathHelper.Epsilon);

				this.ControlPoints.AddRange(controlPoints);
			}
			else
			{
				this.ControlPoints.AddRange(computeControlPoints(this.Degree, this.Knots.ToArray(), fitPoints, this.StartTangent, this.EndTangent));
			}

			this.Weights = Enumerable.Repeat(1.0d, this.ControlPoints.Count).ToList();

			return true;
		}

		private static List<double> addSideKnots(int degree, double[] knots)
		{
			List<double> list = new List<double>();
			for (int i = 0; i < degree; i++)
			{
				list.Add(knots[0]);
			}

			list.AddRange(knots);

			double item = list[list.Count - 1];
			for (int j = 0; j < degree; j++)
			{
				list.Add(item);
			}

			return list;
		}

		private static XYZ c(XYZ[] ctrlPoints, double[] weights, double[] knots, int degree, double u)
		{
			XYZ vectorSum = XYZ.Zero;
			double denominatorSum = 0.0;

			// optimization suggested by ThVoss
			for (int i = 0; i < ctrlPoints.Length; i++)
			{
				double nurb = computeNurb(knots, i, degree, u);
				denominatorSum += nurb * weights[i];
				vectorSum += weights[i] * nurb * ctrlPoints[i];
			}

			// avoid possible divided by zero error, this should never happen
			if (Math.Abs(denominatorSum) < double.Epsilon)
			{
				return XYZ.Zero;
			}

			return 1.0 / denominatorSum * vectorSum;
		}

		private static XYZ[] computeControlPoints(
			int degree,
			double[] knots,
			IList<XYZ> fitPoints,
			XYZ startTangent, XYZ endTangent)
		{
			XYZ[] controlPoints = new XYZ[fitPoints.Count - 1 + degree];

			// Set endpoints and tangent control points
			controlPoints[0] = fitPoints[0];
			controlPoints[1] = fitPoints[0] + startTangent * ((knots[4] - knots[3]) / 3.0);
			controlPoints[fitPoints.Count + 1] = fitPoints[fitPoints.Count - 1];
			controlPoints[fitPoints.Count] = fitPoints[fitPoints.Count - 1] - endTangent * ((knots[fitPoints.Count + 2] - knots[fitPoints.Count + 1]) / 3.0);

			if (fitPoints.Count - 1 > 1)
			{
				var basis = new double[4];
				var factors = new double[fitPoints.Count];

				// Precompute basis for first interior point
				evaluateBasisFunctions(degree, knots, 4, knots[4], basis);
				double denom = basis[1];
				controlPoints[2] = (fitPoints[1] - basis[0] * controlPoints[1]) / denom;

				// Forward sweep for interior control points
				for (int i = 3; i < fitPoints.Count - 1; i++)
				{
					factors[i] = basis[2] / denom;
					evaluateBasisFunctions(degree, knots, i + 2, knots[i + 2], basis);
					denom = basis[1] - basis[0] * factors[i];
					controlPoints[i] = (fitPoints[i - 1] - basis[0] * controlPoints[i - 1]) / denom;
				}

				// Last interior control point
				factors[fitPoints.Count - 1] = basis[2] / denom;
				evaluateBasisFunctions(degree, knots, fitPoints.Count + 1, knots[fitPoints.Count + 1], basis);
				double denomLast = basis[1];
				double numLeft = basis[0];
				double factorLast = factors[fitPoints.Count - 1];
				controlPoints[fitPoints.Count - 1] = (fitPoints[fitPoints.Count - 2] - basis[2] * controlPoints[fitPoints.Count] - numLeft * controlPoints[fitPoints.Count - 2]) / (denomLast - numLeft * factorLast);

				// Backward substitution for tridiagonal system
				if (fitPoints.Count - 1 <= 2)
				{
					controlPoints[fitPoints.Count - 1] *= 0.75;
				}
				else
				{
					for (int j = fitPoints.Count - 2; j >= 2; j--)
					{
						controlPoints[j] -= factors[j + 1] * controlPoints[j + 1];
					}
				}
			}
			return controlPoints;
		}

		private static double computeNurb(double[] knots, int i, int p, double u)
		{
			if (p <= 0)
			{
				if (knots[i] <= u && u < knots[i + 1])
				{
					return 1;
				}

				return 0.0;
			}

			double leftCoefficient = 0.0;
			if (!(Math.Abs(knots[i + p] - knots[i]) < double.Epsilon))
			{
				leftCoefficient = (u - knots[i]) / (knots[i + p] - knots[i]);
			}

			double rightCoefficient = 0.0; // article contains error here, denominator is Knots[i + p + 1] - Knots[i + 1]
			if (!(Math.Abs(knots[i + p + 1] - knots[i + 1]) < double.Epsilon))
			{
				rightCoefficient = (knots[i + p + 1] - u) / (knots[i + p + 1] - knots[i + 1]);
			}

			return leftCoefficient * computeNurb(knots, i, p - 1, u) + rightCoefficient * computeNurb(knots, i + 1, p - 1, u);
		}

		private static double[] createKnotVector(int numControlPoints, int degree, bool isPeriodic)
		{
			// create knot vector
			int numKnots;
			double[] knots;

			if (!isPeriodic)
			{
				numKnots = numControlPoints + degree + 1;
				knots = new double[numKnots];

				int i;
				for (i = 0; i <= degree; i++)
				{
					knots[i] = 0.0;
				}

				for (; i < numControlPoints; i++)
				{
					knots[i] = i - degree;
				}

				for (; i < numKnots; i++)
				{
					knots[i] = numControlPoints - degree;
				}
			}
			else
			{
				numKnots = numControlPoints + 2 * degree + 1;
				knots = new double[numKnots];

				double factor = 1.0 / (numControlPoints - degree);
				for (int i = 0; i < numKnots; i++)
				{
					knots[i] = (i - degree) * factor;
				}
			}

			return knots;
		}

		private static void evaluateBasisFunctions(int degree, double[] knots, int knotIndex, double u, double[] result)
		{
			//https://www.itwinjs.org/reference/core-geometry/bspline/knotvector/evaluatebasisfunctions/
			var leftBasis = new double[degree + 1];
			var rightBasis = new double[degree + 1];

			result[0] = 1.0;
			for (int i = 0; i < degree; i++)
			{
				leftBasis[i] = u - knots[knotIndex - i];
				rightBasis[i] = knots[knotIndex + i + 1] - u;
				double carry = 0.0;
				for (int j = 0; j <= i; j++)
				{
					double fraction = result[j] / (rightBasis[j] + leftBasis[i - j]);
					result[j] = carry + rightBasis[j] * fraction;
					carry = leftBasis[i - j] * fraction;
				}
				result[i + 1] = carry;
			}
		}

		private static double[] generateKnots(XYZ[] fitPoints, bool applySqrt)
		{
			double[] list = new double[fitPoints.Length];
			double init = 0.0;
			XYZ pt = XYZ.NaN;
			for (int i = 0; i < fitPoints.Length; i++)
			{
				XYZ fp = fitPoints[i];
				if (!pt.IsNaN())
				{
					if (applySqrt)
					{
						init += System.Math.Sqrt((fp - pt).GetLength());
					}
					else
					{
						init += (fp - pt).GetLength();
					}
				}

				list[i] = init;
				pt = fp;
			}

			return list;
		}

		private static double[] generateKnotsUniform(int fitPoints)
		{
			double[] list = new double[fitPoints];
			for (int i = 0; i < fitPoints; i++)
			{
				list[i] = i;
			}

			return list;
		}

		private static double[] generateKnotValues(KnotParametrization parametrization, XYZ[] fitPoints)
		{
			switch (parametrization)
			{
				case KnotParametrization.Chord:
					return generateKnots(fitPoints, false);
				case KnotParametrization.SquareRoot:
					return generateKnots(fitPoints, true);
				case KnotParametrization.Uniform:
					return generateKnotsUniform(fitPoints.Length);
				//Custom cannot be processed
				case KnotParametrization.Custom:
				default:
					return [];
			}
		}

		private static void take3<T>(T[] pts, bool reverse, out T pt1, out T pt2, out T pt3)
																					where T : struct
		{
			if (reverse)
			{
				pt1 = pts[pts.Length - 1];
				pt2 = pts[pts.Length - 2];
				pt3 = pts[pts.Length - 3];
			}
			else
			{
				pt1 = pts[0];
				pt2 = pts[1];
				pt3 = pts[2];
			}
		}

		private void getStartAndEndKnots(double[] knots, out double uStart, out double uEnd)
		{
			if (this.IsClosed)
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
			else if (this.IsPeriodic)
			{
				uStart = knots[this.Degree];
				uEnd = knots[knots.Length - this.Degree - 1];
			}
			else
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
		}

		private void prepare(out XYZ[] controlPts, out double[] weights, out double[] knots)
		{
			var c = this.ControlPoints.ToArray();
			var w = this.Weights.ToArray();
			knots = this.Knots.ToArray();

			// control points
			int numCtrlPoints = c.Length;
			if (numCtrlPoints == 0)
			{
				throw new ArgumentException("A spline entity with control points is required.", nameof(c));
			}

			// weights
			if (w.Length == 0 || w.Length != numCtrlPoints)
			{
				// give the default 1.0 to the control points weights
				w = Enumerable.Repeat(1.0d, this.ControlPoints.Count).ToArray();
			}

			// knots
			if (knots.Length == 0 || !this.HasValidKnotCount)
			{
				knots = createKnotVector(numCtrlPoints, this.Degree, this.IsPeriodic);
			}

			if (this.IsPeriodic)
			{
				controlPts = new XYZ[numCtrlPoints + this.Degree];
				weights = new double[numCtrlPoints + this.Degree];
				for (int i = 0; i < this.Degree; i++)
				{
					int index = numCtrlPoints - this.Degree + i;
					controlPts[i] = c[index];
					weights[i] = w[index];
				}

				c.CopyTo(controlPts, this.Degree);
				w.CopyTo(weights, this.Degree);
			}
			else
			{
				controlPts = c;
				weights = w;
			}

			double uStart;
			double uEnd;
			List<XYZ> vertexes = new List<XYZ>();

			if (this.IsClosed)
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
			else if (this.IsPeriodic)
			{
				uStart = knots[this.Degree];
				uEnd = knots[knots.Length - this.Degree - 1];
			}
			else
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
		}

		private void straightLine()
		{
			// Special case: Bezier curve should be a straight line.
			var fitPoints = this.FitPoints.ToArray();

			this.Knots.Clear();
			this.Knots.AddRange(addSideKnots(this.Degree, generateKnotValues(this.KnotParametrization, fitPoints)));

			this.ControlPoints.Clear();
			XYZ v = (fitPoints[1] - fitPoints[0]) / 3.0;
			this.ControlPoints.Add(fitPoints[0]);
			this.ControlPoints.Add(fitPoints[0] + v);
			this.ControlPoints.Add(fitPoints[1] - v);
			this.ControlPoints.Add(fitPoints[1]);

			this.Weights.Clear();
			this.Weights.AddRange([1, 1, 1, 1]);
		}
	}
}