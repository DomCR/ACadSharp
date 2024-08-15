using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Defines the vertex flags.
	/// </summary>
	[Flags]
	public enum VertexFlags
	{
		/// <summary>
		/// Default.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Extra vertex created by curve-fitting.
		/// </summary>
		CurveFittingExtraVertex = 1,
		/// <summary>
		/// Curve-fit tangent defined for this vertex.
		/// A curve-fit tangent direction of 0 may be omitted from DXF output but is significant if this bit is set.
		/// </summary>
		CurveFitTangent = 2,
		/// <summary>
		/// Not used.
		/// </summary>
		NotUsed = 4,
		/// <summary>
		/// Spline vertex created by spline-fitting.
		/// </summary>
		SplineVertexFromSplineFitting = 8,
		/// <summary>
		/// Spline frame control point.
		/// </summary>
		SplineFrameControlPoint = 16,
		/// <summary>
		/// 3D polyline vertex.
		/// </summary>
		PolylineVertex3D = 32,
		/// <summary>
		/// 3D polygon mesh.
		/// </summary>
		PolygonMesh3D = 64,
		/// <summary>
		/// Polyface mesh vertex.
		/// </summary>
		PolyfaceMeshVertex = 128
	}
}
