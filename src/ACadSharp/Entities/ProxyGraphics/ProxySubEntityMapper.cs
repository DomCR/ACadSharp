namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy sub-entity mapper for handling proxy graphics geometry mapping.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IProxyGeometry"/> interface and provides properties
/// for managing the transformation and tiling parameters of proxy sub-entities in the graphics system.
/// </remarks>
public class ProxySubEntityMapper : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the automatic transformation setting for the proxy sub-entity mapper.
	/// </summary>
	public int AutoTransform { get; set; }

	/// <summary>
	/// Gets or sets the first dummy value reserved for future use.
	/// </summary>
	public int DummyValue1 { get; set; }

	/// <summary>
	/// Gets or sets the second dummy value reserved for future use.
	/// </summary>
	public int DummyValue2 { get; set; }

	/// <summary>
	/// Gets or sets the third dummy value reserved for future use.
	/// </summary>
	public int DummyValue3 { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubEntityMapper; } }

	/// <summary>
	/// Gets or sets the projection setting for the proxy sub-entity mapper.
	/// </summary>
	public int Projection { get; set; }

	/// <summary>
	/// Gets or sets the U-direction tiling parameter for the proxy sub-entity mapper.
	/// </summary>
	public int UTiling { get; set; }

	/// <summary>
	/// Gets or sets the V-direction tiling parameter for the proxy sub-entity mapper.
	/// </summary>
	public int VTiling { get; set; }
}