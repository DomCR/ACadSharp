using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyShell : ProxyMeshBase
{
	public int FaceCount { get; set; }

	public List<List<XYZ>> Faces { get; set; } = new ();

	/// <inheritdoc/>
	public override GraphicsType GraphicsType { get { return GraphicsType.Shell; } }
}
