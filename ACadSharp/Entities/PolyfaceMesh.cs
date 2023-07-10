using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PolyfaceMesh"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.PolyfaceMesh"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(DxfSubclassMarker.PolyfaceMesh)]
	public class PolyfaceMesh : Polyline
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.POLYLINE_PFACE; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPolyline;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.PolyfaceMesh;

		public SeqendCollection<VertexFaceRecord> Faces { get; }

		public PolyfaceMesh()
		{
			this.Vertices.OnAdd += this.verticesOnAdd;
			this.Faces = new SeqendCollection<VertexFaceRecord>(this);
		}

		public override IEnumerable<Entity> Explode()
		{
			throw new System.NotImplementedException();
		}

		private void verticesOnAdd(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is not VertexFaceMesh)
			{
				this.Vertices.Remove((Vertex)e.Item);
				throw new ArgumentException($"Wrong vertex type {e.Item.SubclassMarker} for {this.SubclassMarker}");
			}
		}
	}
}
