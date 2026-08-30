using ACadSharp.Entities;
using CSMath;
using CSUtilities.IO;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadOle2FrameTemplate : CadEntityTemplate<Ole2Frame>
	{
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
				this.CadObject.BinaryData = this.Chunks.SelectMany(c => c).ToArray();
			}

			//The four corners are recovered from the OLE2 payload itself, so with no payload there is
			//nothing to recover - and AutoCAD's own DXF export of an OLE2FRAME does not always carry
			//one. Reading on from a null buffer threw ArgumentNullException out of BuildDocument,
			//which loses the ENTIRE document over one frame: a drawing of 32,571 entities carrying
			//twenty OLE frames could not be read back from AutoCAD's DXF at all.
			//
			//Two bytes of header and four points of three doubles is what the block below consumes.
			const int required = 2 + (4 * 3 * 8);
			if (this.CadObject.BinaryData == null || this.CadObject.BinaryData.Length < required)
			{
				builder.Notify($"OLE2FRAME {this.CadObject.Handle} carries no readable OLE2 payload, its corners are left at their defaults", NotificationType.Warning);
				return;
			}

			StreamIO reader = new StreamIO(CadObject.BinaryData);

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