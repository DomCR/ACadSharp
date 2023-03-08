using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadPolyfaceMeshTemplate : CadEntityTemplate
	{
		public CadPolyfaceMeshTemplate(PolyfaceMesh polyfaceMesh) : base(polyfaceMesh) { }

		public ulong? FirstVerticeHandle { get; set; }

		public ulong? LastVerticeHandle { get; set; }

		public List<ulong> VerticesHandles { get; set; } = new List<ulong>();

		public ulong SeqendHandle { get; set; }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			PolyfaceMesh polyfaceMesh = (PolyfaceMesh)this.CadObject;

			if (this.FirstVerticeHandle.HasValue)
			{
				var vertices = this.getEntitiesCollection<Vertex3D>(builder, this.FirstVerticeHandle.Value, this.LastVerticeHandle.Value);
				polyfaceMesh.Vertices.AddRange(vertices);
			}
			else
			{
				foreach (ulong handle in VerticesHandles)
				{
					if (builder.TryGetCadObject<Vertex3D>(handle, out Vertex3D v3))
					{
						polyfaceMesh.Vertices.Add(v3);
					}
				}
			}

			if (builder.TryGetCadObject<Seqend>(this.SeqendHandle, out Seqend seqend))
			{
				polyfaceMesh.Vertices.Seqend = seqend;
			}
		}
	}
}
