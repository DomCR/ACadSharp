using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyMesh : IProxyGeometry
{
	public int ColumnCount { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Mesh; } }

	public int RowCount { get; set; }

	public List<XYZ> Vertices { get; private set; } = new();
}

public abstract class ProxyMeshBase : IProxyGeometry
{
	/// <inheritdoc/>
	public abstract GraphicsType GraphicsType { get; }
}