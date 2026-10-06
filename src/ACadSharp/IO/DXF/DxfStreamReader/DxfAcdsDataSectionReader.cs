using ACadSharp.DataStorage;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO.DXF.DxfStreamReader;

internal class DxfAcdsDataSectionReader : DxfSectionReaderBase
{
	public const string RecordToken = "ACDSRECORD";

	public const string SchemaToken = "ACDSSCHEMA";

	public DxfAcdsDataSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder)
		: base(reader, builder)
	{
	}

	public override void Read()
	{
		this._builder.DataStorage = new CadFileDataStorage();

		try
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			while (this._reader.ValueAsString != DxfFileToken.EndSection)
			{
				if (this._reader.DxfCode != DxfCode.Start)
				{
					//codes 70 and 71, possible versions?
					this._reader.ReadNext();
					continue;
				}

				switch (this._reader.ValueAsString.ToUpper())
				{
					case SchemaToken:
						this._builder.DataStorage.Schemes.Add(this.readAcdsSchema());
						continue;
					case RecordToken:
						this._builder.DataStorage.Records.Add(this.readAcdsRecord());
						continue;
				}

				this._reader.ReadNext();
			}
		}
		catch (Exception ex)
		{
			this._builder.DataStorage = null;
			this._builder.Notify("An error occurred while reading the ACDSDATA", NotificationType.Error, ex);
		}
	}

	private AcdsRecord readAcdsRecord()
	{
		AcdsRecord record = new();

		this._reader.ReadNext();

		while (this._reader.DxfCode != DxfCode.Start
			&& this._reader.DxfCode != DxfCode.EmbeddedObjectStart)
		{
			switch (this._reader.Code)
			{
				case 2:
					AcdsRecordColumn column = this.readAcdsRecordColumn();
					if (!record.Columns.ContainsKey(column.Name))
					{
						record.Columns.Add(column.Name, column);
					}
					else
					{
						this._builder.Notify($"Duplicate column name '{column.Name}' found in ACDS record.", NotificationType.Warning);
					}
					continue;
				case 90:
					record.Index = (uint)this._reader.ValueAsInt;
					break;
			}

			this._reader.ReadNext();
		}

		return record;
	}

	private AcdsRecordColumn readAcdsRecordColumn()
	{
		AcdsRecordColumn column = new();
		column.Name = this._reader.ValueAsString;
		MemoryStream binaryData = null;

		this._reader.ReadNext();

		while (this._reader.DxfCode != DxfCode.Start
			&& this._reader.DxfCode != DxfCode.Name
			&& this._reader.DxfCode != DxfCode.EmbeddedObjectStart)
		{
			switch (this._reader.Code)
			{
				case 280:
					column.DataType = this._reader.ValueAsShort;
					break;
				case 310:
					binaryData ??= new();
					binaryData.Write(this._reader.ValueAsBinaryChunk, 0, this._reader.ValueAsBinaryChunk.Length);
					break;
				default:
					column.CodeValuePair = new KeyValuePair<int, object>(this._reader.Code, this._reader.Value);
					break;
			}

			this._reader.ReadNext();
		}

		if (binaryData != null)
		{
			column.CodeValuePair = new KeyValuePair<int, object>(310, binaryData.ToArray());
		}

		return column;
	}

	private AcdsSchema readAcdsSchema()
	{
		AcdsSchema schema = new AcdsSchema();

		this._reader.ReadNext();

		while (this._reader.DxfCode != DxfCode.Start)
		{
			switch (this._reader.Code)
			{
				case 1:
					schema.Name = this._reader.ValueAsString;
					break;
				case 2:
					var property = this.readProperty();
					schema.Properties.Add(property);
					continue;
				case 90:
					schema.Index = (uint)this._reader.ValueAsInt;
					break;
				case 101 when this._reader.ValueAsString == RecordToken:
					var record = this.readEmbeddedRecord();
					schema.EmbeddedRecords.Add(record);
					continue;
			}

			this._reader.ReadNext();
		}

		return schema;
	}

	private AcdsSchemaRecord readEmbeddedRecord()
	{
		var record = new AcdsSchemaRecord();

		this._reader.ReadNext();

		while (this._reader.DxfCode != DxfCode.Start
			&& this._reader.DxfCode != DxfCode.EmbeddedObjectStart)
		{
			switch (this._reader.Code)
			{
				case 2:
					record.Name = this._reader.ValueAsString;
					break;
				case 90:
					record.Index = (int)this._reader.ValueAsInt;
					break;
				case 91:
					record.Flags = (SchemaRecordFlags)this._reader.ValueAsInt;
					break;
				case 95:
					record.Id = (int)this._reader.ValueAsShort;
					break;
				case 280:
					record.Type = (uint)this._reader.ValueAsInt;
					break;
				case 291:
					record.Value291 = this._reader.ValueAsShort;
					break;
			}

			this._reader.ReadNext();
		}

		return record;
	}

	private AcdsSchemaProperty readProperty()
	{
		var property = new AcdsSchemaProperty();
		property.Name = this._reader.ValueAsString;

		while (this._reader.DxfCode != DxfCode.Start
			&& this._reader.DxfCode != DxfCode.EmbeddedObjectStart)
		{
			switch (this._reader.Code)
			{
				case 91:
					property.Type = (byte)this._reader.ValueAsShort;
					break;
				case 280:
					property.PropertyFlags = (AcdsSchemaPropertyFlags)this._reader.ValueAsShort;
					break;
			}

			this._reader.ReadNext();
		}

		return property;
	}
}