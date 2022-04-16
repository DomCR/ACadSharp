using ACadSharp.IO.Templates;
using System;

namespace ACadSharp.IO.DXF
{
	internal class DxfObjectsSectionReader : DxfSectionReaderBase
	{
		public DxfObjectsSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder, NotificationEventHandler notification = null)
			: base(reader, builder, notification)
		{
		}

		public override void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//Loop until the section ends
			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
			{
				CadTemplate template = this.readObject();

				if (template == null)
					continue;

				//Add the object and the template to the builder
				this._builder.Templates[template.CadObject.Handle] = template;
			}
		}

		private CadTemplate readObject()
		{
			throw new NotImplementedException();
		}
	}
}
