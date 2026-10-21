using ACadSharp.IO.DXF.DxfStreamReader;
using ACadSharp.IO.Templates;
using System;

namespace ACadSharp.IO.DXF
{
	internal class DxfEntitiesSectionReader : DxfSectionReaderBase
	{
		public DxfEntitiesSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder)
			: base(reader, builder)
		{
		}

		public override void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			// [PATCH] Periodic full GC to keep the working set close to the live object graph
			// (reading a huge DXF allocates a lot of transient garbage; without this the WS
			// grows far beyond the live size). 0 disables (upstream behavior).
			int gcEvery = this._builder.Configuration.GCEveryNEntities;
			int gcCounter = 0;

			//Loop until the section ends
			while (this._reader.ValueAsString != DxfFileToken.EndSection)
			{
				CadEntityTemplate template = null;

				try
				{
					template = this.readEntity();
				}
				catch (Exception ex)
				{
					if (!this._builder.Configuration.Failsafe)
						throw;

					this._builder.Notify($"Error while reading an entity at line {this._reader.Position}", NotificationType.Error, ex);

					while (this._reader.DxfCode != DxfCode.Start)
						this._reader.ReadNext();
				}

				if (template == null)
					continue;

				// [PATCH] Periodic GC + working-set trim (see gcEvery above): keeps the working
				// set close to the live object graph instead of letting committed-but-free
				// memory accumulate (23GB WS -> ~13GB for a 1.9M-entity file with XData).
				if (gcEvery > 0 && ++gcCounter >= gcEvery)
				{
					gcCounter = 0;
					MemoryTrimmer.Trim();
				}

				//Add the object and the template to the builder
				this._builder.AddTemplate(template);

				if (template.OwnerHandle == null)
				{
					this._builder.ModelSpaceEntities.Add(template.CadObject);
				}
				else if (this._builder.TryGetObjectTemplate(template.OwnerHandle, out ICadOwnerTemplate owner))
				{
					owner.OwnedObjectsHandlers.Add(template.CadObject.Handle);
				}
				else
				{
					_builder.OrphanTemplates.Add(template);
				}
			}
		}
	}
}
