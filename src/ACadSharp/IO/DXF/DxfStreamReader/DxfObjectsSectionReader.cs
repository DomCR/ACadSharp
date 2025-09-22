using ACadSharp.Classes;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using CSMath;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
				case DxfFileToken.ObjectPlotSettings:
					return this.readObjectCodes<PlotSettings>(new CadNonGraphicalObjectTemplate(new PlotSettings()), this.readPlotSettings);
				case DxfFileToken.ObjectEvalGraph:
					return this.readObjectCodes<EvaluationGraph>(new CadEvaluationGraphTemplate(), this.readEvaluationGraph);
				case DxfFileToken.ObjectDictionaryVar:
					return this.readObjectCodes<DictionaryVariable>(new CadTemplate<DictionaryVariable>(new DictionaryVariable()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectPdfDefinition:
					return this.readObjectCodes<PdfUnderlayDefinition>(new CadNonGraphicalObjectTemplate(new PdfUnderlayDefinition()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectSortEntsTable:
					return this.readSortentsTable();
				case DxfFileToken.ObjectProxyObject:
					return this.readObjectCodes<ProxyObject>(new CadNonGraphicalObjectTemplate(new ProxyObject()), this.readProxyObject);
				case DxfFileToken.ObjectRasterVariables:
					return this.readObjectCodes<RasterVariables>(new CadNonGraphicalObjectTemplate(new RasterVariables()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectGroup:
					return this.readObjectCodes<Group>(new CadGroupTemplate(), this.readGroup);
				case DxfFileToken.ObjectGeoData:
					return this.readObjectCodes<GeoData>(new CadGeoDataTemplate(), this.readGeoData);
				case DxfFileToken.ObjectScale:
					return this.readObjectCodes<Scale>(new CadTemplate<Scale>(new Scale()), this.readScale);
				case DxfFileToken.ObjectTableContent:
					return this.readObjectCodes<TableContent>(new CadTableContentTemplate(), this.readTableContent);
				case DxfFileToken.ObjectVisualStyle:
					return this.readObjectCodes<VisualStyle>(new CadTemplate<VisualStyle>(new VisualStyle()), this.readVisualStyle);
				case DxfFileToken.ObjectSpatialFilter:
					return this.readObjectCodes<SpatialFilter>(new CadSpatialFilterTemplate(), this.readSpatialFilter);
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

		private bool readProxyObject(CadTemplate template, DxfMap map)
		{
			ProxyObject proxy = template.CadObject as ProxyObject;

			switch (this._reader.Code)
			{
				case 90:
				case 94:
				//Undocumented
				case 97:
				case 71:
					return true;
				case 95:
					int format = this._reader.ValueAsInt;
					proxy.Version = (ACadVersion)(format & 0xFFFF);
					proxy.MaintenanceVersion = (short)(format >> 16);
					return true;
				case 91:
					var classId = this._reader.ValueAsShort;
					if (this._builder.DocumentToBuild.Classes.TryGetByClassNumber(classId, out DxfClass dxfClass))
					{
						proxy.DxfClass = dxfClass;
					}
					return true;
				case 161:
					return true;
				case 162:
					return true;
				case 310:
					if (proxy.BinaryData == null)
					{
						proxy.BinaryData = new MemoryStream();
					}
					proxy.BinaryData.Write(this._reader.ValueAsBinaryChunk, 0, this._reader.ValueAsBinaryChunk.Length);
					return true;
				case 311:
					if (proxy.Data == null)
					{
						proxy.Data = new MemoryStream();
					}
					proxy.Data.Write(this._reader.ValueAsBinaryChunk, 0, this._reader.ValueAsBinaryChunk.Length);
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.ProxyObject]);
			}
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

					return this.checkObjectEnd(template, map, this.readEvaluationGraph);
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

					return this.checkObjectEnd(template, map, this.readEvaluationGraph);
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

		private bool readGroup(CadTemplate template, DxfMap map)
		{
			CadGroupTemplate tmp = template as CadGroupTemplate;

			switch (this._reader.Code)
			{
				case 70:
					return true;
				case 340:
					tmp.Handles.Add(this._reader.ValueAsHandle);
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[template.CadObject.SubclassMarker]);
			}
		}

		private bool readGeoData(CadTemplate template, DxfMap map)
		{
			CadGeoDataTemplate tmp = template as CadGeoDataTemplate;

			switch (this._reader.Code)
			{
				case 40 when tmp.CadObject.Version == GeoDataVersion.R2009:
					tmp.CadObject.ReferencePoint = new CSMath.XYZ(
						tmp.CadObject.ReferencePoint.X,
						this._reader.ValueAsDouble,
						tmp.CadObject.ReferencePoint.Z
						);
					return true;
				case 41 when tmp.CadObject.Version == GeoDataVersion.R2009:
					tmp.CadObject.ReferencePoint = new CSMath.XYZ(
						this._reader.ValueAsDouble,
						tmp.CadObject.ReferencePoint.Y,
						tmp.CadObject.ReferencePoint.Z
						);
					return true;
				case 42 when tmp.CadObject.Version == GeoDataVersion.R2009:
					tmp.CadObject.ReferencePoint = new CSMath.XYZ(
						tmp.CadObject.ReferencePoint.X,
						tmp.CadObject.ReferencePoint.Y,
						this._reader.ValueAsDouble
						);
					return true;
				case 46 when tmp.CadObject.Version == GeoDataVersion.R2009:
					tmp.CadObject.HorizontalUnitScale = this._reader.ValueAsDouble;
					return true;
				case 52 when tmp.CadObject.Version == GeoDataVersion.R2009:
					double angle = System.Math.PI / 2.0 - this._reader.ValueAsAngle;
					tmp.CadObject.NorthDirection = new CSMath.XY(Math.Cos(angle), Math.Sin(angle));
					return true;
				// Number of Geo-Mesh points
				case 93:
					var npts = this._reader.ValueAsInt;
					for (int i = 0; i < npts; i++)
					{
						this._reader.ReadNext();
						double sourceX = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						double sourceY = this._reader.ValueAsDouble;

						this._reader.ReadNext();
						double destX = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						double destY = this._reader.ValueAsDouble;

						tmp.CadObject.Points.Add(new GeoData.GeoMeshPoint
						{
							Source = new CSMath.XY(sourceX, sourceY),
							Destination = new CSMath.XY(destX, destY)
						});
					}
					return true;
				// Number of Geo-Mesh points
				case 96:
					var nfaces = this._reader.ValueAsInt;
					for (int i = 0; i < nfaces; i++)
					{
						this._reader.ReadNext();
						Debug.Assert(this._reader.Code == 97);
						int index1 = this._reader.ValueAsInt;
						this._reader.ReadNext();
						Debug.Assert(this._reader.Code == 98);
						int index2 = this._reader.ValueAsInt;
						this._reader.ReadNext();
						Debug.Assert(this._reader.Code == 99);
						int index3 = this._reader.ValueAsInt;

						tmp.CadObject.Faces.Add(new GeoData.GeoMeshFace
						{
							Index1 = index1,
							Index2 = index2,
							Index3 = index3
						});
					}
					return true;
				case 303:
					tmp.CadObject.CoordinateSystemDefinition += this._reader.ValueAsString;
					return true;
				//Obsolete codes for version GeoDataVersion.R2009
				case 3:
				case 4:
				case 14:
				case 24:
				case 15:
				case 25:
				case 43:
				case 44:
				case 45:
				case 94:
				case 293:
				case 16:
				case 26:
				case 17:
				case 27:
				case 54:
				case 140:
				case 304:
				case 292:
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
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

		private bool readSpatialFilter(CadTemplate template, DxfMap map)
		{
			CadSpatialFilterTemplate tmp = template as CadSpatialFilterTemplate;
			SpatialFilter filter = tmp.CadObject as SpatialFilter;

			switch (this._reader.Code)
			{
				case 10:
					filter.BoundaryPoints.Add(new CSMath.XY(this._reader.ValueAsDouble, 0));
					return true;
				case 20:
					var pt = filter.BoundaryPoints.LastOrDefault();
					filter.BoundaryPoints.Add(new CSMath.XY(pt.X, this._reader.ValueAsDouble));
					return true;
				case 40:
					if (filter.ClipFrontPlane && !tmp.HasFrontPlane)
					{
						filter.FrontDistance = this._reader.ValueAsDouble;
						tmp.HasFrontPlane = true;
					}

					double[] array = new double[16]
					{
						0.0, 0.0, 0.0, 0.0,
						0.0, 0.0, 0.0, 0.0,
						0.0, 0.0, 0.0, 0.0,
						0.0, 0.0, 0.0, 1.0
					};

					for (int i = 0; i < 12; i++)
					{
						array[i] = this._reader.ValueAsDouble;

						if (i < 11)
						{
							this._reader.ReadNext();
						}
					}

					if (tmp.InsertTransformRead)
					{
						filter.InsertTransform = new Matrix4(array);
						tmp.InsertTransformRead = true;
					}
					else
					{
						filter.InverseInsertTransform = new Matrix4(array);
					}

					return true;
				case 73:
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.SpatialFilter]);
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
