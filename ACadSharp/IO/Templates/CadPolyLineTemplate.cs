using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadPolyLineTemplate : CadEntityTemplate
	{
		public ulong? FirstVertexHandle { get; internal set; }
		public ulong? LastVertexHandle { get; internal set; }
		public ulong? SeqendHandle { get; internal set; }
		public List<ulong> VertexHandles { get; set; } = new List<ulong>();

		public CadPolyLineTemplate() : base(new PolyLinePlaceholder()) { }

		public CadPolyLineTemplate(Polyline entity) : base(entity) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Polyline polyLine = this.CadObject as Polyline;

			if (this.FirstVertexHandle.HasValue)
			{
				IEnumerable<Vertex> vertices = this.getEntitiesCollection<Vertex>(builder, this.FirstVertexHandle.Value, this.LastVertexHandle.Value);
				polyLine.Vertices.AddRange(vertices);
			}
			else
			{
				foreach (var handle in this.VertexHandles)
				{
					polyLine.Vertices.Add(builder.GetCadObject<Vertex>(handle));
				}
			}

			if (builder.TryGetCadObject<Seqend>(this.SeqendHandle, out Seqend seqend))
			{
				polyLine.Vertices.Seqend = seqend;
			}
		}

		public void SetPolyLineObject(Polyline polyLine)
		{
			polyLine.Handle = this.CadObject.Handle;
			polyLine.Owner = this.CadObject.Owner;

			polyLine.XDictionary = this.CadObject.XDictionary;

			//polyLine.Reactors = this.CadObject.Reactors;
			//polyLine.ExtendedData = this.CadObject.ExtendedData;

			polyLine.Color = this.CadObject.Color;
			polyLine.LineWeight = this.CadObject.LineWeight;
			polyLine.LinetypeScale = this.CadObject.LinetypeScale;
			polyLine.IsInvisible = this.CadObject.IsInvisible;
			polyLine.Transparency = this.CadObject.Transparency;

			this.CadObject = polyLine;
		}

		public class PolyLinePlaceholder : Polyline
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }

			public override IEnumerable<Entity> Explode()
			{
				throw new NotImplementedException();
			}
		}
	}
}
