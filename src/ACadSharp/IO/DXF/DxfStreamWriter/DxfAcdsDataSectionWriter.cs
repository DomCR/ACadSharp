using ACadSharp.Entities;
using ACadSharp.IO.DXF.DxfStreamWriter;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DXF;

/// <summary>
/// Writes the ACDSDATA section of an R2013+ DXF: the ACIS payload of every modeler geometry entity
/// in the document, keyed by the entity's handle.
/// </summary>
/// <remarks>
/// From R2013 on AutoCAD moved the geometry out of the entity, which is why a region written
/// without this section is a handle with no shape. The schema block below is fixed boilerplate,
/// copied group for group from AutoCAD's own export of a production drawing; only the records that
/// follow it carry anything of this document.
/// </remarks>
internal class DxfAcdsDataSectionWriter : DxfSectionWriterBase
{
	public override string SectionName { get { return DxfFileToken.AcdsDataSection; } }

	private readonly IList<ModelerGeometry> _entities;

	public DxfAcdsDataSectionWriter(
		IDxfStreamWriter writer,
		CadDocument document,
		CadObjectHolder holder,
		DxfWriterConfiguration configuration,
		IList<ModelerGeometry> entities)
		: base(writer, document, holder, configuration)
	{
		this._entities = entities;
	}

	/// <summary>
	/// The modeler geometry entities of a document that have a payload to write, in handle order.
	/// </summary>
	/// <remarks>
	/// Regions only: a record here names the entity that owns it, and the other modeler geometry
	/// types are not written to the entities section at all - a record for one of those would point
	/// at a handle the file does not contain.
	/// </remarks>
	public static IList<ModelerGeometry> CollectEntities(CadDocument document)
	{
		var found = new List<ModelerGeometry>();
		foreach (BlockRecord record in document.BlockRecords)
		{
			foreach (Region region in record.Entities.OfType<Region>())
			{
				if (region.AcisData != null
					&& region.AcisData.Length > 0
					&& region.IsValid(CadFileFormat.DXF, document.Header.Version))
				{
					found.Add(region);
				}
			}
		}

		return found.OrderBy(e => e.Handle).ToList();
	}

	protected override void writeSection()
	{
		this._writer.Write(70, 2);
		this._writer.Write(71, 8);

		this.writeSchemas();

		foreach (ModelerGeometry geometry in this._entities)
		{
			this._writer.Write(0, DxfFileToken.AcdsRecord);
			this._writer.Write(90, 1);
			this._writer.Write(2, "AcDbDs::ID");
			this._writer.Write(280, 10);
			this._writer.Write(320, geometry.Handle);
			this._writer.Write(2, "ASM_Data");
			this._writer.Write(280, 15);
			this._writer.Write(94, geometry.AcisData.Length);

			//127 bytes to a line, which is the 254 hex characters AutoCAD writes.
			for (int i = 0; i < geometry.AcisData.Length; i += 127)
			{
				byte[] chunk = new byte[Math.Min(127, geometry.AcisData.Length - i)];
				Array.Copy(geometry.AcisData, i, chunk, 0, chunk.Length);
				this._writer.Write(310, chunk);
			}
		}
	}

	private void writeSchemas()
	{
		//Schema 0 - the thumbnail. No thumbnail record is written; the schema is kept because
		//AutoCAD numbers the ASM schema 1 and a file with only schema 1 is not what it writes.
		this._writer.Write(0, DxfFileToken.AcdsSchema);
		this._writer.Write(90, 0);
		this._writer.Write(1, "AcDb_Thumbnail_Schema");
		this._writer.Write(2, "AcDbDs::ID");
		this._writer.Write(280, 10);
		this._writer.Write(91, 8);
		this._writer.Write(2, "Thumbnail_Data");
		this._writer.Write(280, 15);
		this._writer.Write(91, 0);
		this.writeSchemaProperties();

		//Schema 1 - the one the records above refer to.
		this._writer.Write(0, DxfFileToken.AcdsSchema);
		this._writer.Write(90, 1);
		this._writer.Write(1, "AcDb3DSolid_ASM_Data");
		this._writer.Write(2, "AcDbDs::ID");
		this._writer.Write(280, 10);
		this._writer.Write(91, 8);
		this._writer.Write(2, "ASM_Data");
		this._writer.Write(280, 15);
		this._writer.Write(91, 0);
		this.writeSchemaProperties();

		this.writeAttributeSchema(2, "AcDbDs::TreatedAsObjectDataSchema", "AcDbDs::TreatedAsObjectData", 1, 0);
		this.writeAttributeSchema(3, "AcDbDs::LegacySchema", "AcDbDs::Legacy", 1, 0);
		this.writeAttributeSchema(4, "AcDbDs::IndexedPropertySchema", "AcDs:Indexable", 1, 0);
		this.writeAttributeSchema(5, "AcDbDs::HandleAttributeSchema", "AcDbDs::HandleAttribute", 7, 1);
	}

	private void writeAttributeSchema(int id, string schemaName, string propertyName, int type, int extra)
	{
		this._writer.Write(0, DxfFileToken.AcdsSchema);
		this._writer.Write(90, id);
		this._writer.Write(1, schemaName);
		this._writer.Write(2, propertyName);
		this._writer.Write(280, type);
		this._writer.Write(91, extra);
		if (extra == 1)
		{
			this._writer.Write(284, 1);
		}
	}

	private void writeSchemaProperties()
	{
		//The four properties every schema of AutoCAD's carries, in its order.
		this._writer.Write(101, DxfFileToken.AcdsRecord);
		this._writer.Write(95, 0);
		this._writer.Write(90, 2);
		this._writer.Write(2, "AcDbDs::TreatedAsObjectData");
		this._writer.Write(280, 1);
		this._writer.Write(291, 1);

		this._writer.Write(101, DxfFileToken.AcdsRecord);
		this._writer.Write(95, 0);
		this._writer.Write(90, 3);
		this._writer.Write(2, "AcDbDs::Legacy");
		this._writer.Write(280, 1);
		this._writer.Write(291, 1);

		this._writer.Write(101, DxfFileToken.AcdsRecord);
		this._writer.Write(1, "AcDbDs::ID");
		this._writer.Write(90, 4);
		this._writer.Write(2, "AcDs:Indexable");
		this._writer.Write(280, 1);
		this._writer.Write(291, 1);

		this._writer.Write(101, DxfFileToken.AcdsRecord);
		this._writer.Write(1, "AcDbDs::ID");
		this._writer.Write(90, 5);
		this._writer.Write(2, "AcDbDs::HandleAttribute");
		this._writer.Write(280, 7);
		this._writer.Write(282, 1);
	}
}
