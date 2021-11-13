using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgPolyLineTemplate : DwgEntityTemplate
	{
		public ulong? FirstVertexHandle { get; internal set; }
		public ulong? LastVertexHandle { get; internal set; }
		public ulong SeqendHandle { get; internal set; }
		public List<ulong> VertexHandles { get; set; } = new List<ulong>();

		public DwgPolyLineTemplate(PolyLine entity) : base(entity) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			PolyLine polyLine = this.CadObject as PolyLine;

			if (FirstVertexHandle.HasValue)
			{
				IEnumerable<Vertex> vertices = this.getEntitiesCollection<Vertex>(builder, FirstVertexHandle.Value, LastVertexHandle.Value);
				polyLine.Vertices.AddRange(vertices);
			}
			else
			{
				foreach (var handle in this.VertexHandles)
				{
					polyLine.Vertices.Add(builder.GetCadObject<Vertex>(handle));
				}
			}

			//TODO: DwgPolyLineTemplate SeqendHandle??
		}
	}
}
