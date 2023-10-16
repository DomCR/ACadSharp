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

				//Add the object and the template to the builder
				this._builder.AddTemplate(template);
			}
		}
	}
}
