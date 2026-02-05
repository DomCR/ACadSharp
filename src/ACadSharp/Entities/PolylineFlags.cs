using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Defines the polyline flags.
	/// </summary>
	[Flags]
	public enum PolylineFlags
	{
		/// <summary>
		/// Default, open polyline.
		/// </summary>
		Default = 0,
		/// <summary>
		/// This is a closed polyline (or a polygon mesh closed in the M direction).
		/// </summary>
		ClosedPolylineOrClosedPolygonMeshInM = 1,
		/// <summary>
		/// Curve-fit vertexes have been added.
		/// </summary>
		CurveFit = 2,
		/// <summary>
		/// Spline-fit vertexes have been added.
		/// </summary>
		SplineFit = 4,
		/// <summary>
		/// This is a 3D polyline.
		/// </summary>
		Polyline3D = 8,
		/// <summary>
		/// This is a 3D polygon mesh.
		/// </summary>
		PolygonMesh = 16,
		/// <summary>
		/// The polygon mesh is closed in the N direction.
		/// </summary>
		ClosedPolygonMeshInN = 32,
		/// <summary>
		/// The polyline is a polyface mesh.
		/// </summary>
		PolyfaceMesh = 64,
		/// <summary>
		/// The line type pattern is generated continuously around the vertexes of this polyline.
		/// </summary>
		ContinuousLinetypePattern = 128
	}
}
