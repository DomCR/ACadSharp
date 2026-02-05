using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="VertexFaceRecord"/> entity.
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
		/// Index 1 of the face.
		/// </summary>
		/// <remarks>
		/// The start value for indexes is 1, if 0 the index will not take an effect.
		/// </remarks>
		[DxfCodeValue(71)]
		public short Index1 { get; set; }

		/// <summary>
		/// Index 2 of the face.
		/// </summary>
		/// <remarks>
		/// The start value for indexes is 1, if 0 the index will not take an effect.
		/// </remarks>
		[DxfCodeValue(72)]
		public short Index2 { get; set; }

		/// <summary>
		/// Index 3 of the face.
		/// </summary>
		/// <remarks>
		/// The start value for indexes is 1, if 0 the index will not take an effect.
		/// </remarks>
		[DxfCodeValue(73)]
		public short Index3 { get; set; }

		/// <summary>
		/// Index 4 of the face.
		/// </summary>
		/// <remarks>
		/// The start value for indexes is 1, if 0 the index will not take an effect.
		/// </remarks>
		[DxfCodeValue(74)]
		public short Index4 { get; set; }
	}
}
