using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyPushModelTransform2 : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.PushModelTransform2; } }

	public Matrix4 TransformationMatrix { get; set; }
}