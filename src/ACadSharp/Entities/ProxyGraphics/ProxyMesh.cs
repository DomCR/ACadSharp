namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy mesh graphics entity in AutoCAD.
/// </summary>
public class ProxyMesh : ProxyMeshBase
{
	/// <summary>
	/// Gets or sets the number of columns in the mesh.
	/// </summary>
	public int ColumnCount { get; set; }

	/// <inheritdoc/>
	public override GraphicsType GraphicsType { get { return GraphicsType.Mesh; } }

	/// <summary>
	/// Gets or sets the number of rows in the mesh.
	/// </summary>
	public int RowCount { get; set; }
}
