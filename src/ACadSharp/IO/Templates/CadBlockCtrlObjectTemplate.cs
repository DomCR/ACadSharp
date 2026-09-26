using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockCtrlObjectTemplate : CadTableTemplate<BlockRecord>
	{
		public ulong? ModelSpaceHandle { get; set; }

		public ulong? PaperSpaceHandle { get; set; }

		public CadBlockCtrlObjectTemplate(BlockRecordsTable blocks) : base(blocks) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			this.registerReservedSpace(builder, this.ModelSpaceHandle, BlockRecord.ModelSpaceName);
			this.registerReservedSpace(builder, this.PaperSpaceHandle, BlockRecord.PaperSpaceName);
		}

		//The block control object references *Model_Space/*Paper_Space by handle,
		//separated from the rest of the entries. In malformed files (e.g. produced
		//by some third-party DXF->DWG converters) the same record can also appear
		//in the entry list, or carry a non-canonical name. The handle is authoritative:
		//resolve it, normalize to the canonical reserved name, and add only if absent
		//so a duplicate never aborts the read.
		private void registerReservedSpace(CadDocumentBuilder builder, ulong? handle, string canonicalName)
		{
			if (!builder.TryGetCadObject<BlockRecord>(handle, out BlockRecord record))
			{
				return;
			}

			if (this.CadObject.Contains(canonicalName))
			{
				builder.Notify($"[{this.CadObject.SubclassMarker}] duplicated reference to the reserved block record {canonicalName} ignored", NotificationType.Warning);
				return;
			}

			//Only rename a record that is not yet owned by a table, renaming an owned
			//entry re-keys the table and throws if the name is a default entry
			if (!string.Equals(record.Name, canonicalName, StringComparison.OrdinalIgnoreCase)
				&& record.Owner == null)
			{
				record.Name = canonicalName;
			}

			try
			{
				this.CadObject.Add(record);
			}
			catch (ArgumentException ex)
			{
				builder.Notify($"[{this.CadObject.SubclassMarker}] the reserved block record {record.Name} already exists", NotificationType.Error, ex);
			}
		}
	}
}
