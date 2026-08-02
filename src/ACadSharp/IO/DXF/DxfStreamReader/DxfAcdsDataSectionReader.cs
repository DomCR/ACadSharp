using ACadSharp.DataStorage;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO.DXF.DxfStreamReader;

internal class DxfAcdsDataSectionReader : DxfSectionReaderBase
{
	public const string SchemaToken = "ACDSSCHEMA";

	public const string RecordToken = "ACDSRECORD";

	private List<Schema> _schemas = new List<Schema>();

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
						var s = this.readAcdsSchema();
						this._schemas.Add(s);
						continue;
					case RecordToken:
						this.readAcdsRecord();
						continue;
				}

				this._reader.ReadNext();
			}
		}
		catch (Exception ex)
		{
			this._builder.Notify("An error occurred while reading the ACDSDATA", NotificationType.Error, ex);
		}
	}

	private void readAcdsRecord()
	{
		throw new NotImplementedException();
	}

	private Schema readAcdsSchema()
	{
		Schema schema = new Schema();

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
				case 101:
					var propertyDescriptor = this.readPropertyDescriptor();
					schema.PropertyDescriptors.Add(propertyDescriptor);
					continue;
			}

			this._reader.ReadNext();
		}

		return schema;
	}

	private AcdsPropertyDescriptor readPropertyDescriptor()
	{
		throw new NotImplementedException();
	}

	private SchemaProperty readProperty()
	{
		var property = new SchemaProperty();
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
					property.PropertyFlags = (SchemaPropertyFlags)this._reader.ValueAsShort;
					break;
			}

			this._reader.ReadNext();
		}

		return property;
	}
}
