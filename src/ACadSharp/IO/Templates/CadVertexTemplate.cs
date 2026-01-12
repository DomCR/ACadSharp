using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadVertexTemplate : CadEntityTemplate
	{
		public Vertex Vertex => this.CadObject as Vertex;

		public CadVertexTemplate() : base(new VertexPlaceholder())
		{
		}

		public CadVertexTemplate(Vertex vertex) : base(vertex)
		{
		}

		internal void SetVertexObject(Vertex vertex)
		{
			vertex.Handle = this.CadObject.Handle;
			vertex.Owner = this.CadObject.Owner;

			vertex.XDictionary = this.CadObject.XDictionary;

			//polyLine.Reactors = this.CadObject.Reactors;
			//polyLine.ExtendedData = this.CadObject.ExtendedData;

			vertex.Color = this.CadObject.Color;
			vertex.LineWeight = this.CadObject.LineWeight;
			vertex.LineTypeScale = this.CadObject.LineTypeScale;
			vertex.IsInvisible = this.CadObject.IsInvisible;
			vertex.Transparency = this.CadObject.Transparency;

			Vertex placeholder = this.CadObject as Vertex;

			vertex.Location = placeholder.Location;
			vertex.StartWidth = placeholder.StartWidth;
			vertex.EndWidth = placeholder.EndWidth;
			vertex.Bulge = placeholder.Bulge;
			vertex.Flags = placeholder.Flags;
			vertex.CurveTangent = placeholder.CurveTangent;
			vertex.Id = placeholder.Id;

			this.CadObject = vertex;
		}

		public class VertexPlaceholder : Vertex
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }
		}
	}
}
