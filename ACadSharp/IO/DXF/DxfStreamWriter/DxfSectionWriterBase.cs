using System.Collections.Generic;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfSectionWriterBase
	{
		public abstract string SectionName { get; }

		protected IDxfStreamWriter _writer;
		protected CadDocument _document;

		public DxfSectionWriterBase(
			IDxfStreamWriter writer,
			CadDocument document)
		{
			this._writer = writer;
			this._document = document;
		}

		public void Write()
		{
			this._writer.Write(DxfCode.Start, DxfFileToken.BeginSection);
			this._writer.Write(DxfCode.SymbolTableName, this.SectionName);

			this.writeSection();

			this._writer.Write(DxfCode.Start, DxfFileToken.EndSection);
		}

		protected abstract void writeSection();
	}
}
