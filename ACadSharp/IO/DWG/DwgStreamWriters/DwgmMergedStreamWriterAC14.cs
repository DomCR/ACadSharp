using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgmMergedStreamWriterAC14 : DwgmMergedStreamWriter, IDwgStreamWriter
	{
		public DwgmMergedStreamWriterAC14(Stream stream, IDwgStreamWriter main, IDwgStreamWriter textwriter)
			: base(stream, main, textwriter, main)
		{
		}

		public override void WriteSpearShift()
		{
			if (this._savedPosition)
			{
				this.Main.WriteSpearShift();
				this.Main.SetPositionInBits(this.PositionInBits);
				this.Main.WriteRawLong(this.Main.PositionInBits);
				this.Main.WriteShiftValue();
				this.Main.SetPositionInBits(this.Main.PositionInBits);
			}

			this.HandleWriter.WriteSpearShift();
			this.Main.WriteBytes(((MemoryStream)this.HandleWriter.Stream).GetBuffer());
			this.Main.WriteSpearShift();
		}
	}
}
