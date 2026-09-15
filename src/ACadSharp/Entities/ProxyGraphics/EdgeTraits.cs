using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents the traits and properties of edges in proxy graphics.
/// </summary>
/// <remarks>
/// This class extends <see cref="PrimitiveTraits"/> to provide edge-specific characteristics
/// used in the representation of proxy graphic entities in AutoCAD documents.
/// </remarks>
public class EdgeTraits : PrimitiveTraits
{
	/// <summary>
	/// Gets the collection of line type handles associated with this edge.
	/// </summary>
	/// <value>
	/// A <see cref="List{T}"/> of unsigned 64-bit integers representing handles to line types.
	/// </value>
	public List<ulong> LineTypeHandles { get; } = new();
}
