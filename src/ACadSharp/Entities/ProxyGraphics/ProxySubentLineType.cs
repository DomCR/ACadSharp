namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentLineType : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLineType; } }

	public uint LinetypeIndex { get; set; }

	/// <summary>
	/// Defines if the color is defined by block.
	/// </summary>
	public bool IsByBlock
	{
		get { return this.LinetypeIndex == 0xFFFFFFFE; }
	}

	/// <summary>
	/// Defines if the color is defined by layer.
	/// </summary>
	public bool IsByLayer
	{
		get { return this.LinetypeIndex == 0xFFFFFFFF; }
	}
}
