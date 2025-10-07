using ACadSharp.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACadSharp.IO.Templates
{
	internal class CadPolyLineTemplate : CadEntityTemplate
	{
		public ulong? FirstVertexHandle { get; internal set; }

		public ulong? LastVertexHandle { get; internal set; }

		public IPolyline PolyLine => this.CadObject as IPolyline;

		public ulong? SeqendHandle { get; internal set; }

		public HashSet<ulong> VertexHandles { get; set; } = new();

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

		protected void addVertices(CadDocumentBuilder builder, params IEnumerable<IVertex> vertices)
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
					mesh.Vertices.AddRange(vertices.Cast<VertexFaceMesh>());
					break;
				default:
					builder.Notify($"Unknown polyline type {this.CadObject.SubclassMarker}");
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
					foreach (var handle in this.VertexHandles)
					{
						if (builder.TryGetCadObject(handle, out Vertex v))
						{
							this.addVertices(builder, v);
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
				default:
					builder.Notify($"Unknown polyline type {this.CadObject.SubclassMarker}");
					break;
			}
		}

		private void buildPolyfaceMesh(PolyfaceMesh polyfaceMesh, CadDocumentBuilder builder)
		{
			foreach (var handle in this.VertexHandles)
			{
				if (builder.TryGetCadObject(handle, out Entity e))
				{
					if (e is VertexFaceMesh v3)
					{
						polyfaceMesh.Vertices.Add(v3);
					}
					else if (e is VertexFaceRecord face)
					{
						polyfaceMesh.Faces.Add(face);
					}
					else
					{
						builder.Notify($"Unidentified type for PolyfaceMesh {e.GetType().FullName}");
					}
				}
			}
		}

		internal class PolyLinePlaceholder : Polyline<Vertex>
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }
		}
	}
}