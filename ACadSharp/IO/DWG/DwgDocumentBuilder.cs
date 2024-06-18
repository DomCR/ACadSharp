using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder
	{
		public DwgReaderConfiguration Configuration { get; }

		public DwgHeaderHandlesCollection HeaderHandles { get; set; } = new();

		public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new();

		public List<UnknownEntity> UnknownEntities { get; } = new();

		public List<Entity> PaperSpaceEntities { get; } = new();

		public List<Entity> ModelSpaceEntities { get; } = new();

		public override bool KeepUnknownEntities => this.Configuration.KeepUnknownEntities;

		public DwgDocumentBuilder(ACadVersion version, CadDocument document, DwgReaderConfiguration configuration)
			: base(version, document)
		{
			this.Configuration = configuration;
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
		}
	}
}
