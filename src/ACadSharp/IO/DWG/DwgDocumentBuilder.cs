using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG;

internal class DwgDocumentBuilder : CadDocumentBuilder
{
	public DwgReaderConfiguration Configuration { get; }

	public DwgHeaderHandlesCollection HeaderHandles { get; set; } = new();

	public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new();

	public List<Entity> PaperSpaceEntities { get; } = new();

	public List<Entity> ModelSpaceEntities { get; } = new();

	public override bool KeepUnknownEntities => this.Configuration.KeepUnknownEntities;

	public override bool KeepUnknownNonGraphicalObjects => this.Configuration.KeepUnknownNonGraphicalObjects;

	public override bool IgnoreProxyGraphics => this.Configuration.IgnoreProxyGraphics;

	public DwgDocumentBuilder(ACadVersion version, CadDocument document, DwgReaderConfiguration configuration)
		: base(version, document)
	{
		this.Configuration = configuration;
	}

	public override void BuildDocument()
	{
		if (this.Version < ACadVersion.AC1012)
		{
			//The objects created by the document get the handles after the ones assigned by the reader
			this.DocumentToBuild.Header.HandleSeed = this.InitialHandSeed + 1;
		}

		this.createMissingHandles();

		//Set the names for the block records before add them to the table
		foreach (var item in this.BlockRecordTemplates)
		{
			item.SetBlockToRecord(this, this.HeaderHandles);
		}

		this.RegisterTables();

		this.BuildTables();

		if (this.Version < ACadVersion.AC1012)
		{
			//The drawings older than R13 have no objects, the default entries and dictionaries are created
			this.DocumentToBuild.CreateDefaults();
		}
		else
		{
			this.buildDictionaries();
		}

		base.BuildDocument();

		this.HeaderHandles.UpdateHeader(this.DocumentToBuild.Header, this);

		if (this.Version < ACadVersion.AC1012)
		{
			this.createLayouts();
		}
	}

	/// <summary>
	/// Create the layouts for the model and paper space, the drawings older than R13 don't have them.
	/// </summary>
	private void createLayouts()
	{
		if (this.TryGetCadObject(this.HeaderHandles.MODEL_SPACE, out BlockRecord modelSpace) && modelSpace.Layout == null)
		{
			Layout layout = new Layout();
			layout.Name = Layout.ModelLayoutName;
			this.DocumentToBuild.Layouts.Add(layout);
			layout.AssociatedBlock = modelSpace;
		}

		if (this.TryGetCadObject(this.HeaderHandles.PAPER_SPACE, out BlockRecord paperSpace) && paperSpace.Layout == null)
		{
			Layout layout = new Layout();
			layout.Name = Layout.PaperLayoutName;
			layout.TabOrder = 1;
			this.DocumentToBuild.Layouts.Add(layout);
			layout.AssociatedBlock = paperSpace;

			//Keep the paper viewport of the drawing if it has one
			if (layout.PaperViewport == null)
			{
				layout.UpdatePaperViewport();
			}
		}
	}
}
