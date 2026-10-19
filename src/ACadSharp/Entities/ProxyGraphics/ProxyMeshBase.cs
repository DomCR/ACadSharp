using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy mesh geometry in AutoCAD.
/// </summary>
public abstract class ProxyMeshBase : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the traits associated with the edges of the mesh.
	/// </summary>
	public EdgeTraits EdgeTraits { get; set; }

	/// <summary>
	/// Gets or sets the traits associated with the faces of the mesh.
	/// </summary>
	public FaceTraits FaceTraits { get; set; }

	/// <inheritdoc/>
	public abstract GraphicsType GraphicsType { get; }

	/// <summary>
	/// Gets or sets the traits associated with the vertices of the mesh.
	/// </summary>
	public VertexTraits VertexTraits { get; set; }

	/// <summary>
	/// Gets or sets the list of vertices that define the mesh geometry.
	/// </summary>
	public List<XYZ> Vertices { get; set; } = new();

	/// <inheritdoc/>
	public virtual IProxyGeometry Clone()
	{
		ProxyMeshBase clone = (ProxyMeshBase)this.MemberwiseClone();

		clone.Vertices = this.Vertices != null ? new List<XYZ>(this.Vertices) : null;

		if (this.EdgeTraits != null)
		{
			clone.EdgeTraits = new EdgeTraits();
			copyTraits(this.EdgeTraits, clone.EdgeTraits);
			clone.EdgeTraits.LineTypeHandles.AddRange(this.EdgeTraits.LineTypeHandles);
		}

		if (this.FaceTraits != null)
		{
			clone.FaceTraits = new FaceTraits();
			copyTraits(this.FaceTraits, clone.FaceTraits);
			clone.FaceTraits.Normals.AddRange(this.FaceTraits.Normals);
		}

		if (this.VertexTraits != null)
		{
			clone.VertexTraits = new VertexTraits
			{
				Orientation = this.VertexTraits.Orientation,
				Normals = this.VertexTraits.Normals != null ? new List<XYZ>(this.VertexTraits.Normals) : null
			};
		}

		return clone;
	}

	private static void copyTraits(PrimitiveTraits source, PrimitiveTraits target)
	{
		target.Colors.AddRange(source.Colors);
		target.LayerHandles.AddRange(source.LayerHandles);
		target.MakerIds.AddRange(source.MakerIds);
		target.VisibilityIndicators.AddRange(source.VisibilityIndicators);
	}
}