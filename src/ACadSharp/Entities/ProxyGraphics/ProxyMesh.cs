namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyMesh : ProxyMeshBase
{
	public int ColumnCount { get; set; }

	/// <inheritdoc/>
	public override GraphicsType GraphicsType { get { return GraphicsType.Mesh; } }

	public int RowCount { get; set; }
}
