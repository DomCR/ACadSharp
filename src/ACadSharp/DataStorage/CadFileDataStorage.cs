using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public List<AcdsRecord> Records { get; } = new();

	public List<AcdsSchema> Schemes { get; } = new();

	public const string AcDb3DSolid_ASM_Data = "AcDb3DSolid_ASM_Data";

	public const string AcDbDsId = "AcDbDs::ID";

	public const string AsmData = "ASM_Data";

	public const string HandleAttribute = "AcDbDs::HandleAttribute";

	public const string HandleAttributeSchemaName = "AcDbDs::HandleAttributeSchema";

	public const string Indexable = "AcDs:Indexable";

	public const string IndexedPropertySchemaName = "AcDbDs::IndexedPropertySchema";

	public const string Legacy = "AcDbDs::Legacy";

	public const string LegacySchemaName = "AcDbDs::LegacySchema";

	public const string ThumbnailData = "Thumbnail_Data";

	public const string ThumbnailSchema = "AcDb_Thumbnail_Schema";

	public const string TreatedAsObjectData = "AcDbDs::TreatedAsObjectData";

	public const string TreatedAsObjectDataSchemaName = "AcDbDs::TreatedAsObjectDataSchema";

	public CadFileDataStorage()
	{
	}

	public CadFileDataStorage(IEnumerable<ModelerGeometry> geometries)
	{
		this.init(geometries);
	}

	private void buildAcDb3DSolid_ASM_Data(uint index)
	{
		//{5 : AcDb3DSolid_ASM_Data}
		AcdsSchema schema = new AcdsSchema
		{
			Name = AcDb3DSolid_ASM_Data,
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = AcDbDsId,
			Type = 10,
			PropertyFlags = AcdsSchemaPropertyFlags.Unknown2,
		});
		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = AsmData,
			Type = 15,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		this.Schemes.Add(schema);
	}

	private void buildHandleAttributeSchema(uint index)
	{
		//{4 : AcDbDs::HandleAttributeSchema}
		AcdsSchema schema = new AcdsSchema
		{
			Name = HandleAttributeSchemaName,
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = HandleAttribute,
			Type = 1,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		this.Schemes.Add(schema);
	}

	private void buildIndexedPropertySchema(uint index)
	{
		//{3 : AcDbDs::IndexedPropertySchema}
		AcdsSchema schema = new AcdsSchema
		{
			Name = IndexedPropertySchemaName,
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = Indexable,
			Type = 1,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		this.Schemes.Add(schema);
	}

	private void buildLegacySchema(uint index)
	{
		//{2 : AcDbDs::LegacySchema}
		AcdsSchema schema = new AcdsSchema
		{
			Name = LegacySchemaName,
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = Legacy,
			Type = 1,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		this.Schemes.Add(schema);
	}

	private void buildRecords(uint index, IEnumerable<ModelerGeometry> geometries)
	{
		foreach (var item in geometries)
		{
			AcdsRecord record = new AcdsRecord();
			record.Index = index;

			record.Columns.Add(AcDbDsId, new AcdsRecordColumn
			{
				Name = AcDbDsId,
				DataType = 10,
				Handle = item.Handle,
			});

			record.Columns.Add(AsmData, new AcdsRecordColumn
			{
				Name = AsmData,
				DataType = 15,
				Data = new System.IO.MemoryStream(item.AcisData),
			});

			this.Records.Add(record);
		}
	}

	private void buildThumbnailSchema(uint index)
	{
		//{0 : AcDb_Thumbnail_Schema}
		AcdsSchema thumbnail = new AcdsSchema
		{
			Name = ThumbnailSchema,
			Index = index
		};

		thumbnail.Properties.Add(new AcdsSchemaProperty
		{
			Name = AcDbDsId,
			Type = 10,
			PropertyFlags = AcdsSchemaPropertyFlags.Unknown2,
		});
		thumbnail.Properties.Add(new AcdsSchemaProperty
		{
			Name = ThumbnailData,
			Type = 15,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		thumbnail.Properties.Add(new AcdsSchemaProperty
		{
			Name = TreatedAsObjectData,
			Type = 1,
			PropertyFlags = AcdsSchemaPropertyFlags.Unknown1,
		});

		this.Schemes.Add(thumbnail);
	}

	private void buildTreatedAsObjectDataSchema(uint index)
	{
		//{1 : AcDbDs::TreatedAsObjectDataSchema}
		AcdsSchema schema = new AcdsSchema
		{
			Name = TreatedAsObjectDataSchemaName,
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = TreatedAsObjectData,
			Type = 1,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		this.Schemes.Add(schema);
	}

	private void init(IEnumerable<ModelerGeometry> geometries)
	{
		uint index = 0;
		this.buildThumbnailSchema(index++);
		this.buildTreatedAsObjectDataSchema(index++);
		this.buildLegacySchema(index++);
		this.buildIndexedPropertySchema(index++);
		this.buildHandleAttributeSchema(index++);
		this.buildAcDb3DSolid_ASM_Data(index);

		this.buildRecords(index, geometries);
	}
}