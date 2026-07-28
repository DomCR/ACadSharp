using System;
using System.IO;

namespace ACadSharp.IO.DXF.DxfStreamReader;

internal class DxfAcdsDataSectionReader : DxfSectionReaderBase
{
	public DxfAcdsDataSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder)
		: base(reader, builder)
	{
	}

	public override void Read()
	{
		try
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//The section contains ACDSSCHEMA entries (schema definitions, skipped) and
			//ACDSRECORD entries. A data record associates an owner entity with its
			//ASM/ACIS payload:
			//  2   AcDbDs::ID
			//  320 <owner entity handle>
			//  2   ASM_Data
			//  94  <payload length in bytes>
			//  310 <hex chunks with the binary payload>
			//Schema definitions also nest records introduced by group code 101; those
			//carry no 320/310 pairs so the accumulation below ignores them naturally.
			bool inRecord = false;
			ulong currentHandle = 0;
			MemoryStream payload = null;

			while (this._reader.ValueAsString != DxfFileToken.EndSection)
			{
				if (this._reader.DxfCode == DxfCode.Start)
				{
					this.storeCurrentRecord(currentHandle, payload);

					inRecord = this._reader.ValueAsString == DxfFileToken.AcdsRecord;
					currentHandle = 0;
					payload = null;
				}
				else if (inRecord)
				{
					switch (this._reader.Code)
					{
						case 320:
							currentHandle = this._reader.ValueAsHandle;
							break;
						case 310:
							payload = payload ?? new MemoryStream();
							byte[] chunk = this._reader.ValueAsBinaryChunk;
							payload.Write(chunk, 0, chunk.Length);
							break;
					}
				}

				this._reader.ReadNext();
			}

			this.storeCurrentRecord(currentHandle, payload);
		}
		catch (Exception ex)
		{
			this._builder.Notify("An error occurred while reading the Prototype1b", NotificationType.Error, ex);
		}
	}

	private void storeCurrentRecord(ulong handle, MemoryStream payload)
	{
		if (handle == 0 || payload == null || payload.Length == 0)
		{
			return;
		}

		this._builder.AcdsDataRecords[handle] = payload.ToArray();
	}
}
