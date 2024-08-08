using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="VertexFaceRecord"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityVertex"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolyfaceMeshVertex"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityVertex)]
	[DxfSubClass(DxfSubclassMarker.PolyfaceMeshFace)]
	public class VertexFaceRecord : Vertex
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VERTEX_PFACE_FACE;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.PolyfaceMeshFace;

		/// <summary>
		/// Vert index BS 71 1 - based vertex index(see DXF doc)
		/// </summary>
		[DxfCodeValue(71)]
		public short Index1 { get; set; }

		/// <summary>
		/// Vert index BS 72 1 - based vertex index(see DXF doc)
		/// </summary>
		[DxfCodeValue(72)]
		public short Index2 { get; set; }

		/// <summary>
		/// Vert index BS 73 1 - based vertex index(see DXF doc)
		/// </summary>
		[DxfCodeValue(73)]
		public short Index3 { get; set; }

		/// <summary>
		/// Vert index BS 74 1 - based vertex index(see DXF doc)
		/// </summary>
		[DxfCodeValue(74)]
		public short Index4 { get; set; }
	}
}
