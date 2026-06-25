using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyShell : ProxyMeshBase
{
	public List<int[]> Faces { get; set; } = new();
	
	/// <inheritdoc/>
	public override GraphicsType GraphicsType { get { return GraphicsType.Shell; } }
}
