using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgmMergedStreamWriterAC14 : DwgmMergedStreamWriter, IDwgStreamWriter
	{
		public DwgmMergedStreamWriterAC14(Stream stream, IDwgStreamWriter main, IDwgStreamWriter handle)
			: base(stream, main, main, handle)
		{
		}

		public override void WriteSpearShift()
		{
			int pos = (int)this.Main.PositionInBits;

			if (this._savedPosition)
			{
				this.Main.WriteSpearShift();
				this.Main.SetPositionInBits(this.PositionInBits);
				this.Main.WriteRawLong(pos);
				this.Main.WriteShiftValue();
				this.Main.SetPositionInBits(pos);
			}

			this.HandleWriter.WriteSpearShift();
			this.Main.WriteBytes(((MemoryStream)this.HandleWriter.Stream).GetBuffer());
			this.Main.WriteSpearShift();
		}
	}
}
