using ACadSharp.DataStorage;
using System;

namespace ACadSharp.IO.DXF.DxfStreamWriter;

internal class DxfAcdsDataSectionWriter : DxfSectionWriterBase
{
	public override string SectionName { get { return DxfFileToken.AcdsDataSection; } }

	private CadFileDataStorage _dataStorage;

	public DxfAcdsDataSectionWriter(CadFileDataStorage dataStorage, IDxfStreamWriter writer, CadDocument document, CadObjectHolder objectHolder, DxfWriterConfiguration configuration)
		: base(writer, document, objectHolder, configuration)
	{
		this._dataStorage = dataStorage;
	}

	protected override void writeSection()
	{
		this._writer.Write(70, (short)2);
		this._writer.Write(71, (short)8);

		foreach (AcdsSchema schema in _dataStorage.Schemes)
		{
			this.writeAcdsSchema(schema);
		}

		foreach (AcdsRecord record in this._dataStorage.Records)
		{
			this.writeAcdsRecord(record);
		}
	}

	private void writeAcdsRecord(AcdsRecord record)
	{
		this._writer.Write(0, DxfFileToken.AcdsRecord);
		this._writer.Write(90, record.Index);
		foreach (AcdsRecordColumn col in record.Columns.Values)
		{
			this._writer.Write(2, col.Name);
			this._writer.Write(280, col.DataType);

			switch (col.DataType)
			{
				case 10:
					this._writer.Write(320, col.Handle);
					break;
				case 15:
					this._writer.Write(94, col.Data.Length);
					this._writer.Write(310, col.Data.ToArray());
					break;
			}
		}
	}

	private void writeAcdsSchema(AcdsSchema schema)
	{
		this._writer.Write(0, DxfFileToken.AcdsSchema);
		this._writer.Write(90, schema.Index);
		this._writer.Write(1, schema.Name);

		foreach (AcdsSchemaProperty prop in schema.Properties)
		{
			this.writeAcdsSchemaProperty(prop);
		}

		foreach (AcdsSchemaRecord record in schema.EmbeddedRecords)
		{
			this.writeAcdsSchemaRecord(record);
		}
	}

	private void writeAcdsSchemaRecord(AcdsSchemaRecord record)
	{
		this._writer.Write(101, DxfFileToken.AcdsRecord);

		throw new NotImplementedException();
	}

	private void writeAcdsSchemaProperty(AcdsSchemaProperty prop)
	{
		this._writer.Write(2, prop.Name);
		this._writer.Write(280, prop.Type);
		this._writer.Write(91, prop.PropertyFlags);
	}
}