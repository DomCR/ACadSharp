using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public abstract class ProxyMeshBase : IProxyGeometry
{
	public EdgeTraits EdgeTraits { get; set; }

	public FaceTraits FaceTraits { get; set; }

	/// <inheritdoc/>
	public abstract GraphicsType GraphicsType { get; }

	public VertexTraits VertexTraits { get; set; }

	public List<XYZ> Vertices { get; set; } = new();
}