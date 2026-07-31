using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACadSharp.Entities;

namespace ACadSharp.IO.DXF
{
	internal class DxfAcdsDataSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.AcdsDataSection; } }

		public DxfAcdsDataSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder objectHolder, DxfWriterConfiguration configuration)
			: base(writer, document, objectHolder, configuration) { }

		/// <summary>
		/// The modeler geometry entities whose binary ACIS payload belongs to the
		/// section, in every block record of the document.
		/// </summary>
		public static IEnumerable<ModelerGeometry> GetPayloadEntities(CadDocument document)
		{
			return document.BlockRecords
				.SelectMany(record => record.Entities)
				.OfType<ModelerGeometry>()
				.Where(geometry => geometry.AcisData != null
					&& geometry.AcisData.Length > 0
					&& geometry.IsBinaryAcisData);
		}

		protected override void writeSection()
		{
			this._writer.Write(70, (short)2);
			this._writer.Write(71, (short)2);

			this.writeSchemas();

			foreach (ModelerGeometry geometry in GetPayloadEntities(this._document))
			{
				this.writeDataRecord(geometry);
			}
		}

		//The schema records describe the data layout to the consumers; the set
		//is the fixed boilerplate every writer emits for the ASM payloads.
		private void writeSchemas()
		{
			//thumbnail schema
			this.writeSchemaHeader(0, "AcDb_Thumbnail_Schema", "Thumbnail_Data");
			this.writeSchemaAttributes(0);

			//solid payload schema
			this.writeSchemaHeader(1, "AcDb3DSolid_ASM_Data", "ASM_Data");
			this.writeSchemaAttributes(1);

			this.writeMarkerSchema(2, "AcDbDs::TreatedAsObjectDataSchema", "AcDbDs::TreatedAsObjectData");
			this.writeMarkerSchema(3, "AcDbDs::LegacySchema", "AcDbDs::Legacy");
			this.writeMarkerSchema(4, "AcDbDs::IndexedPropertySchema", "AcDs:Indexable");

			//handle attribute schema carries the owner reference
			this._writer.Write(0, DxfFileToken.AcdsSchema);
			this._writer.Write(90, 5);
			this._writer.Write(1, "AcDbDs::HandleAttributeSchema");
			this._writer.Write(2, "AcDbDs::HandleAttribute");
			this._writer.Write(280, (short)7);
			this._writer.Write(91, 1);
			this._writer.Write(284, (short)1);
		}

		//Schema with an id and a payload member.
		private void writeSchemaHeader(int index, string name, string dataMember)
		{
			this._writer.Write(0, DxfFileToken.AcdsSchema);
			this._writer.Write(90, index);
			this._writer.Write(1, name);
			this._writer.Write(2, "AcDbDs::ID");
			this._writer.Write(280, (short)10);
			this._writer.Write(91, 8);
			this._writer.Write(2, dataMember);
			this._writer.Write(280, (short)15);
			this._writer.Write(91, 0);
		}

		//Attribute sub-records shared by the id/payload schemas.
		private void writeSchemaAttributes(int index)
		{
			this._writer.Write(101, DxfFileToken.AcdsRecord);
			this._writer.Write(95, index);
			this._writer.Write(90, 2);
			this._writer.Write(2, "AcDbDs::TreatedAsObjectData");
			this._writer.Write(280, (short)1);
			this._writer.Write(291, (short)1);

			this._writer.Write(101, DxfFileToken.AcdsRecord);
			this._writer.Write(95, index);
			this._writer.Write(90, 3);
			this._writer.Write(2, "AcDbDs::Legacy");
			this._writer.Write(280, (short)1);
			this._writer.Write(291, (short)1);

			this._writer.Write(101, DxfFileToken.AcdsRecord);
			this._writer.Write(1, "AcDbDs::ID");
			this._writer.Write(90, 4);
			this._writer.Write(2, "AcDs:Indexable");
			this._writer.Write(280, (short)1);
			this._writer.Write(291, (short)1);

			this._writer.Write(101, DxfFileToken.AcdsRecord);
			this._writer.Write(1, "AcDbDs::ID");
			this._writer.Write(90, 5);
			this._writer.Write(2, "AcDbDs::HandleAttribute");
			this._writer.Write(280, (short)7);
			this._writer.Write(282, (short)1);
		}

		//Schema with a single boolean member.
		private void writeMarkerSchema(int index, string name, string member)
		{
			this._writer.Write(0, DxfFileToken.AcdsSchema);
			this._writer.Write(90, index);
			this._writer.Write(1, name);
			this._writer.Write(2, member);
			this._writer.Write(280, (short)1);
			this._writer.Write(91, 0);
		}

		//Payload record: the owner handle at 320 pairs the blob with its entity.
		private void writeDataRecord(ModelerGeometry geometry)
		{
			this._writer.Write(0, DxfFileToken.AcdsRecord);
			this._writer.Write(90, 1);
			this._writer.Write(2, "AcDbDs::ID");
			this._writer.Write(280, (short)10);
			this._writer.Write(320, geometry.Handle);
			this._writer.Write(2, "ASM_Data");
			this._writer.Write(280, (short)15);
			this._writer.Write(94, geometry.AcisData.Length);

			//the stream writer hex-encodes the payload and splits it in rows
			this._writer.Write(310, geometry.AcisData);
		}
	}
}
