using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics entity that pushes a model transformation matrix onto the stack.
/// </summary>
public class ProxyPushModelTransform2 : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.PushModelTransform2; } }

	/// <summary>
	/// Gets or sets the transformation matrix to be pushed onto the stack.
	/// </summary>
	public Matrix4 TransformationMatrix { get; set; }
}