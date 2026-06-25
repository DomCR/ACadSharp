using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents the traits of a vertex in proxy graphics.
/// </summary>
public class VertexTraits
{
	/// <summary>
	/// Gets or sets the list of normals associated with the vertex.
	/// </summary>
	public List<XYZ> Normals { get; set; }

	/// <summary>
	/// Gets or sets the orientation of the vertex.
	/// </summary>
	public int Orientation { get; set; }
}