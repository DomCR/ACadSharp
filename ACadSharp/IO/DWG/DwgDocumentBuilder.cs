using ACadSharp.IO.Templates;
using ACadSharp.Tables.Collections;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder
	{
		public DwgReaderConfiguration Configuration { get; }

		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public AppIdsTable AppIds { get; set; }

		public BlockRecordsTable BlockRecords { get; set; }

		public DimensionStylesTable DimensionStyles { get; set; }

		public LayersTable Layers { get; set; }

		public LineTypesTable LineTypesTable { get; set; }

		public TextStylesTable TextStyles { get; set; }

		public UCSTable UCSs { get; set; }

		public ViewsTable Views { get; set; }

		public VPortsTable VPorts { get; set; }

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

			this.BuildTables();

			this.DocumentToBuild.RegisterCollection(AppIds);
			this.DocumentToBuild.RegisterCollection(Layers);
			this.DocumentToBuild.RegisterCollection(LineTypesTable);
			this.DocumentToBuild.RegisterCollection(TextStyles);
			this.DocumentToBuild.RegisterCollection(UCSs);
			this.DocumentToBuild.RegisterCollection(Views);
			this.DocumentToBuild.RegisterCollection(DimensionStyles);
			this.DocumentToBuild.RegisterCollection(VPorts);
			this.DocumentToBuild.RegisterCollection(BlockRecords);

			base.BuildDocument();
		}

		public void BuildTables()
		{
			foreach (ICadTableTemplate template in this.tableTemplates.Values)
			{
				template.Build(this);
			}
		}
	}
}
