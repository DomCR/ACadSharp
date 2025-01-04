﻿using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using System;
using System.Linq;
using System.Threading;
using static ACadSharp.IO.Templates.CadEvaluationGraphTemplate;

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
				case DxfFileToken.ObjectDBColor:
					return this.readObjectCodes<BookColor>(new CadNonGraphicalObjectTemplate(new BookColor()), this.readBookColor);
				case DxfFileToken.ObjectDictionary:
					return this.readObjectCodes<CadDictionary>(new CadDictionaryTemplate(), this.readDictionary);
				case DxfFileToken.ObjectDictionaryWithDefault:
					return this.readObjectCodes<CadDictionaryWithDefault>(new CadDictionaryWithDefaultTemplate(), this.readDictionaryWithDefault);
				case DxfFileToken.ObjectLayout:
					return this.readObjectCodes<Layout>(new CadLayoutTemplate(), this.readLayout);
				case DxfFileToken.ObjectEvalGraph:
					return this.readObjectCodes<EvaluationGraph>(new CadEvaluationGraphTemplate(), this.readEvaluationGraph);
				case DxfFileToken.ObjectDictionaryVar:
					return this.readObjectCodes<DictionaryVariable>(new CadTemplate<DictionaryVariable>(new DictionaryVariable()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectPdfDefinition:
					return this.readObjectCodes<PdfUnderlayDefinition>(new CadNonGraphicalObjectTemplate(new PdfUnderlayDefinition()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectSortEntsTable:
					return this.readSortentsTable();
				case DxfFileToken.ObjectScale:
					return this.readObjectCodes<Scale>(new CadTemplate<Scale>(new Scale()), this.readScale);
				case DxfFileToken.ObjectTableContent:
					return this.readObjectCodes<TableContent>(new CadTableContentTemplate(), this.readTableContent);
				case DxfFileToken.ObjectVisualStyle:
					return this.readObjectCodes<VisualStyle>(new CadTemplate<VisualStyle>(new VisualStyle()), this.readVisualStyle);
				case DxfFileToken.ObjectXRecord:
					return this.readObjectCodes<XRecord>(new CadXRecordTemplate(), this.readXRecord);
				default:
					DxfMap map = DxfMap.Create<CadObject>();
					CadUnknownNonGraphicalObjectTemplate unknownEntityTemplate = null;
					if (this._builder.DocumentToBuild.Classes.TryGetByName(this._reader.ValueAsString, out Classes.DxfClass dxfClass))
					{
						this._builder.Notify($"NonGraphicalObject not supported read as an UnknownNonGraphicalObject: {this._reader.ValueAsString}", NotificationType.NotImplemented);
						unknownEntityTemplate = new CadUnknownNonGraphicalObjectTemplate(new UnknownNonGraphicalObject(dxfClass));
					}
					else
					{
						this._builder.Notify($"UnknownNonGraphicalObject not supported: {this._reader.ValueAsString}", NotificationType.NotImplemented);
					}

					this._reader.ReadNext();

					do
					{
						if (unknownEntityTemplate != null && this._builder.KeepUnknownEntities)
						{
							this.readCommonCodes(unknownEntityTemplate, out bool isExtendedData, map);
							if (isExtendedData)
								continue;
						}

						this._reader.ReadNext();
					}
					while (this._reader.DxfCode != DxfCode.Start);

					return unknownEntityTemplate;
			}
		}

		protected CadTemplate readObjectCodes<T>(CadTemplate template, ReadObjectDelegate<T> readObject)
			where T : CadObject
		{
			this._reader.ReadNext();

			DxfMap map = DxfMap.Create<T>();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				if (!readObject(template, map))
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

		private bool readEvaluationGraph(CadTemplate template, DxfMap map)
		{
			CadEvaluationGraphTemplate tmp = template as CadEvaluationGraphTemplate;
			EvaluationGraph evGraph = tmp.CadObject;

			switch (this._reader.Code)
			{
				case 91:
					while (this._reader.Code == 91)
					{
						GraphNodeTemplate nodeTemplate = new GraphNodeTemplate();
						EvaluationGraph.Node node = nodeTemplate.Node;

						node.Index = this._reader.ValueAsInt;

						this._reader.ExpectedCode(93);
						node.Flags = this._reader.ValueAsInt;

						this._reader.ExpectedCode(95);
						node.NextNodeIndex = this._reader.ValueAsInt;

						this._reader.ExpectedCode(360);
						nodeTemplate.ExpressionHandle = this._reader.ValueAsHandle;

						this._reader.ExpectedCode(92);
						node.Data1 = this._reader.ValueAsInt;
						this._reader.ExpectedCode(92);
						node.Data2 = this._reader.ValueAsInt;
						this._reader.ExpectedCode(92);
						node.Data3 = this._reader.ValueAsInt;
						this._reader.ExpectedCode(92);
						node.Data4 = this._reader.ValueAsInt;

						this._reader.ReadNext();

						tmp.NodeTemplates.Add(nodeTemplate);
					}

					if (this._reader.DxfCode == DxfCode.Start)
					{
						return true;
					}

					return this.readEvaluationGraph(template, map);
				case 92:
					//Edges
					while (this._reader.Code == 92)
					{
						this._reader.ExpectedCode(93);
						this._reader.ExpectedCode(94);
						this._reader.ExpectedCode(91);
						this._reader.ExpectedCode(91);
						this._reader.ExpectedCode(92);
						this._reader.ExpectedCode(92);
						this._reader.ExpectedCode(92);
						this._reader.ExpectedCode(92);
						this._reader.ExpectedCode(92);

						this._reader.ReadNext();
					}

					if(this._reader.DxfCode == DxfCode.Start)
					{
						return true;
					}

					return this.readEvaluationGraph(template, map);
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.EvalGraph]);
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

		private bool readScale(CadTemplate template, DxfMap map)
		{
			switch (this._reader.Code)
			{
				// Undocumented codes
				case 70:
					//Always 0
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Scale]);
			}
		}

		private bool readTableContent(CadTemplate template, DxfMap map)
		{
			switch (this._reader.Code)
			{
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.TableContent]);
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

			switch (this._reader.Code)
			{
				case 100 when this._reader.ValueAsString == DxfSubclassMarker.XRecord:
					this.readXRecordEntries(tmp);
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.XRecord]);
			}
		}

		private void readXRecordEntries(CadXRecordTemplate template)
		{
			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.GroupCodeValue)
				{
					case GroupCodeValueType.Handle:
						template.AddHandleReference(this._reader.Code, this._reader.ValueAsHandle);
						break;
					default:
						template.CadObject.CreateEntry(this._reader.Code, this._reader.Value);
						break;
				}

				this._reader.ReadNext();
			}
		}

		private bool readBookColor(CadTemplate template, DxfMap map)
		{
			CadNonGraphicalObjectTemplate tmp = template as CadNonGraphicalObjectTemplate;
			BookColor color = tmp.CadObject as BookColor;

			switch (this._reader.Code)
			{
				case 430:
					color.Name = this._reader.ValueAsString;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.DbColor]);
			}
		}

		private bool readDictionary(CadTemplate template, DxfMap map)
		{
			CadDictionaryTemplate tmp = template as CadDictionaryTemplate;
			CadDictionary cadDictionary = tmp.CadObject;

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
						return this.readDictionary(template, map);
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
