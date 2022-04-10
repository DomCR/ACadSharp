using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using System;
using System.Linq;

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
			switch (this._reader.LastValueAsString)
			{
				case DxfFileToken.ObjectDictionary:
					return this.readDictionary();
				default:
					this._notification?.Invoke(null, new NotificationEventArgs($"Object not implemented: {this._reader.LastValueAsString}"));
					do
					{
						this._reader.ReadNext();
					}
					while (this._reader.LastDxfCode != DxfCode.Start);
					return null;
			}
		}

		private CadTemplate readDictionary()
		{
			CadDictionary cadDictionary = new CadDictionary();
			DwgDictionaryTemplate template = new DwgDictionaryTemplate(cadDictionary);

			string lastKey = null;

			//Jump the 0 marker
			this._reader.ReadNext();

			this.readCommonObjectData(template);

			System.Diagnostics.Debug.Assert(DxfSubclassMarker.Dictionary == this._reader.LastValueAsString);

			//Jump the 100 marker
			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				switch (this._reader.LastCode)
				{
					case 280:
						cadDictionary.HardOwnerFlag = this._reader.LastValueAsBool;
						break;
					case 281:
						cadDictionary.ClonningFlags = (DictionaryCloningFlags)this._reader.LastValue;
						break;
					case 3:
						lastKey = this._reader.LastValueAsString;
						template.Entries.Add(lastKey, null);
						break;
					case 350:
						template.Entries[lastKey] = this._reader.LastValueAsHandle;
						break;
					default:
						this._notification?.Invoke(null, new NotificationEventArgs($"Group Code not handled {this._reader.LastGroupCodeValue} for {typeof(CadDictionary)}, code : {this._reader.LastCode} | value : {this._reader.LastValueAsString}"));
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}
	}
}
