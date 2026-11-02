using ACadSharp.Entities;
using CSMath;
using CSUtilities.Converters;
using CSUtilities.IO;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates;

internal class CadOle2FrameTemplate : CadEntityTemplate<Ole2Frame>
{
	public List<byte[]> Chunks { get; set; } = new();

	public CadOle2FrameTemplate(Ole2Frame ole) : base(ole)
	{
	}

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

		if (this.CadObject.BinaryData == null || this.CadObject.BinaryData.Length == 0)
		{
			return;
		}

		StreamIO reader = new StreamIO(this.CadObject.BinaryData);
		reader.EndianConverter = new LittleEndianConverter();

		try
		{
			//section that follows.
			//Unknown data ---The OLE2 data.
			//– Signature: 0x5580 | kPaperSpace (kPaperSpace = 0x01)
			var unknown0 = reader.ReadByte();
			var unknown1 = reader.ReadByte();

			this.CadObject.UpperLeftCorner = this.read3Double(reader);
			var upperRight = this.read3Double(reader);
			this.CadObject.LowerRightCorner = this.read3Double(reader);
			//Expected position = 5,5,0
			var lowerLeft = this.read3Double(reader);

			var unknown2 = reader.ReadShort();  //2
			var unknown3 = reader.ReadShort();  //2

			var unknown4 = reader.ReadLong();   //4

			var version = reader.ReadLong();    //4
			var itemId = reader.ReadLong();    //4
			var type = reader.ReadLong();   //4
			var mon = reader.ReadShort() != 0;    //2
			var aspect = reader.ReadLong();   //4

			var length = reader.ReadLong();
			this.CadObject.OleData = reader.ReadBytes((int)length);
		}
		catch (System.Exception)
		{
			builder.Notify($"Error reading OLE2 Frame data for handle {this.CadObject.Handle}", NotificationType.Warning);
		}
	}

	private XYZ read3Double(StreamIO reader)
	{
		var x = reader.ReadDouble();
		var y = reader.ReadDouble();
		var z = reader.ReadDouble();

		return new XYZ(x, y, z);
	}
}