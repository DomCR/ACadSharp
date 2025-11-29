using ACadSharp.Entities;
using CSMath;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadOle2FrameTemplate : CadEntityTemplate<Ole2Frame>
	{
		public byte[] Data { get; set; }

		public List<byte[]> Chunks { get; set; } = new();

		public CadOle2FrameTemplate(Ole2Frame ole) : base(ole) { }

		public CadOle2FrameTemplate() : base(new Ole2Frame())
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.Chunks.Any())
			{
				this.Data = this.Chunks.SelectMany(c => c).ToArray();
			}

			var diff = Data.Length - 549504;
			StreamIO reader = new StreamIO(Data);

			//section that follows.
			//Unknown data ---The OLE2 data.
			reader.ReadByte();
			reader.ReadByte();

			this.CadObject.UpperLeftCorner = this.read3Double(reader);
			var upperRight = this.read3Double(reader);
			this.CadObject.LowerRightCorner = this.read3Double(reader);
			//Expected position = 5,5,0
			var lowerLeft = this.read3Double(reader);
		}

		private XYZ read3Double(StreamIO reader)
		{
			var x = reader.ReadDouble();
			var y = reader.ReadDouble();
			var z = reader.ReadDouble();

			return new XYZ(x, y, z);
		}
	}
}