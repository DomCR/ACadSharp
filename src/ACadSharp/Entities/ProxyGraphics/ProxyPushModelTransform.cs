using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyPushModelTransform : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.PushModelTransform; } }

	public Matrix4 TransformationMatrix { get; set; }
}
