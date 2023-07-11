using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using System;
using System.Linq;

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
			switch (this._reader.ValueAsString)
			{
				case DxfFileToken.ObjectDictionary:
					return this.readObjectCodes<CadDictionary>(new CadDictionaryTemplate(), readDictionary);
				case DxfFileToken.ObjectDictionaryWithDefault:
					return this.readObjectCodes<CadDictionaryWithDefault>(new CadDictionaryWithDefaultTemplate(), this.readDictionaryWithDefault);
				case DxfFileToken.ObjectLayout:
					return this.readObjectCodes<Layout>(new CadLayoutTemplate(), readLayout);
				case DxfFileToken.ObjectDictionaryVar:
					return this.readObjectCodes<DictionaryVariable>(new CadTemplate<DictionaryVariable>(new DictionaryVariable()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectSortEntsTable:
					return this.readSortentsTable();
				case DxfFileToken.ObjectScale:
					return this.readObjectCodes<Scale>(new CadTemplate<Scale>(new Scale()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectVisualStyle:
					return this.readObjectCodes<VisualStyle>(new CadTemplate<VisualStyle>(new VisualStyle()), this.readVisualStyle);
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

		private bool readObjectSubclassMap(CadTemplate template, DxfMap map)
		{
			switch (this._reader.Code)
			{
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[template.CadObject.SubclassMarker]);
			}
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

		private bool readVisualStyle(CadTemplate template, DxfMap map)
		{
			switch (this._reader.Code)
			{
				// Undocumented codes
				case 176:
				case 177:
				case 420:
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.VisualStyle]);
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

		private bool readDictionary(CadTemplate template, DxfMap map)
		{
			CadDictionary cadDictionary = new CadDictionary();
			CadDictionaryTemplate tmp = template as CadDictionaryTemplate;

			switch (this._reader.Code)
			{
				case 280:
					cadDictionary.HardOwnerFlag = this._reader.ValueAsBool;
					return true;
				case 281:
					cadDictionary.ClonningFlags = (DictionaryCloningFlags)this._reader.Value;
					return true;
				case 3:
					tmp.Entries.Add(this._reader.ValueAsString, null);
					return true;
				case 350: // Soft-owner ID/handle to entry object 
				case 360: // Hard-owner ID/handle to entry object
					tmp.Entries[tmp.Entries.LastOrDefault().Key] = this._reader.ValueAsHandle;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Dictionary]);
			}
		}

		private bool readDictionaryWithDefault(CadTemplate template, DxfMap map)
		{
			CadDictionaryWithDefaultTemplate tmp = template as CadDictionaryWithDefaultTemplate;

			switch (this._reader.Code)
			{
				case 340:
					tmp.DefaultEntryHandle = this._reader.ValueAsHandle;
					return true;
				default:
					if (!this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.DictionaryWithDefault]))
					{
						return readDictionary(template, map);
					}
					return true;
			}
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
