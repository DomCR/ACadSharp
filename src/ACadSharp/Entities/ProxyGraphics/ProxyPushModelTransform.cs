using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics push model transform.
/// </summary>
public class ProxyPushModelTransform : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.PushModelTransform; } }

	/// <summary>
	/// Gets or sets the transformation matrix.
	/// </summary>
	public Matrix4 TransformationMatrix { get; set; }
}