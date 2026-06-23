using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyShell : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Shell; } }

	public int PointCount { get; set; }
	public List<XYZ> Vertices { get; set; }
	public int FaceCount { get; set; }
	public List<List<XYZ>> Faces { get; set; }
}
