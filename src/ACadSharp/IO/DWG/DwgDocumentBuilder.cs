using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG;

internal class DwgDocumentBuilder : CadDocumentBuilder
{
	public DwgReaderConfiguration Configuration { get; }

	public DwgHeaderHandlesCollection HeaderHandles { get; set; } = new();

	public List<CadBlockRecordTemplate> BlockRecordTemplates { get; set; } = new();

	public List<Entity> PaperSpaceEntities { get; } = new();

	public List<Entity> ModelSpaceEntities { get; } = new();

	/// <summary>
	/// [PATCH] Streaming scan mode: decoded entities are no longer added to the
	/// ModelSpaceEntities/PaperSpaceEntities lists (dead storage on the DWG path, they only
	/// pin memory); the caller releases them promptly through PruneTemplate.
	/// Defaults to false; full/lazy read behavior is unchanged.
	/// </summary>
	public bool SkipEntityTracking { get; set; }

	public override bool KeepUnknownEntities => this.Configuration.KeepUnknownEntities;

	public override bool KeepUnknownNonGraphicalObjects => this.Configuration.KeepUnknownNonGraphicalObjects;

	public override bool IgnoreProxyGraphics => this.Configuration.IgnoreProxyGraphics;

	public DwgDocumentBuilder(ACadVersion version, CadDocument document, DwgReaderConfiguration configuration)
		: base(version, document)
	{
		this.Configuration = configuration;

		if (configuration != null)
		{
			this.ShapeFontDirectories.AddRange(configuration.FontSearchDirectories);
		}
	}



	public override void BuildDocument()
	{
		this.createMissingHandles();

		//Set the names for the block records before add them to the table
		foreach (var item in this.BlockRecordTemplates)
		{
			item.SetBlockToRecord(this, this.HeaderHandles);
		}

		// [PATCH] On a partial read, a table template's table object handle may have been rewritten by
		// createMissingHandles to an auto-generated value (InitialHandSeed+N), while the tableTemplates
		// dictionary key is still the file's original handle (the header CONTROL_OBJECT handle).
		// BuildTables looks up the template by the builder table handle, so the auto value is not found
		// -> "Table X not found". Here the builder table handles are realigned using the header
		// CONTROL_OBJECT handles (idempotent: on a full read the two are already equal).
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
