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

		/// <summary>
		/// Face records with the triangle indexes.
		/// </summary>
		public CadObjectCollection<VertexFaceRecord> Faces { get; private set; }

		/// <inheritdoc/>
		public PolyfaceMesh() : base()
		{
			this.Faces = new CadObjectCollection<VertexFaceRecord>(this);
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> Explode()
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			PolyfaceMesh clone = (PolyfaceMesh)base.Clone();

			clone.Faces = new SeqendCollection<VertexFaceRecord>(clone);
			foreach (VertexFaceRecord v in this.Faces)
			{
				clone.Faces.Add((VertexFaceRecord)v.Clone());
			}

			return clone;
		}

		protected override void verticesOnAdd(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is not VertexFaceMesh)
			{
				this.Vertices.Remove((Vertex)e.Item);
				throw new ArgumentException($"Wrong vertex type {e.Item.SubclassMarker} for {this.SubclassMarker}");
			}
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);
			doc.RegisterCollection(this.Faces);
		}

		internal override void UnassignDocument()
		{
			this.Document.UnregisterCollection(this.Faces);
			base.UnassignDocument();
		}
	}
}
