using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PolyFaceMesh"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolyfaceMesh"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(DxfSubclassMarker.PolyfaceMesh)]
	public class PolyFaceMesh : Polyline
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;   //TODO: Investigate the PolyfaceMesh ObjectType code

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.PolyfaceMesh;

		public PolyFaceMesh() : base()
		{
			this.Vertices.OnAdd += this.verticesOnAdd;
		}

		public override IEnumerable<Entity> Explode()
		{
			return Polyline.explode(this);
		}

		private void verticesOnAdd(object sender, CollectionChangedEventArgs e)
		{
		}
	}
}
