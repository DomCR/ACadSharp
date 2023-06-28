using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadMLineTemplate : CadEntityTemplate<MLine>
	{
		public ulong? MLineStyleHandle { get; set; }

		public string MLineStyleName { get; set; }

		public int? NVertex { get; set; }

		public int? NElements { get; set; }

		private MLine.Vertex _currVertex;

		private MLine.Vertex.Segment _currSegmentElement;

		public CadMLineTemplate() : base(new MLine()) { }

		public CadMLineTemplate(MLine mline) : base(mline) { }

		public bool TryReadVertex(int dxfcode, object value)
		{
			var mline = this.CadObject as MLine;

			switch (dxfcode)
			{
				case 11:
					this._currVertex = new MLine.Vertex();
					mline.Vertices.Add(_currVertex);
					_currVertex.Position = new CSMath.XYZ(
						(double)value,
						_currVertex.Position.Y,
						_currVertex.Position.Z
						);
					return true;
				case 21:
					_currVertex.Position = new CSMath.XYZ(
						_currVertex.Position.X,
						(double)value,
						_currVertex.Position.Z
						);
					return true;
				case 31:
					_currVertex.Position = new CSMath.XYZ(
						_currVertex.Position.X,
						_currVertex.Position.Y,
						(double)value
						);
					return true;
				case 12:
					_currVertex.Direction = new CSMath.XYZ(
						(double)value,
						_currVertex.Direction.Y,
						_currVertex.Direction.Z
						);
					return true;
				case 22:
					_currVertex.Direction = new CSMath.XYZ(
						_currVertex.Direction.X,
						(double)value,
						_currVertex.Direction.Z
						);
					return true;
				case 32:
					_currVertex.Direction = new CSMath.XYZ(
						_currVertex.Direction.X,
						_currVertex.Direction.Y,
						(double)value
						);
					return true;
				case 13:
					_currVertex.Miter = new CSMath.XYZ(
						(double)value,
						_currVertex.Miter.Y,
						_currVertex.Miter.Z
						);
					return true;
				case 23:
					_currVertex.Miter = new CSMath.XYZ(
						_currVertex.Miter.X,
						(double)value,
						_currVertex.Miter.Z
						);
					return true;
				case 33:
					_currVertex.Miter = new CSMath.XYZ(
						_currVertex.Miter.X,
						_currVertex.Miter.Y,
						(double)value
						);
					return true;
				case 74:
					this._currSegmentElement = new MLine.Vertex.Segment();
					_currVertex.Segments.Add(_currSegmentElement);
					return true;
				case 41:
					_currSegmentElement?.Parameters.Add((double)value);
					return true;
				case 42:
					_currSegmentElement?.AreaFillParameters.Add((double)value);
					return true;
				case 75:
					return true;
				default:
					return false;
			}
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			MLine mLine = this.CadObject as MLine;

			if (builder.TryGetCadObject<MLStyle>(this.MLineStyleHandle, out MLStyle style))
			{
				mLine.MLStyle = style;
			}
		}
	}
}
