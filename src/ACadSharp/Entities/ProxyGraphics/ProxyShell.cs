using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyShell : IProxyGeometry
{
	public int FaceCount { get; set; }

	public List<List<XYZ>> Faces { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Shell; } }

	public List<XYZ> Vertices { get; set; }
}