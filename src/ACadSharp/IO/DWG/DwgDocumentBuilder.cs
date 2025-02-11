using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder<DwgReaderConfiguration>
	{
		public DwgHeaderHandlesCollection HeaderHandles { get; set; } = new();

		public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new();

		public List<Entity> PaperSpaceEntities { get; } = new();

		public List<Entity> ModelSpaceEntities { get; } = new();

		public DwgDocumentBuilder(ACadVersion version, CadDocument document, DwgReaderConfiguration configuration)
			: base(version, document, configuration)
		{
		}

		public override void BuildDocument()
		{
			//Set the names for the block records before add them to the table
			foreach (var item in this.BlockRecordTemplates)
			{
				item.SetBlockToRecord(this);
			}

			this.RegisterTables();

			this.BuildTables();

			this.buildDictionaries();

			base.BuildDocument();

			this.HeaderHandles.UpdateHeader(this.DocumentToBuild.Header, this);
		}
	}
}
