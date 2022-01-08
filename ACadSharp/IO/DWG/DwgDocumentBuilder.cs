using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder
	{
		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public List<DwgBlockRecordTemplate> BlockRecordTemplates { get; set; } = new List<DwgBlockRecordTemplate>();

		public DwgDocumentBuilder(CadDocument document, NotificationEventHandler notification = null)
			: base(document, notification)
		{
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
	}
}
