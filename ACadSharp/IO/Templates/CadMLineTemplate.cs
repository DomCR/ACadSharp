using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadMLineTemplate : CadEntityTemplate
	{
		public ulong? MLineStyleHandle { get; set; }

		private List<int> _readedCodes = new List<int>();

		private MLine.Vertex _currVertex;

		private MLine.Vertex.Segment _currSegmentElement;

		public CadMLineTemplate(MLine mline) : base(mline) { }

		public override bool AddHandle(int dxfcode, ulong handle)
		{
			bool value = base.AddHandle(dxfcode, handle);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 340:
					this.MLineStyleHandle = handle;
					value = true;
					break;
			}

			return value;
		}

		public override bool AddName(int dxfcode, string name)
		{
			bool value = base.AddName(dxfcode, name);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 2:
					return true;
			}

			return value;
		}

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return found;


			var mline = this.CadObject as MLine;

			if (_currVertex == null)
			{
				//Make sure to work with none null value
				_currVertex = new MLine.Vertex();
			}

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
					_currSegmentElement.Parameters.Add((double)value);
					return true;
				case 42:
					_currSegmentElement.AreaFillParameters.Add((double)value);
					return true;
				case 73:
				case 75:
					return true;
			}

			return found;
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
