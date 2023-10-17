using System;

namespace ACadSharp.Header
{
	//https://help.autodesk.com/view/ACD/2020/ENU/?guid=GUID-DD9B3216-A533-4D47-95D8-7585F738FD75

	/// <summary>
	/// Object snap mode AcDb::OsnapMask
	/// </summary>
	[Flags]
	public enum ObjectSnapMode : ushort
	{
		/// <summary>
		/// Switch off all snapping modes
		/// </summary>
		None = 0,
		/// <summary>
		/// Snap to end point of lines, arcs, and to polyline vertices
		/// </summary>
		EndPoint = 1,
		/// <summary>
		/// Snap to mid point of lines, arcs, and polyline segments
		/// </summary>
		MidPoint = 2,
		/// <summary>
		/// Snap to center of circles, arcs. and polyline arc segments
		/// </summary>
		Center = 4,
		/// <summary>
		/// Snap to the center of point objects
		/// </summary>
		Node = 8,
		/// <summary>
		/// Snap to circle quadrant points at 0°, 90°, 180°, and 270°.
		/// </summary>
		Quadrant = 16, // 0x0010
		/// <summary>
		/// Snap to intersections of any two drawing objects
		/// </summary>
		Intersection = 32, // 0x0020
		/// <summary>
		/// Snap to insertion points of blocks, texts, and images.
		/// </summary>
		Insertion = 64, // 0x0040
		/// <summary>
		/// Snap to points which form a perpendicular with the selected object.
		/// </summary>
		Perpendicular = 128, // 0x0080
		/// <summary>
		/// Snap to tangent points
		/// </summary>
		Tangent = 256, // 0x0100
		/// <summary>
		/// Snap to the nearest point on a drawing object
		/// </summary>
		Nearest = 512, // 0x0200
		/// <summary>
		/// Clears all object snaps
		/// </summary>
		ClearsAllObjectSnaps = 1024, // 0x0400
		/// <summary>
		/// Snap to apparent intersections in the view
		/// </summary>
		ApparentIntersection = 2048, // 0x0800
		/// <summary>
		/// Snap to extensions of lines
		/// </summary>
		Extension = 4096, // 0x1000
		/// <summary>
		/// Parallel lines
		/// </summary>
		Parallel = 8192, // 0x2000
		/// <summary>
		/// All snapping modes on
		/// </summary>
		AllModes = Parallel | Extension | ApparentIntersection | ClearsAllObjectSnaps | Nearest | Tangent | Perpendicular | Insertion | Intersection | Quadrant | Node | Center | MidPoint | EndPoint, // 0x3FFF
		/// <summary>
		/// Switch off snapping
		/// </summary>
		SwitchedOff = 16384, // 0x4000
	}
}
