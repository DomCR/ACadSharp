using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder
	{
		public DwgReaderConfiguration Configuration { get; }

		public DwgHeaderHandlesCollection HeaderHandles { get; set; } = new();

		public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new List<CadBlockRecordTemplate>();

		public DwgDocumentBuilder(CadDocument document, DwgReaderConfiguration configuration)
			: base(document)
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

			this.BuildTable(this.AppIds);
			this.BuildTable(this.LineTypesTable);
			this.BuildTable(this.Layers);
			this.BuildTable(this.TextStyles);
			this.BuildTable(this.UCSs);
			this.BuildTable(this.Views);
			this.BuildTable(this.DimensionStyles);
			this.BuildTable(this.VPorts);
			this.BuildTable(this.BlockRecords);

			base.BuildDocument();
		}
	}
}
