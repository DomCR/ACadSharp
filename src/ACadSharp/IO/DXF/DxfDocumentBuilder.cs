using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfDocumentBuilder : CadDocumentBuilder
	{
		public DxfReaderConfiguration Configuration { get; }

		public override bool KeepUnknownEntities => this.Configuration.KeepUnknownEntities;

		public override bool KeepUnknownNonGraphicalObjects => this.Configuration.KeepUnknownNonGraphicalObjects;

		public HashSet<Entity> ModelSpaceEntities { get; } = new();

		public CadBlockRecordTemplate ModelSpaceTemplate { get; set; }

		public DxfDocumentBuilder(ACadVersion version, CadDocument document, DxfReaderConfiguration configuration) : base(version, document)
		{
			this.Configuration = configuration;
		}

		public override void BuildDocument()
		{
			if (this.ModelSpaceTemplate == null)
			{
				BlockRecord record = BlockRecord.ModelSpace;
				this.BlockRecords.Add(record);
				this.ModelSpaceTemplate = new CadBlockRecordTemplate(record);
				this.AddTemplate(this.ModelSpaceTemplate);
			}

			this.createMissingHandles();

			this.ModelSpaceTemplate.OwnedObjectsHandlers.UnionWith(this.ModelSpaceEntities.Select(o => o.Handle));

			this.RegisterTables();

			this.BuildTables();

			this.buildDictionaries();

			//Assign the owners for the different objects
			foreach (CadTemplate template in this.cadObjectsTemplates.Values)
			{
				this.assignOwner(template);
			}

			base.BuildDocument();

			if (this.Configuration.CreateDefaults)
			{
				this.DocumentToBuild.CreateDefaults();
			}
		}

		public List<Entity> BuildEntities()
		{
			var entities = new List<Entity>();

			foreach (CadEntityTemplate item in this.cadObjectsTemplates.Values.OfType<CadEntityTemplate>())
			{
				item.Build(this);

				item.SetUnlinkedReferences();
			}

			foreach (var item in this.cadObjectsTemplates.Values
				.OfType<CadEntityTemplate>()
				.Where(o => o.CadObject.Owner == null))
			{
				item.CadObject.Handle = 0;
				entities.Add(item.CadObject);
			}

			return entities;
		}

		private void assignOwner(CadTemplate template)
		{
			if (template.CadObject.Owner != null || template.CadObject is CadDictionary || !template.OwnerHandle.HasValue)
				return;

			if (this.TryGetObjectTemplate(template.OwnerHandle, out CadTemplate owner))
			{
				switch (owner)
				{
					case CadDictionaryTemplate:
						//Entries of the dictionary are assigned in the template
						break;
					case CadBlockRecordTemplate record when template.CadObject is Entity entity:
						//The entries should be assigned in the blocks or entities section
						break;
					case CadPolyLineTemplate pline when template.CadObject is Vertex v:
						pline.VertexHandles.Add(v.Handle);
						break;
					case CadPolyLineTemplate pline when template.CadObject is Seqend seqend:
						pline.SeqendHandle = seqend.Handle;
						break;
					case CadInsertTemplate insert when template.CadObject is AttributeEntity att:
						insert.AttributesHandles.Add(att.Handle);
						break;
					case CadInsertTemplate insert when template.CadObject is Seqend seqend:
						insert.SeqendHandle = seqend.Handle;
						break;
					default:
						this.Notify($"Owner {owner.GetType().Name} with handle {template.OwnerHandle} assignation not implemented for {template.CadObject.GetType().Name} with handle {template.CadObject.Handle}");
						break;
				}
			}
			else
			{
				this.Notify($"Owner {template.OwnerHandle} not found for {template.GetType().FullName} with handle {template.CadObject.Handle}");
			}
		}
	}
}