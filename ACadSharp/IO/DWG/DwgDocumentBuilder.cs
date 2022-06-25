using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder
	{
		public DwgReaderFlags Flags { get; set; }

		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new List<CadBlockRecordTemplate>();

		public List<CadDictionaryTemplate> CadDictionaryTemplates { get; set; } = new List<CadDictionaryTemplate>();

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
	}
}
