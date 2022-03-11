using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadPolyLineTemplate : CadEntityTemplate
	{
		public ulong? FirstVertexHandle { get; internal set; }
		public ulong? LastVertexHandle { get; internal set; }
		public ulong SeqendHandle { get; internal set; }
		public List<ulong> VertexHandles { get; set; } = new List<ulong>();

		public CadPolyLineTemplate() : base(new PolyLinePlaceholder()) { }

		public CadPolyLineTemplate(PolyLine entity) : base(entity) { }

		public override void Build(CadDocumentBuilder builder)
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

		public void SetPolyLineObject(PolyLine polyLine)
		{
			polyLine.Handle = this.CadObject.Handle;
			polyLine.Owner = this.CadObject.Owner;
			polyLine.Dictionary = this.CadObject.Dictionary;
			//polyLine.Reactors = this.CadObject.Reactors;
			//polyLine.ExtendedData = this.CadObject.ExtendedData;

			polyLine.Color = this.CadObject.Color;
			polyLine.Lineweight = this.CadObject.Lineweight;
			polyLine.LinetypeScale = this.CadObject.LinetypeScale;
			polyLine.IsInvisible = this.CadObject.IsInvisible;
			polyLine.Transparency = this.CadObject.Transparency;

			this.CadObject = polyLine;
		}

		public class PolyLinePlaceholder : PolyLine
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }
		}
	}
}
