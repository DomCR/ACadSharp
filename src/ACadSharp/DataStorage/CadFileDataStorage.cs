using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public List<AcdsRecord> Records { get; } = new();

	public List<AcdsSchema> Schemes { get; } = new();

	public const string AcDb3DSolid_ASM_Data = "AcDb3DSolid_ASM_Data";

	public const string AcDbDsIdName = "AcDbDs::ID";

	public const string AsmData = "ASM_Data";

	public const string ThumbnailSchemaName = "AcDb_Thumbnail_Schema";

	public CadFileDataStorage()
	{
	}

	public CadFileDataStorage(IEnumerable<ModelerGeometry> geometries)
	{
		this.build(geometries);
	}

	private void build(IEnumerable<ModelerGeometry> geometries)
	{
		uint index = 0;
		this.buildThumbnailSchema(index++);
		this.buildTreatedAsObjectDataSchema(index++);
		this.buildLegacySchema(index++);
		this.buildIndexedPropertySchema(index++);
		this.buildHandleAttributeSchema(index++);
		this.buildAcDb3DSolid_ASM_Data(index++);
	}

	private void buildAcDb3DSolid_ASM_Data(uint index)
	{
		//{5 : AcDb3DSolid_ASM_Data}
		AcdsSchema thumbnail = new AcdsSchema
		{
			Name = AcDb3DSolid_ASM_Data,
			Index = index
		};
	}

	private void buildHandleAttributeSchema(uint index)
	{
		//{4 : AcDbDs::HandleAttributeSchema}
		AcdsSchema schema = new AcdsSchema
		{
			Name = "AcDbDs::HandleAttributeSchema",
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = "AcDbDs::HandleAttribute",
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
			Name = "AcDbDs::IndexedPropertySchema",
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = "AcDs:Indexable",
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
			Name = "AcDbDs::LegacySchema",
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = "AcDbDs::Legacy",
			Type = 1,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		this.Schemes.Add(schema);
	}

	private void buildThumbnailSchema(uint index)
	{
		//{0 : AcDb_Thumbnail_Schema}
		AcdsSchema thumbnail = new AcdsSchema
		{
			Name = ThumbnailSchemaName,
			Index = index
		};

		thumbnail.Properties.Add(new AcdsSchemaProperty
		{
			Name = AcDbDsIdName,
			Type = 10,
			PropertyFlags = AcdsSchemaPropertyFlags.Unknown2,
		});
		thumbnail.Properties.Add(new AcdsSchemaProperty
		{
			Name = "Thumbnail_Data",
			Type = 15,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		thumbnail.Properties.Add(new AcdsSchemaProperty
		{
			Name = "AcDbDs::TreatedAsObjectData",
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
			Name = "AcDbDs::TreatedAsObjectDataSchema",
			Index = index
		};

		schema.Properties.Add(new AcdsSchemaProperty
		{
			Name = "AcDbDs::TreatedAsObjectData",
			Type = 1,
			PropertyFlags = AcdsSchemaPropertyFlags.None,
		});

		this.Schemes.Add(schema);
	}
}