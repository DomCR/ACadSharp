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

		protected void writeCommonObjectData(CadObject cadObject)
		{
			this._writer.Write(DxfCode.Handle, cadObject.Handle);
			this._writer.Write(DxfCode.SoftPointerId, cadObject.Owner.Handle);

			//TODO: Write exended data and dictionary
			if (cadObject.ExtendedData != null)
			{
				//this._writer.Write(DxfCode.ControlString, "{ACAD_REACTORS");
				//this._writer.Write(DxfCode.HardOwnershipId, cadObject.ExtendedData);
				//this._writer.Write(DxfCode.ControlString, "}");
			}

			if (cadObject.Dictionary != null)
			{
				//this._writer.Write(DxfCode.ControlString, "{ACAD_XDICTIONARY");
				//this._writer.Write(DxfCode.HardOwnershipId, cadObject.Dictionary.Handle);
				//this._writer.Write(DxfCode.ControlString, "}");
			}
		}

		protected abstract void writeSection();
	}
}
