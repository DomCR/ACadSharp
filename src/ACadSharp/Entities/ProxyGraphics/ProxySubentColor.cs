namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentColor : IProxyGeometry
{
	public Color Color
	{
		get
		{
			return new Color((short)this.ColorIndex);
		}
	}

	public int ColorIndex { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentColor; } }
}