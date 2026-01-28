using ACadSharp.Entities;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadPolyLineTemplate : CadEntityTemplate, ICadOwnerTemplate
	{
		public ulong? FirstVertexHandle { get; internal set; }

		public ulong? LastVertexHandle { get; internal set; }

		public ulong? SeqendHandle { get; internal set; }

		public HashSet<ulong> OwnedObjectsHandlers { get; } = new();

		public CadPolyLineTemplate() : base(new PolyLinePlaceholder())
		{
		}

		public CadPolyLineTemplate(IPolyline entity) : base((Entity)entity)
		{
		}

		public void SetPolyLineObject<T>(Polyline<T> polyLine)
			where T : Entity, IVertex
		{
			polyLine.Handle = this.CadObject.Handle;
			polyLine.Color = this.CadObject.Color;
			polyLine.LineWeight = this.CadObject.LineWeight;
			polyLine.LineTypeScale = this.CadObject.LineTypeScale;
			polyLine.IsInvisible = this.CadObject.IsInvisible;
			polyLine.Transparency = this.CadObject.Transparency;

			this.CadObject = polyLine;
		}

		protected void addVertices(CadDocumentBuilder builder, params IEnumerable<Entity> vertices)
		{
			switch (this.CadObject)
			{
				case Polyline2D pline2d:
					pline2d.Vertices.AddRange(vertices.Cast<Vertex2D>());
					break;
				case Polyline3D pline3d:
					pline3d.Vertices.AddRange(vertices.Cast<Vertex3D>());
					break;
				case PolyfaceMesh mesh:
					foreach (var item in vertices)
					{
						this.addPolyfaceMeshVertex(builder, mesh, item);
					}
					break;
				case PolygonMesh polygon:
					polygon.Vertices.AddRange(vertices.Cast<PolygonMeshVertex>());
					break;
				default:
					builder.Notify($"Unknown polyline type {this.CadObject.SubclassMarker}", NotificationType.Warning);
					break;
			}
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			IPolyline polyLine = this.CadObject as IPolyline;

			if (builder.TryGetCadObject<Seqend>(this.SeqendHandle, out Seqend seqend))
			{
				this.setSeqend(builder, seqend);
			}

			if (this.FirstVertexHandle.HasValue)
			{
				IEnumerable<Vertex> vertices = this.getEntitiesCollection<Vertex>(builder, this.FirstVertexHandle.Value, this.LastVertexHandle.Value);
				this.addVertices(builder, vertices);
			}
			else
			{
				if (this.CadObject is PolyfaceMesh mesh)
				{
					this.buildPolyfaceMesh(mesh, builder);
				}
				else
				{
					foreach (var handle in this.OwnedObjectsHandlers)
					{
						if (builder.TryGetCadObject(handle, out Vertex v))
						{
							this.addVertices(builder, v);
						}
						else if (builder.TryGetCadObject(handle, out Seqend s))
						{
							this.setSeqend(builder, s);
						}
						else
						{
							builder.Notify($"Vertex {handle} not found for polyline {this.CadObject.Handle}", NotificationType.Warning);
						}
					}
				}
			}
		}

		protected void setSeqend(CadDocumentBuilder builder, Seqend seqend)
		{
			switch (this.CadObject)
			{
				case Polyline2D pline2d:
					pline2d.Vertices.Seqend = seqend;
					break;
				case Polyline3D pline3d:
					pline3d.Vertices.Seqend = seqend;
					break;
				case PolyfaceMesh mesh:
					mesh.Vertices.Seqend = seqend;
					break;
				case PolygonMesh polygon:
					polygon.Vertices.Seqend = seqend;
					break;
				default:
					builder.Notify($"Unknown polyline type {this.CadObject.SubclassMarker}", NotificationType.Warning);
					break;
			}
		}

		private void buildPolyfaceMesh(PolyfaceMesh polyfaceMesh, CadDocumentBuilder builder)
		{
			foreach (var handle in this.OwnedObjectsHandlers)
			{
				if (builder.TryGetCadObject(handle, out Entity e))
				{
					this.addPolyfaceMeshVertex(builder, polyfaceMesh, e);
				}
			}
		}

		private void addPolyfaceMeshVertex(CadDocumentBuilder builder, PolyfaceMesh polyfaceMesh, Entity e)
		{
			if (e is VertexFaceMesh v3)
			{
				polyfaceMesh.Vertices.Add(v3);
			}
			else if (e is VertexFaceRecord face)
			{
				polyfaceMesh.Faces.Add(face);
			}
			else if (e is Seqend seqend)
			{
				polyfaceMesh.Vertices.Seqend = seqend;
			}
			else
			{
				builder.Notify($"Unidentified type for PolyfaceMesh {e.GetType().FullName}");
			}
		}

		internal class PolyLinePlaceholder : Polyline<Vertex>
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }
		}
	}
}