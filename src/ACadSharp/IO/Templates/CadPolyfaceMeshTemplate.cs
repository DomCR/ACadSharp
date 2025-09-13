using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadPolyfaceMeshTemplate : CadEntityTemplate
	{
		public ulong? FirstVerticeHandle { get; set; }

		public ulong? LastVerticeHandle { get; set; }

		public HashSet<ulong> VerticesHandles { get; set; } = new();

		public ulong? SeqendHandle { get; set; }

		public CadPolyfaceMeshTemplate(PolyfaceMesh polyfaceMesh) : base(polyfaceMesh) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			PolyfaceMesh polyfaceMesh = (PolyfaceMesh)this.CadObject;

			if (builder.TryGetCadObject<Seqend>(this.SeqendHandle, out Seqend seqend))
			{
				polyfaceMesh.Vertices.Seqend = seqend;
			}

			if (this.FirstVerticeHandle.HasValue)
			{
				IEnumerable<Entity> vertices = this.getEntitiesCollection<Entity>(builder, this.FirstVerticeHandle.Value, this.LastVerticeHandle.Value);
				foreach (var item in vertices)
				{
					this.addItemToPolyface(item, builder);
				}
			}
			else
			{
				foreach (ulong handle in this.VerticesHandles)
				{
					if (builder.TryGetCadObject<CadObject>(handle, out CadObject item))
					{
						this.addItemToPolyface(item, builder);
					}
				}
			}
		}

		private void addItemToPolyface(CadObject item, CadDocumentBuilder builder)
		{
			PolyfaceMesh polyfaceMesh = (PolyfaceMesh)this.CadObject;

			if (item is VertexFaceMesh v3)
			{
				polyfaceMesh.Vertices.Add(v3);
			}
			else if (item is VertexFaceRecord face)
			{
				polyfaceMesh.Faces.Add(face);
			}
			else
			{
				builder.Notify($"Unidentified type for PolyfaceMesh {item.GetType().FullName}");
			}
		}
	}
}
