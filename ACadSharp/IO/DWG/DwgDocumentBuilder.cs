using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder
	{
		public DwgReaderFlags Flags { get; set; }

		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new List<CadBlockRecordTemplate>();

		public DwgDocumentBuilder(CadDocument document, DwgReaderFlags flags)
			: base(document)
		{
			this.Flags = flags;
		}

		public override void BuildDocument()
		{
			//Set the names for the block records before add them to the table
			foreach (var item in this.BlockRecordTemplates)
			{
				item.SetBlockToRecord(this);
			}

			base.BuildDocument();
		}

		protected CadTableTemplate<T> getTableTemplate<T>()
			where T : TableEntry
		{
			return templates.Values.OfType<CadTableTemplate<T>>().FirstOrDefault();
		}
	}
}
