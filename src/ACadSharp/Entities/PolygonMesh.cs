using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PolygonMesh"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolygonMesh"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(DxfSubclassMarker.PolygonMesh)]
	public class PolygonMesh : Polyline<PolygonMeshVertex>
	{
		/// <inheritdoc/>
		public override PolylineFlags Flags { get => base.Flags | (PolylineFlags.PolygonMesh); set => base.Flags = value; }

		/// <summary>
		/// Gets or sets the number of smoothing surface density divisions for the M direction.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 73)]
		public short MSmoothSurfaceDensity { get; set; }

		/// <summary>
		/// Gets or sets the number of vertices in the M direction.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 71)]
		public short MVertexCount { get; set; }

		/// <summary>
		/// Gets or sets the number of smoothing surface density divisions for the N direction.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 74)]
		public short NSmoothSurfaceDensity { get; set; }

		/// <summary>
		/// Gets or sets the number of vertices in the N direction.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 72)]
		public short NVertexCount { get; set; }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.POLYLINE_MESH;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.PolygonMesh;
	}
}