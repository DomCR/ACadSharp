using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyPushClip : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.PushClip; } }

	public XYZ Extrusion { get; set; }
	public XYZ ClipBoundaryOrigin { get; set; }
	public int PointCount { get; set; }
	public List<XY> Points { get; set; }
	public Matrix4 ClipBoundaryTransformMatrix { get; set; }
	public Matrix4 InverseBlockTransformMatrix { get; set; }
	public int FrontClipOn { get; set; }
	public int BackClipOn { get; set; }
	public double FrontClip { get; set; }
	public double BackClip { get; set; }
	public bool DrawBoundary { get; set; }
}
