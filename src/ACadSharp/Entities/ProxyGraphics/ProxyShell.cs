using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy shell entity in a CAD drawing. A proxy shell is a 3D representation of a solid or surface that is not natively supported by the CAD software, allowing for visualization and interaction with the geometry.
/// </summary>
public class ProxyShell : ProxyMeshBase
{
	/// <summary>
	/// The faces of the shell. Each face is represented by an array of integers, where each integer corresponds to a vertex index in the <see cref="ProxyMeshBase.Vertices"/> list.
	/// </summary>
	public List<int[]> Faces { get; set; } = new();

	/// <inheritdoc/>
	public override GraphicsType GraphicsType { get { return GraphicsType.Shell; } }
}
