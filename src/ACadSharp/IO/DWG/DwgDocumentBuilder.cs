using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG;

internal class DwgDocumentBuilder : CadDocumentBuilder
{
	/// <summary>
	/// Modeler geometry entities (R2013+) whose "has DS binary data" flag is set:
	/// their ACIS payload lives in the AcDs data section and is attached to them
	/// once that section is read.
	/// </summary>
	public List<ModelerGeometry> AcisDsEntities { get; } = new();

	public DwgReaderConfiguration Configuration { get; }

	public DwgHeaderHandlesCollection HeaderHandles { get; set; } = new();

	public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new();

	public List<Entity> PaperSpaceEntities { get; } = new();

	public List<Entity> ModelSpaceEntities { get; } = new();

	/// <summary>
	/// [PATCH] Streaming-scan mode: decoded entities are not added to the ModelSpaceEntities/PaperSpaceEntities
	/// lists (dead storage in the DWG path, they would only pin memory); the caller releases the
	/// templates via PruneTemplate. Default false: full/lazy read behavior is unchanged.
	/// </summary>
	public bool SkipEntityTracking { get; set; }

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
		this.createMissingHandles();

		//Set the names for the block records before add them to the table
		foreach (var item in this.BlockRecordTemplates)
		{
			item.SetBlockToRecord(this, this.HeaderHandles);
		}


		// [PATCH] After a partial read, table templates may have been assigned auto-generated handles
		// (InitialHandSeed+N) by createMissingHandles, while the tableTemplates dictionary keys are still
		// the file's original handles (the header CONTROL_OBJECT handles). BuildTables looks up the
		// templates by the builder table handles, so the auto values do not match —> "Table X not found".
		// Realign the builder table handles with the header CONTROL_OBJECT handles here
		// (idempotent: in a full read the two are already equal).
		var hh = this.HeaderHandles;
		if (hh.APPID_CONTROL_OBJECT.HasValue) this.AppIds.Handle = hh.APPID_CONTROL_OBJECT.Value;
		if (hh.STYLE_CONTROL_OBJECT.HasValue) this.TextStyles.Handle = hh.STYLE_CONTROL_OBJECT.Value;
		if (hh.LINETYPE_CONTROL_OBJECT.HasValue) this.LineTypesTable.Handle = hh.LINETYPE_CONTROL_OBJECT.Value;
		if (hh.LAYER_CONTROL_OBJECT.HasValue) this.Layers.Handle = hh.LAYER_CONTROL_OBJECT.Value;
		if (hh.UCS_CONTROL_OBJECT.HasValue) this.UCSs.Handle = hh.UCS_CONTROL_OBJECT.Value;
		if (hh.VIEW_CONTROL_OBJECT.HasValue) this.Views.Handle = hh.VIEW_CONTROL_OBJECT.Value;
		if (hh.BLOCK_CONTROL_OBJECT.HasValue) this.BlockRecords.Handle = hh.BLOCK_CONTROL_OBJECT.Value;
		if (hh.DIMSTYLE_CONTROL_OBJECT.HasValue) this.DimensionStyles.Handle = hh.DIMSTYLE_CONTROL_OBJECT.Value;
		if (hh.VPORT_CONTROL_OBJECT.HasValue) this.VPorts.Handle = hh.VPORT_CONTROL_OBJECT.Value;
		this.RegisterTables();

		this.BuildTables();

		this.buildDictionaries();

		base.BuildDocument();

		this.HeaderHandles.UpdateHeader(this.DocumentToBuild.Header, this);
	}
}
