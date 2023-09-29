using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Vertex2D"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityVertex"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolylineVertex"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityVertex)]
	[DxfSubClass(DxfSubclassMarker.PolylineVertex)]
	public class Vertex2D : Vertex
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VERTEX_2D;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.PolylineVertex;

		public Vertex2D() { }

		public Vertex2D(XY location)
		{
			this.Location = (XYZ)location;
		}
	}
}
