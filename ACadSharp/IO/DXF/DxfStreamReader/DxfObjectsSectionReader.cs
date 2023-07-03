using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using System;

namespace ACadSharp.IO.DXF
{
	internal class DxfObjectsSectionReader : DxfSectionReaderBase
	{
		public delegate bool ReadObjectDelegate<T>(CadTemplate template, DxfMap map) where T : CadObject;

		public DxfObjectsSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder)
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
				CadTemplate template = null;

				try
				{
					template = this.readObject();
				}
				catch (Exception ex)
				{
					if (!this._builder.Configuration.Failsafe)
						throw;

					this._builder.Notify($"Error while reading an object at line {this._reader.Position}", NotificationType.Error, ex);

					while (this._reader.DxfCode != DxfCode.Start)
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

			switch (this._reader.ValueAsString)
			{
				case DxfFileToken.ObjectDictionary:
					return this.readDictionary();
				case DxfFileToken.ObjectLayout:
					return this.readObjectCodes<Layout>(new CadLayoutTemplate(), readLayout);
				case DxfFileToken.ObjectDictionaryVar:
					template = new CadTemplate<DictionaryVariable>(new DictionaryVariable());
					break;
				case DxfFileToken.ObjectSortEntsTable:
					return this.readSortentsTable();
				case DxfFileToken.ObjectXRecord:
					return this.readObjectCodes<XRecrod>(new CadXRecordTemplate(), readXRecord);
				default:
					this._builder.Notify($"Object not implemented: {this._reader.ValueAsString}", NotificationType.NotImplemented);
					do
					{
						this._reader.ReadNext();
					}
					while (this._reader.DxfCode != DxfCode.Start);
					return null;
			}

			//Jump the 0 marker
			this._reader.ReadNext();

			this.readCommonObjectData(template);

			while (this._reader.DxfCode == DxfCode.Subclass)
			{
				switch (this._reader.ValueAsString)
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
						this._builder.Notify($"Unhandeled dxf entity subclass {this._reader.ValueAsString}");
						while (this._reader.DxfCode != DxfCode.Start)
							this._reader.ReadNext();
						break;
				}
			}

			return template;
		}

		protected CadTemplate readObjectCodes<T>(CadTemplate template, ReadObjectDelegate<T> readEntity)
			where T : CadObject
		{
			this._reader.ReadNext();

			DxfMap map = DxfMap.Create<T>();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				if (!readEntity(template, map))
				{
					this.readCommonCodes(template, out bool isExtendedData, map);
					if (isExtendedData)
						continue;
				}

				if (this._reader.DxfCode != DxfCode.Start)
					this._reader.ReadNext();
			}

			return template;
		}

		private bool readPlotSettings(CadTemplate template, DxfMap map)
		{
			switch (this._reader.Code)
			{
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.PlotSettings]);
			}
		}

		private bool readLayout(CadTemplate template, DxfMap map)
		{
			CadLayoutTemplate tmp = template as CadLayoutTemplate;

			switch (this._reader.Code)
			{
				case 330:
					tmp.PaperSpaceBlockHandle = this._reader.ValueAsHandle;
					return true;
				case 331:
					tmp.LasActiveViewportHandle = (this._reader.ValueAsHandle);
					return true;
				default:
					if (!this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Layout]))
					{
						return this.readPlotSettings(template, map);
					}
					return true;
			}
		}

		private bool readXRecord(CadTemplate template, DxfMap map)
		{
			CadXRecordTemplate tmp = template as CadXRecordTemplate;

			//TODO: Finsih cadXrecordtemplate

			switch (this._reader.Code)
			{
				case 100 when this._reader.ValueAsString == DxfSubclassMarker.XRecord:
					this.readXRecordEntries(tmp.CadObject);
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.XRecord]);
			}
		}

		private void readXRecordEntries(XRecrod recrod)
		{
			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				recrod.Entries.Add(new XRecrod.Entry(this._reader.Code, this._reader.Value));

				this._reader.ReadNext();
			}
		}

		private CadTemplate readDictionary()
		{
			CadDictionary cadDictionary = new CadDictionary();
			CadDictionaryTemplate template = new CadDictionaryTemplate(cadDictionary);

			string lastKey = null;

			//Jump the 0 marker
			this._reader.ReadNext();

			this.readCommonObjectData(template);

			System.Diagnostics.Debug.Assert(DxfSubclassMarker.Dictionary == this._reader.ValueAsString);

			//Jump the 100 marker
			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 280:
						cadDictionary.HardOwnerFlag = this._reader.ValueAsBool;
						break;
					case 281:
						cadDictionary.ClonningFlags = (DictionaryCloningFlags)this._reader.Value;
						break;
					case 3:
						lastKey = this._reader.ValueAsString;
						template.Entries.Add(lastKey, null);
						break;
					case 350: // Soft-owner ID/handle to entry object 
					case 360: // Hard-owner ID/handle to entry object
						template.Entries[lastKey] = this._reader.ValueAsHandle;
						break;
					default:
						this._builder.Notify($"Group Code not handled {this._reader.GroupCodeValue} for {typeof(CadDictionary)}, code : {this._reader.Code} | value : {this._reader.ValueAsString}");
						break;
				}

				this._reader.ReadNext();
			}

			return template;
		}

		private CadTemplate readSortentsTable()
		{
			SortEntitiesTable sortTable = new SortEntitiesTable();
			CadSortensTableTemplate template = new CadSortensTableTemplate(sortTable);

			//Jump the 0 marker
			this._reader.ReadNext();

			this.readCommonObjectData(template);

			System.Diagnostics.Debug.Assert(DxfSubclassMarker.SortentsTable == this._reader.ValueAsString);

			//Jump the 100 marker
			this._reader.ReadNext();

			(ulong?, ulong?) pair = (null, null);

			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 5:
						pair.Item1 = this._reader.ValueAsHandle;
						break;
					case 330:
						template.BlockOwnerHandle = this._reader.ValueAsHandle;
						break;
					case 331:
						pair.Item2 = this._reader.ValueAsHandle;
						break;
					default:
						this._builder.Notify($"Group Code not handled {this._reader.GroupCodeValue} for {typeof(SortEntitiesTable)}, code : {this._reader.Code} | value : {this._reader.ValueAsString}");
						break;
				}

				if (pair.Item1.HasValue && pair.Item2.HasValue)
				{
					template.Values.Add((pair.Item1.Value, pair.Item2.Value));
					pair = (null, null);
				}

				this._reader.ReadNext();
			}

			return template;
		}
	}
}
