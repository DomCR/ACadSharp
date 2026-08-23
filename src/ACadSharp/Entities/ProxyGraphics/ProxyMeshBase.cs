using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy mesh geometry in AutoCAD.
/// </summary>
public abstract class ProxyMeshBase : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the traits associated with the edges of the mesh.
	/// </summary>
	public EdgeTraits EdgeTraits { get; set; }

	/// <summary>
	/// Gets or sets the traits associated with the faces of the mesh.
	/// </summary>
	public FaceTraits FaceTraits { get; set; }

	/// <inheritdoc/>
	public abstract GraphicsType GraphicsType { get; }

	/// <summary>
	/// Gets or sets the traits associated with the vertices of the mesh.
	/// </summary>
	public VertexTraits VertexTraits { get; set; }

	/// <summary>
	/// Gets or sets the list of vertices that define the mesh geometry.
	/// </summary>
	public List<XYZ> Vertices { get; set; } = new();
}