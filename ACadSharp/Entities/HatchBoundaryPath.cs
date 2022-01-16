using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class HatchBoundaryPath
	{
		#region Boundary path edge classes
		public abstract class Edge
		{

		}
		public class Line : Edge
		{
			public XY Start { get; set; }
			public XY End { get; set; }
		}
		public class Arc : Edge
		{
			public XY Center { get; set; }
			public double Radius { get; set; }
			public double StartAngle { get; set; }
			public double EndAngle { get; set; }
			public bool CounterClockWise { get; set; }
		}
		public class Ellipse : Edge
		{
			public XY Center { get; set; }
			public XY MajorAxisEndPoint { get; set; }
			public double MinorToMajorRatio { get; set; }
			public double StartAngle { get; set; }
			public double EndAngle { get; set; }
			public bool CounterClockWise { get; set; }
		}
		public class Spline : Edge
		{
			public int Degree { get; internal set; }
			public bool Rational { get; internal set; }
			public bool Periodic { get; internal set; }
			public List<double> Knots { get; set; } = new List<double>();
			/// <remarks>
			/// Position values are only X and Y, Z represents the weight.
			/// </remarks>
			public List<XYZ> ControlPoints { get; set; } = new List<XYZ>();
		}
		public class Polyline : Edge
		{
			public bool IsClosed { get; set; }
			/// <remarks>
			/// Position values are only X and Y, Z represents the Bulge.
			/// </remarks>
			public List<XYZ> Vertices { get; set; } = new List<XYZ>();
		}
		#endregion

		public List<Edge> Edges { get; set; } = new List<Edge>();
		public BoundaryPathFlags Flags { get; set; }
	}
}
