using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PolyfaceMesh"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyFaceMesh"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolyfaceMesh"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyFaceMesh)]
	[DxfSubClass(DxfSubclassMarker.PolyfaceMesh)]
	public class PolyfaceMesh : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.POLYLINE_PFACE; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPolyFaceMesh;

		public SeqendCollection<Vertex3D> Vertices { get; }

		public SeqendCollection<VertexFaceRecord> Faces { get; }

		public PolyfaceMesh()
		{
			this.Vertices = new SeqendCollection<Vertex3D>(this);
			this.Faces = new SeqendCollection<VertexFaceRecord>(this);
		}
	}
}
