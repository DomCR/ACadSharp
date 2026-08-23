using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy push clip graphics object for CAD proxy graphics operations.
/// </summary>
/// <remarks>
/// This class defines clipping boundaries and transformations for proxy graphics rendering,
/// including front/back clip planes and boundary definitions in 3D space.
/// </remarks>
public class ProxyPushClip : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the back clipping plane distance.
	/// </summary>
	/// <value>A double representing the back clip distance in drawing units.</value>
	public double BackClip { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the back clipping plane is enabled.
	/// </summary>
	/// <value><c>true</c> if back clipping is active; otherwise, <c>false</c>.</value>
	public bool BackClipOn { get; set; }

	/// <summary>
	/// Gets or sets the origin point of the clip boundary in 3D space.
	/// </summary>
	/// <value>An <see cref="XYZ"/> coordinate representing the clip boundary origin.</value>
	public XYZ ClipBoundaryOrigin { get; set; }

	/// <summary>
	/// Gets or sets the transformation matrix applied to the clip boundary.
	/// </summary>
	/// <value>A <see cref="Matrix4"/> representing the clip boundary transformation.</value>
	public Matrix4 ClipBoundaryTransformMatrix { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the clip boundary should be drawn.
	/// </summary>
	/// <value><c>true</c> if the boundary should be rendered; otherwise, <c>false</c>.</value>
	public bool DrawBoundary { get; set; }

	/// <summary>
	/// Gets or sets the extrusion direction vector.
	/// </summary>
	/// <value>An <see cref="XYZ"/> vector representing the extrusion direction.</value>
	public XYZ Extrusion { get; set; }

	/// <summary>
	/// Gets or sets the front clipping plane distance.
	/// </summary>
	/// <value>A double representing the front clip distance in drawing units.</value>
	public double FrontClip { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the front clipping plane is enabled.
	/// </summary>
	/// <value><c>true</c> if front clipping is active; otherwise, <c>false</c>.</value>
	public bool FrontClipOn { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.PushClip; } }

	/// <summary>
	/// Gets or sets the inverse block transformation matrix.
	/// </summary>
	/// <value>A <see cref="Matrix4"/> representing the inverse transformation of the block.</value>
	public Matrix4 InverseBlockTransformMatrix { get; set; }

	/// <summary>
	/// Gets or sets the number of points defining the clip boundary polygon.
	/// </summary>
	/// <value>An integer representing the point count.</value>
	public int PointCount { get; set; }

	/// <summary>
	/// Gets or sets the collection of 2D points that define the clip boundary polygon.
	/// </summary>
	/// <value>A <see cref="List{T}"/> of <see cref="XY"/> coordinates representing boundary points.</value>
	public List<XY> ClipBoundary { get; set; }
}