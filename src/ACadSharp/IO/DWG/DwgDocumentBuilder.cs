using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System.Collections.Generic;
using System.Linq;

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
		this.createMissingHandles();

		//Set the names for the block records before add them to the table
		foreach (var item in this.BlockRecordTemplates)
		{
			item.SetBlockToRecord(this, this.HeaderHandles);
		}

		this.attachSpaceEntities(this.HeaderHandles.MODEL_SPACE, this.ModelSpaceEntities);
		this.attachSpaceEntities(this.HeaderHandles.PAPER_SPACE, this.PaperSpaceEntities);

		this.RegisterTables();

		this.BuildTables();

		this.buildDictionaries();

		base.BuildDocument();

		this.HeaderHandles.UpdateHeader(this.DocumentToBuild.Header, this);
	}
	private void attachSpaceEntities(ulong? handle, List<Entity> entities)
	{
		if (!handle.HasValue || handle == 0 || entities.Count == 0) return;
		var space = this.BlockRecordTemplates.FirstOrDefault(t => t.CadObject.Handle == handle);
		if (space == null)
		{
			this.Notify($"Block record {handle} for {entities.Count} explicit space entities was not found.", NotificationType.Warning);
			return;
		}
		foreach (var entity in entities) space.OwnedObjectsHandlers.Add(entity.Handle);
	}

}
