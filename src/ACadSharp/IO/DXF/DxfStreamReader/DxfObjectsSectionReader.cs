using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using CSMath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static ACadSharp.IO.Templates.CadEvaluationGraphTemplate;
using static ACadSharp.IO.Templates.CadTableEntityTemplate;

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
				case DxfFileToken.ObjectPlaceholder:
					return this.readObjectCodes<AcdbPlaceHolder>(new CadNonGraphicalObjectTemplate(new AcdbPlaceHolder()), this.readObjectSubclassMap);
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
				case DxfFileToken.ObjectImageDefinition:
					return this.readObjectCodes<ImageDefinition>(new CadNonGraphicalObjectTemplate(new ImageDefinition()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectDictionaryVar:
					return this.readObjectCodes<DictionaryVariable>(new CadTemplate<DictionaryVariable>(new DictionaryVariable()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectPdfDefinition:
					return this.readObjectCodes<PdfUnderlayDefinition>(new CadNonGraphicalObjectTemplate(new PdfUnderlayDefinition()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectSortEntsTable:
					return this.readSortentsTable();
				case DxfFileToken.ObjectImageDefinitionReactor:
					return this.readObjectCodes<ImageDefinitionReactor>(new CadNonGraphicalObjectTemplate(new ImageDefinitionReactor()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectProxyObject:
					return this.readObjectCodes<ProxyObject>(new CadNonGraphicalObjectTemplate(new ProxyObject()), this.readProxyObject);
				case DxfFileToken.ObjectRasterVariables:
					return this.readObjectCodes<RasterVariables>(new CadNonGraphicalObjectTemplate(new RasterVariables()), this.readObjectSubclassMap);
				case DxfFileToken.ObjectGroup:
					return this.readObjectCodes<Group>(new CadGroupTemplate(), this.readGroup);
				case DxfFileToken.ObjectGeoData:
					return this.readObjectCodes<GeoData>(new CadGeoDataTemplate(), this.readGeoData);
				case DxfFileToken.ObjectMaterial:
					return this.readObjectCodes<Material>(new CadMaterialTemplate(), this.readMaterial);
				case DxfFileToken.ObjectScale:
					return this.readObjectCodes<Scale>(new CadTemplate<Scale>(new Scale()), this.readScale);
				case DxfFileToken.ObjectTableContent:
					return this.readObjectCodes<TableContent>(new CadTableContentTemplate(), this.readTableContent);
				case DxfFileToken.ObjectVisualStyle:
					return this.readObjectCodes<VisualStyle>(new CadTemplate<VisualStyle>(new VisualStyle()), this.readVisualStyle);
				case DxfFileToken.ObjectSpatialFilter:
					return this.readObjectCodes<SpatialFilter>(new CadSpatialFilterTemplate(), this.readSpatialFilter);
				case DxfFileToken.ObjectMLineStyle:
					return this.readObjectCodes<MLineStyle>(new CadMLineStyleTemplate(), this.readMLineStyle);
				case DxfFileToken.ObjectMLeaderStyle:
					return this.readObjectCodes<MultiLeaderStyle>(new CadMLeaderStyleTemplate(), this.readMLeaderStyle);
				case DxfFileToken.ObjectTableStyle:
					return this.readObjectCodes<TableStyle>(new CadTableStyleTemplate(), this.readTableStyle);
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

				if (this.lockPointer)
				{
					this.lockPointer = false;
					continue;
				}

				if (this._reader.DxfCode != DxfCode.Start)
				{
					this._reader.ReadNext();
				}
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

		private bool readMaterial(CadTemplate template, DxfMap map)
		{
			CadMaterialTemplate tmp = template as CadMaterialTemplate;
			List<double> arr = null;

			switch (this._reader.Code)
			{
				case 43:
					arr = new();
					for (int i = 0; i < 16; i++)
					{
						Debug.Assert(this._reader.Code == 43);

						arr.Add(this._reader.ValueAsDouble);

						this._reader.ReadNext();
					}

					tmp.CadObject.DiffuseMatrix = new CSMath.Matrix4(arr.ToArray());
					return this.checkObjectEnd(template, map, this.readMaterial);
				case 47:
					arr = new();
					for (int i = 0; i < 16; i++)
					{
						Debug.Assert(this._reader.Code == 47);

						arr.Add(this._reader.ValueAsDouble);

						this._reader.ReadNext();
					}

					tmp.CadObject.SpecularMatrix = new CSMath.Matrix4(arr.ToArray());
					return this.checkObjectEnd(template, map, this.readMaterial);
				case 49:
					arr = new();
					for (int i = 0; i < 16; i++)
					{
						Debug.Assert(this._reader.Code == 49);

						arr.Add(this._reader.ValueAsDouble);

						this._reader.ReadNext();
					}

					tmp.CadObject.ReflectionMatrix = new CSMath.Matrix4(arr.ToArray());
					return this.checkObjectEnd(template, map, this.readMaterial);
				case 142:
					arr = new();
					for (int i = 0; i < 16; i++)
					{
						Debug.Assert(this._reader.Code == 142);

						arr.Add(this._reader.ValueAsDouble);

						this._reader.ReadNext();
					}

					tmp.CadObject.OpacityMatrix = new CSMath.Matrix4(arr.ToArray());
					return this.checkObjectEnd(template, map, this.readMaterial);
				case 144:
					arr = new();
					for (int i = 0; i < 16; i++)
					{
						Debug.Assert(this._reader.Code == 144);

						arr.Add(this._reader.ValueAsDouble);

						this._reader.ReadNext();
					}

					tmp.CadObject.BumpMatrix = new CSMath.Matrix4(arr.ToArray());
					return this.checkObjectEnd(template, map, this.readMaterial);
				case 147:
					arr = new();
					for (int i = 0; i < 16; i++)
					{
						Debug.Assert(this._reader.Code == 147);

						arr.Add(this._reader.ValueAsDouble);

						this._reader.ReadNext();
					}

					tmp.CadObject.RefractionMatrix = new CSMath.Matrix4(arr.ToArray());
					return this.checkObjectEnd(template, map, this.readMaterial);
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

		private void readLinkedData(CadTemplate template, DxfMap map)
		{
			CadTableContentTemplate tmp = template as CadTableContentTemplate;
			LinkedData linkedData = tmp.CadObject;

			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start && this._reader.DxfCode != DxfCode.Subclass)
			{
				switch (this._reader.Code)
				{
					default:
						if (!this.tryAssignCurrentValue(linkedData, map.SubClasses[DxfSubclassMarker.LinkedData]))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readLinkedData)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				this._reader.ReadNext();
			}
		}

		private bool readTableContent(CadTemplate template, DxfMap map)
		{
			switch (this._reader.Code)
			{
				case 100 when this._reader.ValueAsString.Equals(DxfSubclassMarker.TableContent, StringComparison.InvariantCultureIgnoreCase):
					this.readTableContentSubclass(template, map);
					this.lockPointer = true;
					return true;
				case 100 when this._reader.ValueAsString.Equals(DxfSubclassMarker.FormattedTableData, StringComparison.InvariantCultureIgnoreCase):
					this.readFormattedTableDataSubclass(template, map);
					this.lockPointer = true;
					return true;
				case 100 when this._reader.ValueAsString.Equals(DxfSubclassMarker.LinkedTableData, StringComparison.InvariantCultureIgnoreCase):
					this.readLinkedTableDataSubclass(template, map);
					this.lockPointer = true;
					return true;
				case 100 when this._reader.ValueAsString.Equals(DxfSubclassMarker.LinkedData, StringComparison.InvariantCultureIgnoreCase):
					this.readLinkedData(template, map);
					this.lockPointer = true;
					return true;
				default:
					return false;
			}
		}

		private void readTableContentSubclass(CadTemplate template, DxfMap map)
		{
			CadTableContentTemplate tmp = template as CadTableContentTemplate;
			TableContent tableContent = tmp.CadObject;

			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start && this._reader.DxfCode != DxfCode.Subclass)
			{
				switch (this._reader.Code)
				{
					case 340:
						tmp.SytleHandle = this._reader.ValueAsHandle;
						break;
					default:
						if (!this.tryAssignCurrentValue(tableContent, map.SubClasses[DxfSubclassMarker.TableContent]))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readTableContentSubclass)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				this._reader.ReadNext();
			}
		}

		private void readFormattedTableDataSubclass(CadTemplate template, DxfMap map)
		{
			CadTableContentTemplate tmp = template as CadTableContentTemplate;
			FormattedTableData formattedTable = tmp.CadObject;

			this._reader.ReadNext();

			TableEntity.CellRange cellRange = null;
			while (this._reader.DxfCode != DxfCode.Start && this._reader.DxfCode != DxfCode.Subclass)
			{
				switch (this._reader.Code)
				{
					case 90:
						break;
					case 91:
						if (cellRange == null)
						{
							cellRange = new();
							formattedTable.MergedCellRanges.Add(cellRange);
						}
						cellRange.TopRowIndex = this._reader.ValueAsInt;
						break;
					case 92:
						if (cellRange == null)
						{
							cellRange = new();
							formattedTable.MergedCellRanges.Add(cellRange);
						}
						cellRange.LeftColumnIndex = this._reader.ValueAsInt;
						break;
					case 93:
						if (cellRange == null)
						{
							cellRange = new();
							formattedTable.MergedCellRanges.Add(cellRange);
						}
						cellRange.BottomRowIndex = this._reader.ValueAsInt;
						break;
					case 94:
						if (cellRange == null)
						{
							cellRange = new();
							formattedTable.MergedCellRanges.Add(cellRange);
						}
						cellRange.RightColumnIndex = this._reader.ValueAsInt;
						cellRange = null;
						break;
					case 300 when this._reader.ValueAsString.Equals("TABLEFORMAT", StringComparison.InvariantCultureIgnoreCase):
						this.readStyleOverride(new CadCellStyleTemplate(formattedTable.CellStyleOverride));
						break;
					default:
						if (!this.tryAssignCurrentValue(formattedTable, map.SubClasses[DxfSubclassMarker.FormattedTableData]))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readFormattedTableDataSubclass)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				this._reader.ReadNext();
			}
		}

		private void readLinkedTableDataSubclass(CadTemplate template, DxfMap map)
		{
			CadTableContentTemplate tmp = template as CadTableContentTemplate;
			TableContent tableContent = tmp.CadObject;

			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start && this._reader.DxfCode != DxfCode.Subclass)
			{
				switch (this._reader.Code)
				{
					case 90:
						//Column count
						break;
					case 91:
						//Row count
						break;
					//Unknown
					case 92:
						break;
					case 300 when this._reader.ValueAsString.Equals(DxfFileToken.ObjectTableColumn, StringComparison.InvariantCultureIgnoreCase):
						//Read Column
						this.readTableColumn();
						break;
					case 301 when this._reader.ValueAsString.Equals(DxfFileToken.ObjectTableRow, StringComparison.InvariantCultureIgnoreCase):
						//Read Row
						this.readTableRow();
						break;
					default:
						if (!this.tryAssignCurrentValue(tableContent, map.SubClasses[DxfSubclassMarker.LinkedTableData]))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readLinkedTableDataSubclass)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				this._reader.ReadNext();
			}
		}


		private TableEntity.Column readTableColumn()
		{
			this._reader.ReadNext();

			TableEntity.Column column = new TableEntity.Column();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.LinkedTableDataColumn_BEGIN, StringComparison.InvariantCultureIgnoreCase):
						this.readLinkedTableColumn(column);
						break;
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.FormattedTableDataColumn_BEGIN, StringComparison.InvariantCultureIgnoreCase):
						this.readFormattedTableColumn(column);
						break;
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.ObjectTableColumnBegin, StringComparison.InvariantCultureIgnoreCase):
						this.readTableColumn(column);
						end = true;
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readTableColumn)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					return column;
				}

				this._reader.ReadNext();
			}

			return column;
		}

		private TableEntity.Row readTableRow()
		{
			this._reader.ReadNext();

			TableEntity.Row row = new TableEntity.Row();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.LinkedTableDataRow_BEGIN, StringComparison.InvariantCultureIgnoreCase):
						this.readLinkedTableRow(row);
						break;
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.FormattedTableDataRow_BEGIN, StringComparison.InvariantCultureIgnoreCase):
						this.readFormattedTableRow(row);
						break;
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.ObjectTableRowBegin, StringComparison.InvariantCultureIgnoreCase):
						this.readTableRow(row);
						end = true;
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readTableRow)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					return row;
				}

				this._reader.ReadNext();
			}

			return row;
		}

		private void readTableRow(TableEntity.Row row)
		{
			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 40:
						row.Height = this._reader.ValueAsDouble;
						break;
					case 90:
						//styleID
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("TABLEROW_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readTableRow)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readFormattedTableRow(TableEntity.Row row)
		{
			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 300 when this._reader.ValueAsString.Equals("ROWTABLEFORMAT", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 1 when this._reader.ValueAsString.Equals("TABLEFORMAT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readStyleOverride(new CadCellStyleTemplate(row.CellStyleOverride));
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("FORMATTEDTABLEDATAROW_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readFormattedTableRow)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readTableColumn(TableEntity.Column column)
		{
			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("TABLECOLUMN_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 1:
						end = true;
						break;
					case 40:
						column.Width = this._reader.ValueAsDouble;
						break;
					case 90:
						//StyleId
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("TABLECOLUMN_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readTableColumn)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readLinkedTableColumn(TableEntity.Column column)
		{
			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.LinkedTableDataColumn_BEGIN, StringComparison.InvariantCultureIgnoreCase):
						break;
					case 1:
						end = true;
						break;
					case 91:
						column.CustomData = this._reader.ValueAsInt;
						break;
					case 300:
						column.Name = this._reader.ValueAsString;
						break;
					case 301 when this._reader.ValueAsString.Equals(DxfFileToken.CustomData, StringComparison.InvariantCultureIgnoreCase):
						this.readCustomData();
						break;
					case 309:
						end = this._reader.ValueAsString.Equals(DxfFileToken.LinkedTableDataColumn_END, StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readLinkedTableColumn)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readLinkedTableRow(TableEntity.Row row)
		{
			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals(DxfFileToken.LinkedTableDataRow_BEGIN, StringComparison.InvariantCultureIgnoreCase):
						break;
					case 90:
						break;
					case 91:
						row.CustomData = this._reader.ValueAsInt;
						break;
					case 300 when this._reader.ValueAsString.Equals(DxfFileToken.ObjectCell, StringComparison.InvariantCultureIgnoreCase):
						this.readCell();
						break;
					case 301 when this._reader.ValueAsString.Equals(DxfFileToken.CustomData, StringComparison.InvariantCultureIgnoreCase):
						this.readCustomData();
						break;
					case 309:
						end = this._reader.ValueAsString.Equals(DxfFileToken.LinkedTableDataRow_END, StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readLinkedTableRow)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private TableEntity.Cell readCell()
		{
			this._reader.ReadNext();

			TableEntity.Cell cell = new TableEntity.Cell();
			CadTableCellTemplate template = new CadTableCellTemplate(cell);

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("LINKEDTABLEDATACELL_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readLinkedTableCell(cell);
						break;
					case 1 when this._reader.ValueAsString.Equals("FORMATTEDTABLEDATACELL_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readFormattedTableCell(cell);
						break;
					case 1 when this._reader.ValueAsString.Equals("TABLECELL_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readTableCell(cell);
						end = true;
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readCell)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					return cell;
				}

				this._reader.ReadNext();
			}

			return cell;
		}

		private void readTableCell(TableEntity.Cell cell)
		{
			var map = DxfClassMap.Create(cell.GetType(), "TABLECELL_BEGIN");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					//Unknown
					case 40:
					case 41:
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("TABLECELL_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					case 330:
						//Unknown handle
						break;
					default:
						if (!this.tryAssignCurrentValue(cell, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readTableCell)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readFormattedTableCell(TableEntity.Cell cell)
		{
			var map = DxfClassMap.Create(cell.GetType(), "FORMATTEDTABLEDATACELL_BEGIN");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 300 when this._reader.ValueAsString.Equals("CELLTABLEFORMAT", StringComparison.InvariantCultureIgnoreCase):
						this.readCellTableFormat(cell);
						continue;
					case 309:
						end = this._reader.ValueAsString.Equals("FORMATTEDTABLEDATACELL_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						if (!this.tryAssignCurrentValue(cell, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readFormattedTableCell)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readCellTableFormat(TableEntity.Cell cell)
		{
			var map = DxfClassMap.Create(cell.GetType(), "CELLTABLEFORMAT");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.Code == 1)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("TABLEFORMAT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readStyleOverride(new CadCellStyleTemplate(cell.StyleOverride));
						break;
					case 1 when this._reader.ValueAsString.Equals("CELLSTYLE_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readCellStyle(new CadCellStyleTemplate());
						break;
					default:
						if (!this.tryAssignCurrentValue(cell, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readCellTableFormat)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readCellStyle(CadCellStyleTemplate template)
		{
			//var map = DxfClassMap.Create(cell.GetType(), "CELLTABLEFORMAT");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.Code != 1)
			{
				switch (this._reader.Code)
				{
					case 309:
						end = this._reader.ValueAsString.Equals("CELLSTYLE_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						//if (!this.tryAssignCurrentValue(cell, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readCellStyle)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readLinkedTableCell(TableEntity.Cell cell)
		{
			var map = DxfClassMap.Create(cell.GetType(), "LINKEDTABLEDATACELL_BEGIN");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 95:
						//BL 95 Number of cell contents
						break;
					case 301 when this._reader.ValueAsString.Equals(DxfFileToken.CustomData, StringComparison.InvariantCultureIgnoreCase):
						this.readCustomData();
						break;
					case 302 when this._reader.ValueAsString.Equals("CONTENT", StringComparison.InvariantCultureIgnoreCase):
						var c = this.readLinkedTableCellContent();
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("LINKEDTABLEDATACELL_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						if (!this.tryAssignCurrentValue(cell, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readLinkedTableCell)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private CadTableCellContentTemplate readLinkedTableCellContent()
		{
			TableEntity.CellContent content = new TableEntity.CellContent();
			CadTableCellContentTemplate template = new CadTableCellContentTemplate(content);
			var map = DxfClassMap.Create(content.GetType(), "CONTENT");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("FORMATTEDCELLCONTENT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						readFormattedCellContent();
						end = true;
						break;
					case 1 when this._reader.ValueAsString.Equals("CELLCONTENT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						readCellContent(template);
						break;
					default:
						if (!this.tryAssignCurrentValue(content, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readLinkedTableCellContent)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}

			return template;
		}

		private void readCellContent(CadTableCellContentTemplate template)
		{
			TableEntity.CellContent content = template.Content;
			var map = DxfClassMap.Create(content.GetType(), "CELLCONTENT_BEGIN");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 91:
						break;
					case 300 when this._reader.ValueAsString.Equals("VALUE", StringComparison.InvariantCultureIgnoreCase):
						this.readDataMapValue();
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("CELLCONTENT_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					case 340:
						template.BlockRecordHandle = this._reader.ValueAsHandle;
						break;
					default:
						if (!this.tryAssignCurrentValue(content, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readCellContent)} {this._reader.Position}.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readFormattedCellContent()
		{
			TableEntity.ContentFormat format = new();
			CadTableCellContentFormatTemplate template = new CadTableCellContentFormatTemplate(format);
			var map = DxfClassMap.Create(format.GetType(), "FORMATTEDCELLCONTENT");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 300 when this._reader.ValueAsString.Equals("CONTENTFORMAT", StringComparison.InvariantCultureIgnoreCase):
						readContentFormat(template);
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("FORMATTEDCELLCONTENT_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						if (!this.tryAssignCurrentValue(format, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readFormattedCellContent)} method.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readContentFormat(CadTableCellContentFormatTemplate template)
		{
			var format = template.Format;
			var map = DxfClassMap.Create(format.GetType(), "CONTENTFORMAT_BEGIN");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("CONTENTFORMAT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("CONTENTFORMAT_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					case 340:
						template.TextStyleHandle = this._reader.ValueAsHandle;
						break;
					default:
						if (!this.tryAssignCurrentValue(format, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readContentFormat)} method.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readFormattedTableColumn(TableEntity.Column column)
		{
			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 300 when this._reader.ValueAsString.Equals("COLUMNTABLEFORMAT", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 1 when this._reader.ValueAsString.Equals("TABLEFORMAT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readStyleOverride(new CadCellStyleTemplate(column.CellStyleOverride));
						break;
					case 309:
						end = this._reader.ValueAsString.Equals(DxfFileToken.FormattedTableDataColumn_END, StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readFormattedTableColumn)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readStyleOverride(CadCellStyleTemplate template)
		{
			var style = template.Format as TableEntity.CellStyle;
			var mapstyle = DxfClassMap.Create(style.GetType(), "TABLEFORMAT_STYLE");
			var mapformat = DxfClassMap.Create(typeof(TableEntity.ContentFormat), "TABLEFORMAT_BEGIN");

			this._reader.ReadNext();

			bool end = false;
			TableEntity.CellEdgeFlags currBorder = TableEntity.CellEdgeFlags.Unknown;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 95:
						currBorder = (TableEntity.CellEdgeFlags)this._reader.ValueAsInt;
						break;
					case 1 when this._reader.ValueAsString.Equals("TABLEFORMAT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 300 when this._reader.ValueAsString.Equals("CONTENTFORMAT", StringComparison.InvariantCultureIgnoreCase):
						readContentFormat(new CadTableCellContentFormatTemplate(new TableEntity.ContentFormat()));
						break;
					case 301 when this._reader.ValueAsString.Equals("MARGIN", StringComparison.InvariantCultureIgnoreCase):
						this.readCellMargin(template);
						break;
					case 302 when this._reader.ValueAsString.Equals("GRIDFORMAT", StringComparison.InvariantCultureIgnoreCase):
						TableEntity.CellBorder border = new TableEntity.CellBorder(currBorder);
						this.readGridFormat(template, border);
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("TABLEFORMAT_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						if (!this.tryAssignCurrentValue(style, mapstyle) && !this.tryAssignCurrentValue(style, mapformat))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readStyleOverride)} method.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readGridFormat(CadCellStyleTemplate template, TableEntity.CellBorder border)
		{
			var map = DxfClassMap.Create(border.GetType(), nameof(TableEntity.CellBorder));

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("GRIDFORMAT_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 62:
						border.Color = new Color(this._reader.ValueAsShort);
						break;
					case 92:
						border.LineWeight = (LineWeightType)this._reader.ValueAsInt;
						break;
					case 93:
						border.IsInvisible = this._reader.ValueAsBool;
						break;
					case 340:
						template.BorderLinetypePairs.Add(new Tuple<TableEntity.CellBorder, ulong>(border, this._reader.ValueAsHandle));
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("GRIDFORMAT_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						if (!this.tryAssignCurrentValue(border, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readGridFormat)} method.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readCellMargin(CadCellStyleTemplate template)
		{
			var style = template.Format as TableEntity.CellStyle;

			this._reader.ReadNext();

			bool end = false;
			int i = 0;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("CELLMARGIN_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 40:
						switch (i)
						{
							case 0:
								style.VerticalMargin = this._reader.ValueAsDouble;
								break;
							case 1:
								style.HorizontalMargin = this._reader.ValueAsDouble;
								break;
							case 2:
								style.BottomMargin = this._reader.ValueAsDouble;
								break;
							case 3:
								style.RightMargin = this._reader.ValueAsDouble;
								break;
							case 4:
								style.MarginHorizontalSpacing = this._reader.ValueAsDouble;
								break;
							case 5:
								style.MarginVerticalSpacing = this._reader.ValueAsDouble;
								break;
						}

						i++;
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("CELLMARGIN_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readCellMargin)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readCustomData()
		{
			this._reader.ReadNext();

			int ndata = 0;
			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 1 when this._reader.ValueAsString.Equals("DATAMAP_BEGIN", StringComparison.InvariantCultureIgnoreCase):
						break;
					case 90:
						ndata = this._reader.ValueAsInt;
						break;
					case 300:
						//Name
						break;
					case 301 when this._reader.ValueAsString.Equals("DATAMAP_VALUE", StringComparison.InvariantCultureIgnoreCase):
						this.readDataMapValue();
						break;
					case 309:
						end = this._reader.ValueAsString.Equals("DATAMAP_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readCustomData)} method.", NotificationType.None);
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
			}
		}

		private void readDataMapValue()
		{
			TableEntity.CellValue value = new TableEntity.CellValue();
			var map = DxfClassMap.Create(value.GetType(), "DATAMAP_VALUE");

			this._reader.ReadNext();

			bool end = false;
			while (this._reader.DxfCode != DxfCode.Start)
			{
				switch (this._reader.Code)
				{
					case 11:
					case 21:
					case 31:
						//Value as point
						break;
					case 91:
					case 92:
						//Value as int
						break;
					case 140:
						//Value as double
						break;
					case 310:
						//Value as byte array
						break;
					case 304:
						end = this._reader.ValueAsString.Equals("ACVALUE_END", StringComparison.InvariantCultureIgnoreCase);
						break;
					default:
						if (!this.tryAssignCurrentValue(value, map))
						{
							this._builder.Notify($"Unhandled dxf code {this._reader.Code} value {this._reader.ValueAsString} at {nameof(readDataMapValue)} method.", NotificationType.None);
						}
						break;
				}

				if (end)
				{
					break;
				}

				this._reader.ReadNext();
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
					//Avoid noise while is not implemented
					return true;
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

		private bool readMLineStyle(CadTemplate template, DxfMap map)
		{
			var tmp = template as CadMLineStyleTemplate;
			var mLineStyle = template.CadObject as MLineStyle;

			switch (this._reader.Code)
			{
				case 6:
					var t = tmp.ElementTemplates.LastOrDefault();
					if (t == null)
					{
						return true;
					}
					t.LineTypeName = this._reader.ValueAsString;
					return true;
				case 49:
					MLineStyle.Element element = new MLineStyle.Element();
					CadMLineStyleTemplate.ElementTemplate elementTemplate = new CadMLineStyleTemplate.ElementTemplate(element);
					element.Offset = this._reader.ValueAsDouble;

					tmp.ElementTemplates.Add(elementTemplate);
					mLineStyle.AddElement(element);
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readTableStyle(CadTemplate template, DxfMap map)
		{
			var tmp = template as CadTableStyleTemplate;
			var style = tmp.CadObject;
			var cellStyle = tmp.CurrentCellStyleTemplate?.CellStyle;

			switch (this._reader.Code)
			{
				case 7:
					tmp.CreateCurrentCellStyleTemplate();
					tmp.CurrentCellStyleTemplate.TextStyleName = this._reader.ValueAsString;
					return true;
				case 94:
					cellStyle.Alignment = this._reader.ValueAsInt;
					return true;
				case 62:
					cellStyle.Color = new Color(this._reader.ValueAsShort);
					return true;
				case 63:
					cellStyle.BackgroundColor = new Color(this._reader.ValueAsShort);
					return true;
				case 140:
					cellStyle.TextHeight = this._reader.ValueAsDouble;
					return true;
				case 170:
					cellStyle.CellAlignment = (TableEntity.Cell.CellAlignmentType)this._reader.ValueAsShort;
					return true;
				case 283:
					cellStyle.IsFillColorOn = this._reader.ValueAsBool;
					return true;
				case 90:
					cellStyle.Type = (TableEntity.CellStyleType)this._reader.ValueAsShort;
					return true;
				case 91:
					cellStyle.StyleClass = (TableEntity.CellStyleClass)this._reader.ValueAsShort;
					return true;
				case 1:
					//Undocumented
					return true;
				case 274:
					cellStyle.TopBorder.LineWeight = (LineWeightType)this._reader.ValueAsInt;
					return true;
				case 275:
					cellStyle.HorizontalInsideBorder.LineWeight = (LineWeightType)this._reader.ValueAsInt;
					return true;
				case 276:
					cellStyle.BottomBorder.LineWeight = (LineWeightType)this._reader.ValueAsInt;
					return true;
				case 277:
					cellStyle.LeftBorder.LineWeight = (LineWeightType)this._reader.ValueAsInt;
					return true;
				case 278:
					cellStyle.VerticalInsideBorder.LineWeight = (LineWeightType)this._reader.ValueAsInt;
					return true;
				case 279:
					cellStyle.RightBorder.LineWeight = (LineWeightType)this._reader.ValueAsInt;
					return true;
				case 284:
					cellStyle.TopBorder.IsInvisible = this._reader.ValueAsBool;
					return true;
				case 285:
					cellStyle.HorizontalInsideBorder.IsInvisible = this._reader.ValueAsBool;
					return true;
				case 286:
					cellStyle.BottomBorder.IsInvisible = this._reader.ValueAsBool;
					return true;
				case 287:
					cellStyle.LeftBorder.IsInvisible = this._reader.ValueAsBool;
					return true;
				case 288:
					cellStyle.VerticalInsideBorder.IsInvisible = this._reader.ValueAsBool;
					return true;
				case 289:
					cellStyle.RightBorder.IsInvisible = this._reader.ValueAsBool;
					return true;
				case 64:
					cellStyle.TopBorder.Color = new Color(this._reader.ValueAsShort);
					return true;
				case 65:
					cellStyle.HorizontalInsideBorder.Color = new Color(this._reader.ValueAsShort);
					return true;
				case 66:
					cellStyle.BottomBorder.Color = new Color(this._reader.ValueAsShort);
					return true;
				case 67:
					cellStyle.LeftBorder.Color = new Color(this._reader.ValueAsShort);
					return true;
				case 68:
					cellStyle.VerticalInsideBorder.Color = new Color(this._reader.ValueAsShort);
					return true;
				case 69:
					cellStyle.RightBorder.Color = new Color(this._reader.ValueAsShort);
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readMLeaderStyle(CadTemplate template, DxfMap map)
		{
			var tmp = template as CadMLeaderStyleTemplate;

			switch (this._reader.Code)
			{
				case 179:
					return true;
				case 340:
					tmp.LeaderLineTypeHandle = this._reader.ValueAsHandle;
					return true;
				case 342:
					tmp.MTextStyleHandle = this._reader.ValueAsHandle;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
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
					case GroupCodeValueType.Point3D:
						var code = this._reader.Code;
						var x = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						var y = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						var z = this._reader.ValueAsDouble;
						XYZ pt = new XYZ(x, y, z);
						template.CadObject.CreateEntry(code, pt);
						break;
					case GroupCodeValueType.Handle:
					case GroupCodeValueType.ObjectId:
					case GroupCodeValueType.ExtendedDataHandle:
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
