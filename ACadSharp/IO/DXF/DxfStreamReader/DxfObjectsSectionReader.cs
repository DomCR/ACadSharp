using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using System;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfObjectsSectionReader : DxfSectionReaderBase
	{
		public DxfObjectsSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder)
			: base(reader, builder)
		{
		}

		public override void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//Loop until the section ends
			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
			{
				CadTemplate template = null;

				try
				{
					template = this.readObject();
				}
				catch (Exception)
				{
					if (!this._builder.Configuration.Failsafe)
						throw;

					while (this._reader.LastDxfCode != DxfCode.Start)
						this._reader.ReadNext();
				}

				if (template == null)
					continue;

				//Add the object and the template to the builder
				this._builder.AddTemplate(template);
			}
		}

		private CadTemplate readObject()
		{
			CadTemplate template = null;

			switch (this._reader.LastValueAsString)
			{
				case DxfFileToken.ObjectDictionary:
					return this.readDictionary();
				case DxfFileToken.ObjectLayout:
					template = new CadLayoutTemplate(new Layout());
					break;
				case DxfFileToken.ObjectDictionaryVar:
					template = new CadTemplate<DictionaryVariable>(new DictionaryVariable());
					break;
				case DxfFileToken.TableXRecord:
					template = new CadXRecordTemplate(new XRecrod());
					break;
				default:
					this._builder.Notify($"Object not implemented: {this._reader.LastValueAsString}", NotificationType.NotImplemented);
					do
					{
						this._reader.ReadNext();
					}
					while (this._reader.LastDxfCode != DxfCode.Start);
					return null;
			}

			//Jump the 0 marker
			this._reader.ReadNext();

			this.readCommonObjectData(template);

			while (this._reader.LastDxfCode == DxfCode.Subclass)
			{
				switch (this._reader.LastValueAsString)
				{
					case DxfSubclassMarker.DictionaryVariables:
						this.readMapped<DictionaryVariable>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Layout:
						this.readMapped<Layout>(template.CadObject, template);
						break;
					case DxfSubclassMarker.PlotSettings:
						this.readMapped<PlotSettings>(template.CadObject, template);
						break;
					case DxfSubclassMarker.XRecord:
						this.readMapped<XRecrod>(template.CadObject, template);
						break;
					default:
						this._builder.Notify($"Unhandeled dxf entity subclass {this._reader.LastValueAsString}");
						while (this._reader.LastDxfCode != DxfCode.Start)
							this._reader.ReadNext();
						break;
				}
			}

			return template;
		}

		private CadTemplate readDictionary()
		{
			CadDictionary cadDictionary = new CadDictionary();
			CadDictionaryTemplate template = new CadDictionaryTemplate(cadDictionary);

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
					case 350: // Soft-owner ID/handle to entry object 
					case 360: // Hard-owner ID/handle to entry object
						template.Entries[lastKey] = this._reader.LastValueAsHandle;
						break;
					default:
						this._builder.Notify($"Group Code not handled {this._reader.LastGroupCodeValue} for {typeof(CadDictionary)}, code : {this._reader.LastCode} | value : {this._reader.LastValueAsString}");
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}
	}
}
