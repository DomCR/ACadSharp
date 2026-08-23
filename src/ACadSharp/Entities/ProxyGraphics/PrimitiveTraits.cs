using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents the base class for traits associated with graphics primitives in proxy graphics.
/// Contains collections for colors, layer handles, marker identifiers, and visibility indicators.
/// </summary>
public abstract class PrimitiveTraits
{
	/// <summary>
	/// Gets the collection of color values for the graphics primitives.
	/// </summary>
	public List<int> Colors { get; } = new();

	/// <summary>
	/// Gets the collection of layer handles referenced by the graphics primitives.
	/// </summary>
	public List<ulong> LayerHandles { get; } = new();

	/// <summary>
	/// Gets the collection of marker identifiers associated with the graphics primitives.
	/// </summary>
	public List<int> MakerIds { get; } = new();

	/// <summary>
	/// Gets the collection of visibility indicators for the graphics primitives.
	/// </summary>
	public List<int> VisibilityIndicators { get; } = new();
}
