namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy subentity line type.
/// </summary>
public class ProxySubentLineType : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLineType; } }

	/// <summary>
	/// Defines if the color is defined by block.
	/// </summary>
	public bool IsByBlock
	{
		get { return this.LineTypeIndex == 0xFFFFFFFE; }
	}

	/// <summary>
	/// Defines if the color is defined by layer.
	/// </summary>
	public bool IsByLayer
	{
		get { return this.LineTypeIndex == 0xFFFFFFFF; }
	}

	public uint LineTypeIndex { get; set; }
}