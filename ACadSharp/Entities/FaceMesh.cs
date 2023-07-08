using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="FaceMesh"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityVertex"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolyfaceMeshFace"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityVertex)]
	[DxfSubClass(DxfSubclassMarker.PolyfaceMeshFace)]
	public class FaceMesh : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.VERTEX_PFACE_FACE; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityVertex;

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
