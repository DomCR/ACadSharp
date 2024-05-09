﻿using ACadSharp.Entities;
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

		public CadBlockRecordTemplate ModelSpaceTemplate { get; set; }

		public HashSet<ulong> ModelSpaceEntities { get; } = new();

		public override bool KeepUnknownEntities => this.Configuration.KeepUnknownEntities;

		public DxfDocumentBuilder(CadDocument document, DxfReaderConfiguration configuration) : base(document)
		{
			this.Configuration = configuration;
		}

		public override void BuildDocument()
		{
			this.RegisterTables();

			this.BuildTables();

			//Assign the owners for the different objects
			foreach (CadTemplate template in this.templates.Values)
			{
				this.assignOwner(template);
			}

			if (this.ModelSpaceTemplate != null)
			{
				this.ModelSpaceTemplate.OwnedObjectsHandlers.AddRange(ModelSpaceEntities);
			}

			base.BuildDocument();
		}

		public List<Entity> BuildEntities()
		{
			var entities = new List<Entity>();

			foreach (CadEntityTemplate item in this.templates.Values.OfType<CadEntityTemplate>())
			{
				item.Build(this);

				item.SetUnlinkedReferences();

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
						record.OwnedObjectsHandlers.Add(entity.Handle);
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
