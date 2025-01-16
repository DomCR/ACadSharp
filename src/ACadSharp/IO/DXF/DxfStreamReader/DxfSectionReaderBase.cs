using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.XData;
using CSMath;
using CSUtilities.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfSectionReaderBase
	{
		public delegate bool ReadEntityDelegate<T>(CadEntityTemplate template, DxfMap map, string subclass = null) where T : Entity;

		protected readonly IDxfStreamReader _reader;
		protected readonly DxfDocumentBuilder _builder;

		public DxfSectionReaderBase(IDxfStreamReader reader, DxfDocumentBuilder builder)
		{
			this._reader = reader;
			this._builder = builder;
		}

		public abstract void Read();

		protected void readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle, out ulong? xdictHandle, out List<ulong> reactors)
		{
			name = null;
			handle = 0;
			ownerHandle = null;
			xdictHandle = null;
			reactors = new List<ulong>();

			if (this._reader.DxfCode == DxfCode.Start
					|| this._reader.DxfCode == DxfCode.Subclass)
				this._reader.ReadNext();

			//Loop until the common data end
			while (this._reader.DxfCode != DxfCode.Start
					&& this._reader.DxfCode != DxfCode.Subclass)
			{
				switch (this._reader.Code)
				{
					//Table name
					case 2:
						name = this._reader.ValueAsString;
						break;
					//Handle
					case 5:
					case 105:
						handle = this._reader.ValueAsHandle;
						break;
					//Start of application - defined group
					case 102:
						this.readDefinedGroups(out xdictHandle, out reactors);
						break;
					//Soft - pointer ID / handle to owner BLOCK_RECORD object
					case 330:
						ownerHandle = this._reader.ValueAsHandle;
						break;
					case 71:
					//Number of entries for dimension style table
					case 340:
					//Dimension table has the handles of the styles at the begining
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} at line {this._reader.Position}.");
						break;
				}

				this._reader.ReadNext();
			}
		}

		[Obsolete("Only needed for SortEntitiesTable but it should be removed")]
		protected void readCommonObjectData(CadTemplate template)
		{
			while (this._reader.DxfCode != DxfCode.Subclass)
			{
				switch (this._reader.Code)
				{
					//object name
					case 0:
						Debug.Assert(template.CadObject.ObjectName == this._reader.ValueAsString);
						break;
					//Handle
					case 5:
						template.CadObject.Handle = this._reader.ValueAsHandle;
						break;
					//Start of application - defined group
					case 102:
						this.readDefinedGroups(template);
						break;
					//Soft - pointer ID / handle to owner BLOCK_RECORD object
					case 330:
						template.OwnerHandle = this._reader.ValueAsHandle;
						break;
					default:
						this._builder.Notify($"Unhandled dxf code {this._reader.Code} at line {this._reader.Position}.", NotificationType.None);
						break;
				}

				this._reader.ReadNext();
			}
		}

		protected void readCommonCodes(CadTemplate template, out bool isExtendedData, DxfMap map = null)
		{
			isExtendedData = false;

			switch (this._reader.Code)
			{
				//Handle
				case 5:
					template.CadObject.Handle = this._reader.ValueAsHandle;
					break;
				//Check with mapper
				case 100:
					if (map != null && !map.SubClasses.ContainsKey(this._reader.ValueAsString))
						this._builder.Notify($"[{template.CadObject.ObjectName}] Unidentified subclass {this._reader.ValueAsString}", NotificationType.Warning);
					break;
				//Start of application - defined group
				case 102:
					this.readDefinedGroups(template);
					break;
				//Soft - pointer ID / handle to owner BLOCK_RECORD object
				case 330:
					template.OwnerHandle = this._reader.ValueAsHandle;
					break;
				case 1001:
					isExtendedData = true;
					this.readExtendedData(template.EDataTemplateByAppName);
					break;
				default:
					this._builder.Notify($"[{template.CadObject.SubclassMarker}] Unhandled dxf code {this._reader.Code} with value {this._reader.ValueAsString}", NotificationType.None);
					break;
			}
		}

		protected CadEntityTemplate readEntity()
		{
			switch (this._reader.ValueAsString)
			{
				case DxfFileToken.EntityAttribute:
					return this.readEntityCodes<AttributeEntity>(new CadAttributeTemplate(new AttributeEntity()), this.readAttributeDefinition);
				case DxfFileToken.EntityAttributeDefinition:
					return this.readEntityCodes<AttributeDefinition>(new CadAttributeTemplate(new AttributeDefinition()), this.readAttributeDefinition);
				case DxfFileToken.EntityArc:
					return this.readEntityCodes<Arc>(new CadEntityTemplate<Arc>(), this.readArc);
				case DxfFileToken.EntityCircle:
					return this.readEntityCodes<Circle>(new CadEntityTemplate<Circle>(), this.readEntitySubclassMap);
				case DxfFileToken.EntityDimension:
					return this.readEntityCodes<Dimension>(new CadDimensionTemplate(), this.readDimension);
				case DxfFileToken.Entity3DFace:
					return this.readEntityCodes<Face3D>(new CadEntityTemplate<Face3D>(), this.readEntitySubclassMap);
				case DxfFileToken.EntityEllipse:
					return this.readEntityCodes<Ellipse>(new CadEntityTemplate<Ellipse>(), this.readEntitySubclassMap);
				case DxfFileToken.EntityLeader:
					return this.readEntityCodes<Leader>(new CadLeaderTemplate(), this.readLeader);
				case DxfFileToken.EntityLine:
					return this.readEntityCodes<Line>(new CadEntityTemplate<Line>(), this.readEntitySubclassMap);
				case DxfFileToken.EntityLwPolyline:
					return this.readEntityCodes<LwPolyline>(new CadEntityTemplate<LwPolyline>(), this.readLwPolyline);
				case DxfFileToken.EntityMesh:
					return this.readEntityCodes<Mesh>(new CadMeshTemplate(), this.readMesh);
				case DxfFileToken.EntityHatch:
					return this.readEntityCodes<Hatch>(new CadHatchTemplate(), this.readHatch);
				case DxfFileToken.EntityInsert:
					return this.readEntityCodes<Insert>(new CadInsertTemplate(), this.readInsert);
				case DxfFileToken.EntityMText:
					return this.readEntityCodes<MText>(new CadTextEntityTemplate(new MText()), this.readTextEntity);
				case DxfFileToken.EntityMLine:
					return this.readEntityCodes<MLine>(new CadMLineTemplate(), this.readMLine);
				case DxfFileToken.EntityPdfUnderlay:
					return this.readEntityCodes<PdfUnderlay>(new CadPdfUnderlayTemplate(), this.readUnderlayEntity);
				case DxfFileToken.EntityPoint:
					return this.readEntityCodes<Point>(new CadEntityTemplate<Point>(), this.readEntitySubclassMap);
				case DxfFileToken.EntityPolyline:
					return this.readPolyline();
				case DxfFileToken.EntityRay:
					return this.readEntityCodes<Ray>(new CadEntityTemplate<Ray>(), this.readEntitySubclassMap);
				case DxfFileToken.EndSequence:
					return this.readEntityCodes<Seqend>(new CadEntityTemplate<Seqend>(), this.readEntitySubclassMap);
				case DxfFileToken.EntitySolid:
					return this.readEntityCodes<Solid>(new CadEntityTemplate<Solid>(), this.readEntitySubclassMap);
				case DxfFileToken.EntityTable:
					return this.readEntityCodes<TableEntity>(new CadTableEntityTemplate(), this.readTableEntity);
				case DxfFileToken.EntityText:
					return this.readEntityCodes<TextEntity>(new CadTextEntityTemplate(new TextEntity()), this.readTextEntity);
				case DxfFileToken.EntityTolerance:
					return this.readEntityCodes<Tolerance>(new CadToleranceTemplate(new Tolerance()), this.readTolerance);
				case DxfFileToken.EntityVertex:
					return this.readEntityCodes<Entity>(new CadVertexTemplate(), this.readVertex);
				case DxfFileToken.EntityViewport:
					return this.readEntityCodes<Viewport>(new CadViewportTemplate(), this.readViewport);
				case DxfFileToken.EntityShape:
					return this.readEntityCodes<Shape>(new CadShapeTemplate(new Shape()), this.readShape);
				case DxfFileToken.EntitySpline:
					return this.readEntityCodes<Spline>(new CadSplineTemplate(), this.readSpline);
				case DxfFileToken.EntityXline:
					return this.readEntityCodes<XLine>(new CadEntityTemplate<XLine>(), this.readEntitySubclassMap);
				default:
					DxfMap map = DxfMap.Create<Entity>();
					CadUnknownEntityTemplate unknownEntityTemplate = null;
					if (this._builder.DocumentToBuild.Classes.TryGetByName(this._reader.ValueAsString, out Classes.DxfClass dxfClass))
					{
						this._builder.Notify($"Entity not supported read as an UnknownEntity: {this._reader.ValueAsString}", NotificationType.NotImplemented);
						unknownEntityTemplate = new CadUnknownEntityTemplate(new UnknownEntity(dxfClass));
					}
					else
					{
						this._builder.Notify($"Entity not supported: {this._reader.ValueAsString}", NotificationType.NotImplemented);
					}

					this._reader.ReadNext();

					do
					{
						if (unknownEntityTemplate != null && this._builder.KeepUnknownEntities)
						{
							this.readCommonEntityCodes(unknownEntityTemplate, out bool isExtendedData, map);
							if (isExtendedData)
								continue;
						}

						this._reader.ReadNext();
					}
					while (this._reader.DxfCode != DxfCode.Start);

					return unknownEntityTemplate;
			}
		}

		protected CadEntityTemplate readEntityCodes<T>(CadEntityTemplate template, ReadEntityDelegate<T> readEntity)
			where T : Entity
		{
			this._reader.ReadNext();

			DxfMap map = DxfMap.Create<T>();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				if (!readEntity(template, map))
				{
					this.readCommonEntityCodes(template, out bool isExtendedData, map);
					if (isExtendedData)
						continue;
				}

				if (this._reader.DxfCode != DxfCode.Start)
					this._reader.ReadNext();
			}

			return template;
		}

		protected void readCommonEntityCodes(CadEntityTemplate template, out bool isExtendedData, DxfMap map = null)
		{
			isExtendedData = false;
			switch (this._reader.Code)
			{
				case 6:
					template.LineTypeName = this._reader.ValueAsString;
					break;
				case 8:
					template.LayerName = this._reader.ValueAsString;
					break;
				//Absent or zero indicates entity is in model space. 1 indicates entity is in paper space (optional).
				case 67:
					break;
				//Number of bytes Proxy entity graphics data
				case 92:
				case 160:
				//Proxy entity graphics data
				case 310:
					break;
				case 347:
					template.MaterialHandle = this._reader.ValueAsHandle;
					break;
				case 430:
					template.BookColorName = this._reader.ValueAsString;
					break;
				default:
					if (!this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Entity]))
					{
						this.readCommonCodes(template, out isExtendedData, map);
					}
					break;
			}
		}

		private bool readArc(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			switch (this._reader.Code)
			{
				default:
					if (!this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Arc]))
					{
						return this.readEntitySubclassMap(template, map, DxfSubclassMarker.Circle);
					}
					return true;
			}
		}

		private bool readAttributeDefinition(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			DxfClassMap emap = map.SubClasses[template.CadObject.SubclassMarker];
			CadAttributeTemplate tmp = template as CadAttributeTemplate;

			switch (this._reader.Code)
			{
				case 44:
				case 46:
					return true;
				case 101:
					var att = tmp.CadObject as AttributeBase;
					att.MText = new MText();
					CadTextEntityTemplate mtextTemplate = new CadTextEntityTemplate(att.MText);
					tmp.MTextTemplate = mtextTemplate;
					this.readEntityCodes<MText>(mtextTemplate, this.readTextEntity);
					return true;
				default:
					if (!this.tryAssignCurrentValue(template.CadObject, emap))
					{
						return this.readTextEntity(template, map, DxfSubclassMarker.Text);
					}
					return true;
			}
		}

		private bool readTableEntity(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			string mapName = string.IsNullOrEmpty(subclass) ? template.CadObject.SubclassMarker : subclass;
			CadTableEntityTemplate tmp = template as CadTableEntityTemplate;
			TableEntity table = tmp.CadObject as TableEntity;

			switch (this._reader.Code)
			{
				case 2:
					tmp.BlockName = this._reader.ValueAsString;
					return true;
				case 342:
					tmp.StyleHandle = this._reader.ValueAsHandle;
					return true;
				case 343:
					tmp.BlockOwnerHandle = this._reader.ValueAsHandle;
					return true;
				case 141:
					var row = new TableEntity.Row();
					row.Height = this._reader.ValueAsDouble;
					table.Rows.Add(row);
					return true;
				case 142:
					var col = new TableEntity.Column();
					col.Width = this._reader.ValueAsDouble;
					table.Columns.Add(col);
					return true;
				case 144:
					tmp.CurrentCellTemplate.FormatTextHeight = this._reader.ValueAsDouble;
					return true;
				case 145:
					tmp.CurrentCell.Rotation = this._reader.ValueAsDouble;
					return true;
				case 170:
					//Has data flag
					return true;
				case 171:
					tmp.CreateCell((TableEntity.CellType)this._reader.ValueAsInt);
					return true;
				case 172:
					tmp.CurrentCell.FlagValue = this._reader.ValueAsInt;
					return true;
				case 173:
					tmp.CurrentCell.MergedValue = this._reader.ValueAsInt;
					return true;
				case 174:
					tmp.CurrentCell.Autofit = this._reader.ValueAsBool;
					return true;
				case 175:
					tmp.CurrentCell.BorderWidth = this._reader.ValueAsInt;
					return true;
				case 176:
					tmp.CurrentCell.BorderHeight = this._reader.ValueAsInt;
					return true;
				case 178:
					tmp.CurrentCell.VirtualEdgeFlag = this._reader.ValueAsShort;
					return true;
				case 179:
					//Unknown value
					return true;
				case 301:
					var content = new TableEntity.CellContent();
					tmp.CurrentCell.Contents.Add(content);
					this.readCellValue(content);
					return true;
				case 340:
					tmp.CurrentCellTemplate.BlockRecordHandle = this._reader.ValueAsHandle;
					return true;
				default:
					if (!this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Insert]))
					{
						return this.readEntitySubclassMap(template, map, DxfSubclassMarker.TableEntity);
					}
					return true;
			}
		}

		private void readCellValue(TableEntity.CellContent content)
		{
			if (this._reader.ValueAsString.Equals("CELL_VALUE", StringComparison.OrdinalIgnoreCase))
			{
				this._reader.ReadNext();
			}
			else
			{
				throw new Exceptions.DxfException($"Expected value not found CELL_VALUE", this._reader.Position);
			}

			while (this._reader.Code != 304
				&& !this._reader.ValueAsString.Equals("ACVALUE_END", StringComparison.OrdinalIgnoreCase))
			{
				switch (this._reader.Code)
				{
					case 1:
						content.Value.Text = this._reader.ValueAsString;
						break;
					case 2:
						content.Value.Text += this._reader.ValueAsString;
						break;
					case 11:
						content.Value.Value = new XYZ(this._reader.ValueAsDouble, 0, 0);
						break;
					case 21:
						content.Value.Value = new XYZ(0, this._reader.ValueAsDouble, 0);
						break;
					case 31:
						content.Value.Value = new XYZ(0, 0, this._reader.ValueAsDouble);
						break;
					case 302:
						//TODO: Fix this assignation to cell value
						content.Value.Value = this._reader.ValueAsString;
						break;
					case 90:
						content.Value.ValueType = (TableEntity.CellValueType)this._reader.ValueAsInt;
						break;
					case 91:
						content.Value.Value = this._reader.ValueAsInt;
						break;
					case 93:
						content.Value.Flags = this._reader.ValueAsInt;
						break;
					case 94:
						content.Value.Units = (TableEntity.ValueUnitType)this._reader.ValueAsInt;
						break;
					case 140:
						content.Value.Value = this._reader.ValueAsDouble;
						break;
					case 300:
						content.Value.Format = this._reader.ValueAsString;
						break;
					default:
						this._builder.Notify($"[CELL_VALUE] Unhandled dxf code {this._reader.Code} with value {this._reader.ValueAsString}", NotificationType.None);
						break;
				}

				this._reader.ReadNext();
			}
		}

		private bool readTextEntity(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			string mapName = string.IsNullOrEmpty(subclass) ? template.CadObject.SubclassMarker : subclass;
			CadTextEntityTemplate tmp = template as CadTextEntityTemplate;

			switch (this._reader.Code)
			{
				//TODO: Implement multiline text def codes
				case 1 or 3 when tmp.CadObject is MText mtext:
					mtext.Value += this._reader.ValueAsString;
					return true;
				case 70:
				case 74:
				case 101:
					return true;
				case 7:
					tmp.StyleName = this._reader.ValueAsString;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[mapName]);
			}
		}

		private bool readTolerance(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadToleranceTemplate tmp = template as CadToleranceTemplate;

			switch (this._reader.Code)
			{
				case 3:
					tmp.DimensionStyleName = this._reader.ValueAsString;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[template.CadObject.SubclassMarker]);
			}
		}

		private bool readDimension(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadDimensionTemplate tmp = template as CadDimensionTemplate;

			switch (this._reader.Code)
			{
				case 2:
					tmp.BlockName = this._reader.ValueAsString;
					return true;
				case 3:
					tmp.StyleName = this._reader.ValueAsString;
					return true;
				case 50:
					var dim = new DimensionLinear();
					tmp.SetDimensionObject(dim);
					dim.Rotation = CSMath.MathHelper.DegToRad(this._reader.ValueAsDouble);
					map.SubClasses.Add(DxfSubclassMarker.LinearDimension, DxfClassMap.Create<DimensionLinear>());
					return true;
				case 70:
					//Flags do not have set
					tmp.SetDimensionFlags((DimensionType)this._reader.ValueAsShort);
					return true;
				//Measurement - read only
				case 42:
					return true;
				//Undocumented codes
				case 73:
				case 74:
				case 75:
				case 90:
				case 361:
					return true;
				case 100:
					switch (this._reader.ValueAsString)
					{
						case DxfSubclassMarker.Dimension:
							return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Dimension]);
						case DxfSubclassMarker.AlignedDimension:
							tmp.SetDimensionObject(new DimensionAligned());
							map.SubClasses.Add(this._reader.ValueAsString, DxfClassMap.Create<DimensionAligned>());
							return true;
						case DxfSubclassMarker.DiametricDimension:
							tmp.SetDimensionObject(new DimensionDiameter());
							map.SubClasses.Add(this._reader.ValueAsString, DxfClassMap.Create<DimensionDiameter>());
							return true;
						case DxfSubclassMarker.Angular2LineDimension:
							tmp.SetDimensionObject(new DimensionAngular2Line());
							map.SubClasses.Add(this._reader.ValueAsString, DxfClassMap.Create<DimensionAngular2Line>());
							return true;
						case DxfSubclassMarker.Angular3PointDimension:
							tmp.SetDimensionObject(new DimensionAngular3Pt());
							map.SubClasses.Add(this._reader.ValueAsString, DxfClassMap.Create<DimensionAngular3Pt>());
							return true;
						case DxfSubclassMarker.RadialDimension:
							tmp.SetDimensionObject(new DimensionRadius());
							map.SubClasses.Add(this._reader.ValueAsString, DxfClassMap.Create<DimensionRadius>());
							return true;
						case DxfSubclassMarker.OrdinateDimension:
							tmp.SetDimensionObject(new DimensionOrdinate());
							map.SubClasses.Add(this._reader.ValueAsString, DxfClassMap.Create<DimensionOrdinate>());
							return true;
						case DxfSubclassMarker.LinearDimension:
							return true;
						default:
							return false;
					}
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		protected bool readHatch(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadHatchTemplate tmp = template as CadHatchTemplate;
			Hatch hatch = tmp.CadObject;

			bool isFirstSeed = true;
			XY seedPoint = new XY();

			switch (this._reader.Code)
			{
				case 2:
					tmp.HatchPatternName = this._reader.ValueAsString;
					return true;
				case 10:
					seedPoint.X = this._reader.ValueAsDouble;
					return true;
				case 20:
					if (!isFirstSeed)
					{
						seedPoint.Y = this._reader.ValueAsDouble;
						hatch.SeedPoints.Add(seedPoint);
					}
					return true;
				case 30:
					hatch.Elevation = this._reader.ValueAsDouble;
					isFirstSeed = false;
					return true;
				//TODO: Check hatch undocumented codes
				case 43:
				case 44:
				case 45:
				case 46:
				case 49:
				case 53:
				case 79:
				case 90:
					return true;
				//Information about the hatch pattern
				case 75:
					return true;
				//Number of pattern definition lines
				case 78:
					return true;
				//Number of boundary paths (loops)
				case 91:
					this.readLoops(tmp, this._reader.ValueAsInt);
					return true;
				//Number of seed points
				case 98:
					return true;
				case 450:
					hatch.GradientColor.Enabled = this._reader.ValueAsBool;
					return true;
				case 451:
					hatch.GradientColor.Reserved = this._reader.ValueAsInt;
					return true;
				case 452:
					hatch.GradientColor.IsSingleColorGradient = this._reader.ValueAsBool;
					return true;
				case 453:
					//Number of colors
					return true;
				case 460:
					hatch.GradientColor.Angle = this._reader.ValueAsDouble;
					return true;
				case 461:
					hatch.GradientColor.Shift = this._reader.ValueAsDouble;
					return true;
				case 462:
					hatch.GradientColor.ColorTint = this._reader.ValueAsDouble;
					return true;
				case 463:
					GradientColor gradient = new GradientColor();
					gradient.Value = this._reader.ValueAsDouble;
					hatch.GradientColor.Colors.Add(gradient);
					return true;
				case 63:
					GradientColor colorByIndex = hatch.GradientColor.Colors.LastOrDefault();
					if (colorByIndex != null)
					{
						colorByIndex.Color = new Color((short)this._reader.ValueAsUShort);
					}
					return true;
				case 421:
					GradientColor colorByRgb = hatch.GradientColor.Colors.LastOrDefault();
					if (colorByRgb != null)
					{
						//TODO: Hatch assign color by true color
						//TODO: Is always duplicated by 63, is it needed??
						//colorByRgb.Color = new Color(this._reader.LastValueAsShort);
					}
					return true;
				case 470:
					hatch.GradientColor.Name = this._reader.ValueAsString;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[template.CadObject.SubclassMarker]);
			}
		}

		private bool readInsert(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadInsertTemplate tmp = template as CadInsertTemplate;

			switch (this._reader.Code)
			{
				case 2:
					tmp.BlockName = this._reader.ValueAsString;
					return true;
				case 66:
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private CadEntityTemplate readPolyline()
		{
			CadPolyLineTemplate template = null;

			if (this._builder.Version == ACadVersion.Unknown)
			{
				var polyline = new Polyline2D();
				template = new CadPolyLineTemplate(polyline);
				this.readEntityCodes<Polyline2D>(template, this.readPolyline);

				while (this._reader.Code == 0 && this._reader.ValueAsString == DxfFileToken.EntityVertex)
				{
					Vertex2D v = new Vertex2D();
					CadVertexTemplate vertexTemplate = new CadVertexTemplate(v);
					this.readEntityCodes<Vertex2D>(vertexTemplate, this.readVertex);

					if (vertexTemplate.Vertex.Handle == 0)
					{
						template.PolyLine.Vertices.Add(vertexTemplate.Vertex);
					}
					else
					{
						template.VertexHandles.Add(vertexTemplate.Vertex.Handle);
						this._builder.AddTemplate(vertexTemplate);
					}
				}

				while (this._reader.Code == 0 && this._reader.ValueAsString == DxfFileToken.EndSequence)
				{
					var seqend = new Seqend();
					var seqendTemplate = new CadEntityTemplate<Seqend>(seqend);
					this.readEntityCodes<Seqend>(seqendTemplate, this.readEntitySubclassMap);

					this._builder.AddTemplate(seqendTemplate);

					template.SeqendHandle = seqend.Handle;
				}
			}
			else
			{
				template = new CadPolyLineTemplate();
				this.readEntityCodes<Entity>(template, this.readPolyline);
			}

			if (template.CadObject is CadPolyLineTemplate.PolyLinePlaceholder)
			{
				this._builder.Notify($"[{DxfFileToken.EntityPolyline}] Subclass not found, entity discarded", NotificationType.Warning);
				return null;
			}

			return template;
		}

		private bool readPolyline(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadPolyLineTemplate tmp = template as CadPolyLineTemplate;

			switch (this._reader.Code)
			{
				//DXF: always 0
				//APP: a “dummy” point; the X and Y values are always 0, and the Z value is the polyline's elevation (in OCS when 2D, WCS when 3D)
				case 10:
				case 20:
				//Obsolete; formerly an “entities follow flag” (optional; ignore if present)
				case 66:
				//Polygon mesh M vertex count (optional; default = 0)
				case 71:
				//Polygon mesh N vertex count(optional; default = 0)
				case 72:
				//Smooth surface M density(optional; default = 0)
				case 73:
				//Smooth surface N density (optional; default = 0)
				case 74:
					return true;
				case 100:
					switch (this._reader.ValueAsString)
					{
						case DxfSubclassMarker.Polyline:
							tmp.SetPolyLineObject(new Polyline2D());
							map.SubClasses.Add(DxfSubclassMarker.Polyline, DxfClassMap.Create<Polyline2D>());
							return true;
						case DxfSubclassMarker.Polyline3d:
							tmp.SetPolyLineObject(new Polyline3D());
							map.SubClasses.Add(DxfSubclassMarker.Polyline3d, DxfClassMap.Create<Polyline3D>());
							return true;
						case DxfSubclassMarker.PolyfaceMesh:
							tmp.SetPolyLineObject(new PolyfaceMesh());
							map.SubClasses.Add(DxfSubclassMarker.PolyfaceMesh, DxfClassMap.Create<PolyfaceMesh>());
							return true;
						default:
							return false;
					}
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readLeader(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadLeaderTemplate tmp = template as CadLeaderTemplate;

			switch (this._reader.Code)
			{
				case 3:
					tmp.DIMSTYLEName = this._reader.ValueAsString;
					return true;
				case 10:
					tmp.CadObject.Vertices.Add(new XYZ(this._reader.ValueAsDouble, 0, 0));
					return true;
				case 20:
					XYZ y = tmp.CadObject.Vertices[tmp.CadObject.Vertices.Count - 1];
					y.Y = this._reader.ValueAsDouble;
					tmp.CadObject.Vertices[tmp.CadObject.Vertices.Count - 1] = y;
					return true;
				case 30:
					XYZ z = tmp.CadObject.Vertices[tmp.CadObject.Vertices.Count - 1];
					z.Z = this._reader.ValueAsDouble;
					tmp.CadObject.Vertices[tmp.CadObject.Vertices.Count - 1] = z;
					return true;
				case 340:
					tmp.AnnotationHandle = this._reader.ValueAsHandle;
					return true;
				//Vertices count
				case 76:
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readLwPolyline(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadEntityTemplate<LwPolyline> tmp = template as CadEntityTemplate<LwPolyline>;

			LwPolyline.Vertex last = tmp.CadObject.Vertices.LastOrDefault();

			switch (this._reader.Code)
			{
				case 10:
					tmp.CadObject.Vertices.Add(new LwPolyline.Vertex(new XY(this._reader.ValueAsDouble, 0)));
					return true;
				case 20:
					if (last is not null)
					{
						last.Location = new XY(last.Location.X, this._reader.ValueAsDouble);
					}
					return true;
				case 40:
					if (last is not null)
					{
						last.StartWidth = this._reader.ValueAsDouble;
					}
					return true;
				case 41:
					if (last is not null)
					{
						last.EndWidth = this._reader.ValueAsDouble;
					}
					return true;
				case 42:
					if (last is not null)
					{
						last.Bulge = this._reader.ValueAsDouble;
					}
					return true;
				case 50:
					if (last is not null)
					{
						last.CurveTangent = this._reader.ValueAsDouble;
					}
					return true;
				//Obsolete; formerly an “entities follow flag” (optional; ignore if present)
				case 66:
				//Vertex count
				case 90:
					return true;
				case 91:
					if (last is not null)
					{
						last.Id = this._reader.ValueAsInt;
					}
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readMesh(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadMeshTemplate tmp = template as CadMeshTemplate;

			switch (this._reader.Code)
			{
				case 100:
					if (this._reader.ValueAsString.Equals(DxfSubclassMarker.Mesh, StringComparison.OrdinalIgnoreCase))
					{
						tmp.SubclassMarker = true;
					}
					return true;
				//Count of sub-entity which property has been overridden
				case 90:
					//TODO: process further entities
					return true;
				case 92:
					if (!tmp.SubclassMarker)
					{
						return false;
					}

					int nvertices = this._reader.ValueAsInt;
					for (int i = 0; i < nvertices; i++)
					{
						this._reader.ReadNext();
						double x = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						double y = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						double z = this._reader.ValueAsDouble;
						tmp.CadObject.Vertices.Add(new XYZ(x, y, z));
					}
					return true;
				case 93:
					int size = this._reader.ValueAsInt;
					this._reader.ReadNext();

					int indexes = 0;
					for (int i = 0; i < size; i += indexes + 1)
					{
						indexes = this._reader.ValueAsInt;
						this._reader.ReadNext();

						int[] face = new int[indexes];
						for (int j = 0; j < indexes; j++)
						{
							face[j] = this._reader.ValueAsInt;

							if ((i + j + 2) < size)
							{
								this._reader.ReadNext();
							}
						}

						tmp.CadObject.Faces.Add(face);
					}

					Debug.Assert(this._reader.Code == 90);

					return true;
				case 94:
					int numEdges = this._reader.ValueAsInt;
					this._reader.ReadNext();
					for (int i = 0; i < numEdges; i++)
					{
						Mesh.Edge edge = new Mesh.Edge();

						edge.Start = this._reader.ValueAsInt;
						this._reader.ReadNext();
						edge.End = this._reader.ValueAsInt;

						if (i < numEdges - 1)
						{
							this._reader.ReadNext();
						}

						tmp.CadObject.Edges.Add(edge);
					}

					Debug.Assert(this._reader.Code == 90);

					return true;
				case 95:
					this._reader.ReadNext();
					for (int i = 0; i < tmp.CadObject.Edges.Count; i++)
					{
						Mesh.Edge edge = tmp.CadObject.Edges[i];
						edge.Crease = this._reader.ValueAsDouble;

						tmp.CadObject.Edges[i] = edge;

						if (i < tmp.CadObject.Edges.Count - 1)
						{
							this._reader.ReadNext();
						}
					}

					Debug.Assert(this._reader.Code == 140);

					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readMLine(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadMLineTemplate tmp = template as CadMLineTemplate;

			switch (this._reader.Code)
			{
				// String of up to 32 characters. The name of the style used for this mline. An entry for this style must exist in the MLINESTYLE dictionary.
				// Do not modify this field without also updating the associated entry in the MLINESTYLE dictionary
				case 2:
					tmp.MLineStyleName = this._reader.ValueAsString;
					return true;
				case 72:
					tmp.NVertex = this._reader.ValueAsInt;
					return true;
				case 73:
					tmp.NElements = this._reader.ValueAsInt;
					return true;
				case 340:
					tmp.MLineStyleHandle = this._reader.ValueAsHandle;
					return true;
				default:
					if (!tmp.TryReadVertex(this._reader.Code, this._reader.Value))
					{
						return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
					}
					return true;
			}
		}

		private bool readShape(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadShapeTemplate tmp = template as CadShapeTemplate;

			switch (this._reader.Code)
			{
				case 2:
					tmp.ShapeFileName = this._reader.ValueAsString;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readSpline(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadSplineTemplate tmp = template as CadSplineTemplate;

			XYZ controlPoint;

			switch (this._reader.Code)
			{
				case 10:
					controlPoint = new CSMath.XYZ(this._reader.ValueAsDouble, 0, 0);
					tmp.CadObject.ControlPoints.Add(controlPoint);
					return true;
				case 20:
					controlPoint = tmp.CadObject.ControlPoints.LastOrDefault();
					controlPoint.Y = this._reader.ValueAsDouble;
					tmp.CadObject.ControlPoints[tmp.CadObject.ControlPoints.Count - 1] = controlPoint;
					return true;
				case 30:
					controlPoint = tmp.CadObject.ControlPoints.LastOrDefault();
					controlPoint.Z = this._reader.ValueAsDouble;
					tmp.CadObject.ControlPoints[tmp.CadObject.ControlPoints.Count - 1] = controlPoint;
					return true;
				case 40:
					tmp.CadObject.Knots.Add(this._reader.ValueAsDouble);
					return true;
				case 41:
					tmp.CadObject.Weights.Add(this._reader.ValueAsDouble);
					return true;
				case 72:
				case 73:
				case 74:
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readUnderlayEntity(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadPdfUnderlayTemplate tmp = template as CadPdfUnderlayTemplate;

			switch (this._reader.Code)
			{
				case 340:
					tmp.DefinitionHandle = this._reader.ValueAsHandle;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readVertex(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadVertexTemplate tmp = template as CadVertexTemplate;

			switch (this._reader.Code)
			{
				//Polyface mesh vertex index
				case 71:
				case 72:
				case 73:
				case 74:
					return true;
				case 100:
					switch (this._reader.ValueAsString)
					{
						case DxfSubclassMarker.Vertex:
							return true;
						case DxfSubclassMarker.PolylineVertex:
							tmp.SetVertexObject(new Vertex2D());
							map.SubClasses.Add(DxfSubclassMarker.PolylineVertex, DxfClassMap.Create<Vertex2D>());
							return true;
						case DxfSubclassMarker.Polyline3dVertex:
							tmp.SetVertexObject(new Vertex3D());
							map.SubClasses.Add(DxfSubclassMarker.Polyline3dVertex, DxfClassMap.Create<Vertex3D>());
							return true;
						case DxfSubclassMarker.PolyfaceMeshVertex:
							tmp.SetVertexObject(new VertexFaceMesh());
							map.SubClasses.Add(DxfSubclassMarker.PolyfaceMeshVertex, DxfClassMap.Create<VertexFaceMesh>());
							return true;
						case DxfSubclassMarker.PolyfaceMeshFace:
							tmp.SetVertexObject(new VertexFaceRecord());
							map.SubClasses.Add(DxfSubclassMarker.PolyfaceMeshFace, DxfClassMap.Create<VertexFaceRecord>());
							return true;
						default:
							return false;
					}
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[tmp.CadObject.SubclassMarker]);
			}
		}

		private bool readViewport(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			CadViewportTemplate tmp = template as CadViewportTemplate;

			switch (this._reader.Code)
			{
				//Undocumented
				case 67:
				case 68:
					return true;
				case 69:
					tmp.ViewportId = this._reader.ValueAsShort;
					return true;
				case 331:
					tmp.FrozenLayerHandles.Add(this._reader.ValueAsHandle);
					return true;
				case 348:
					tmp.VisualStyleHandle = this._reader.ValueAsHandle;
					return true;
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[DxfSubclassMarker.Viewport]);
			}
		}

		private bool readEntitySubclassMap(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			string mapName = string.IsNullOrEmpty(subclass) ? template.CadObject.SubclassMarker : subclass;

			switch (this._reader.Code)
			{
				default:
					return this.tryAssignCurrentValue(template.CadObject, map.SubClasses[mapName]);
			}
		}

		protected void readExtendedData(Dictionary<string, List<ExtendedDataRecord>> edata)
		{
			List<ExtendedDataRecord> records = new();
			edata.Add(this._reader.ValueAsString, records);

			this._reader.ReadNext();

			while (this._reader.DxfCode >= DxfCode.ExtendedDataAsciiString)
			{
				if (this._reader.DxfCode == DxfCode.ExtendedDataRegAppName)
				{
					this.readExtendedData(edata);
					break;
				}

				ExtendedDataRecord record = null;
				double x = 0;
				double y = 0;
				double z = 0;

				switch (this._reader.DxfCode)
				{
					case DxfCode.ExtendedDataAsciiString:
					case DxfCode.ExtendedDataRegAppName:
						record = new ExtendedDataString(this._reader.ValueAsString);
						break;
					case DxfCode.ExtendedDataControlString:
						record = new ExtendedDataControlString(this._reader.ValueAsString == "}");
						break;
					case DxfCode.ExtendedDataLayerName:
						record = new ExtendedDataLayer(this._reader.ValueAsHandle);
						break;
					case DxfCode.ExtendedDataBinaryChunk:
						record = new ExtendedDataBinaryChunk(this._reader.ValueAsBinaryChunk);
						break;
					case DxfCode.ExtendedDataHandle:
						record = new ExtendedDataHandle(this._reader.ValueAsHandle);
						break;
					case DxfCode.ExtendedDataXCoordinate:
						x = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						y = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						z = this._reader.ValueAsDouble;

						record = new ExtendedDataCoordinate(
							new XYZ(
								x,
								y,
								z)
							);
						break;
					case DxfCode.ExtendedDataWorldXCoordinate:
						x = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						y = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						z = this._reader.ValueAsDouble;

						record = new ExtendedDataCoordinate(
							new XYZ(
								x,
								y,
								z)
							);
						break;
					case DxfCode.ExtendedDataWorldXDisp:
						x = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						y = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						z = this._reader.ValueAsDouble;

						record = new ExtendedDataCoordinate(
							new XYZ(
								x,
								y,
								z)
							);
						break;
					case DxfCode.ExtendedDataWorldXDir:
						x = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						y = this._reader.ValueAsDouble;
						this._reader.ReadNext();
						z = this._reader.ValueAsDouble;

						record = new ExtendedDataCoordinate(
							new XYZ(
								x,
								y,
								z)
							);
						break;
					case DxfCode.ExtendedDataReal:
						record = new ExtendedDataReal(this._reader.ValueAsDouble);
						break;
					case DxfCode.ExtendedDataDist:
						record = new ExtendedDataDistance(this._reader.ValueAsDouble);
						break;
					case DxfCode.ExtendedDataScale:
						record = new ExtendedDataScale(this._reader.ValueAsDouble);
						break;
					case DxfCode.ExtendedDataInteger16:
						record = new ExtendedDataInteger16(this._reader.ValueAsShort);
						break;
					case DxfCode.ExtendedDataInteger32:
						record = new ExtendedDataInteger32((int)this._reader.ValueAsInt);
						break;
					default:
						this._builder.Notify($"Unknown code for extended data: {this._reader.DxfCode}", NotificationType.Warning);
						break;
				}

				if (record != null)
				{
					records.Add(record);
				}

				this._reader.ReadNext();
			}
		}

		private void readLoops(CadHatchTemplate template, int count)
		{
			if (this._reader.Code == 91)
				this._reader.ReadNext();

			for (int i = 0; i < count; i++)
			{
				if (this._reader.Code != 92)
				{
					this._builder.Notify($"Boundary path should start with code 92 but was {this._reader.Code}");
					break;
				}

				CadHatchTemplate.CadBoundaryPathTemplate path = this.readLoop();
				if (path != null)
					template.PathTempaltes.Add(path);
			}
		}

		private CadHatchTemplate.CadBoundaryPathTemplate readLoop()
		{
			CadHatchTemplate.CadBoundaryPathTemplate template = new CadHatchTemplate.CadBoundaryPathTemplate();
			template.Path.Flags = (BoundaryPathFlags)this._reader.ValueAsInt;

			if (template.Path.Flags.HasFlag(BoundaryPathFlags.Polyline))
			{
				Hatch.BoundaryPath.Polyline pl = this.readPolylineBoundary();
				template.Path.Edges.Add(pl);
			}
			else
			{
				this._reader.ReadNext();

				if (this._reader.Code != 93)
				{
					this._builder.Notify($"Edge Boundary path should start with code 93 but was {this._reader.Code}");
					return null;
				}

				int edges = this._reader.ValueAsInt;
				this._reader.ReadNext();

				for (int i = 0; i < edges; i++)
				{
					var edge = this.readEdge();
					if (edge != null)
						template.Path.Edges.Add(edge);
				}
			}

			bool end = false;
			while (!end)
			{
				switch (this._reader.Code)
				{
					//Number of source boundary objects
					case 97:
						break;
					case 330:
						template.Handles.Add(this._reader.ValueAsHandle);
						break;
					default:
						end = true;
						continue;
				}

				this._reader.ReadNext();
			}

			return template;
		}

		private Hatch.BoundaryPath.Polyline readPolylineBoundary()
		{
			Hatch.BoundaryPath.Polyline boundary = new Hatch.BoundaryPath.Polyline();

			this._reader.ReadNext();

			if (this._reader.Code != 72)
			{
				this._builder.Notify($"Polyline Boundary path should start with code 72 but was {this._reader.Code}");
				return null;
			}

			//72
			bool hasBulge = this._reader.ValueAsBool;
			this._reader.ReadNext();

			//73
			bool isClosed = this._reader.ValueAsBool;
			this._reader.ReadNext();

			//93
			int nvertices = this._reader.ValueAsInt;
			this._reader.ReadNext();

			for (int i = 0; i < nvertices; i++)
			{
				double bulge = 0.0;

				//10
				double x = this._reader.ValueAsDouble;
				this._reader.ReadNext();
				//20
				double y = this._reader.ValueAsDouble;
				this._reader.ReadNext();

				if (hasBulge)
				{
					//42
					bulge = this._reader.ValueAsDouble;
					this._reader.ReadNext();
				}

				boundary.Vertices.Add(new XYZ(x, y, bulge));
			}

			return boundary;
		}

		private Hatch.BoundaryPath.Edge readEdge()
		{
			if (this._reader.Code != 72)
			{
				this._builder.Notify($"Edge Boundary path should define the type with code 72 but was {this._reader.Code}");
				return null;
			}

			Hatch.BoundaryPath.EdgeType type = (Hatch.BoundaryPath.EdgeType)this._reader.ValueAsInt;
			this._reader.ReadNext();

			switch (type)
			{
				case Hatch.BoundaryPath.EdgeType.Line:
					Hatch.BoundaryPath.Line line = new Hatch.BoundaryPath.Line();
					while (true)
					{
						switch (this._reader.Code)
						{
							case 10:
								line.Start = new XY(this._reader.ValueAsDouble, line.Start.Y);
								break;
							case 20:
								line.Start = new XY(line.Start.X, this._reader.ValueAsDouble);
								break;
							case 11:
								line.End = new XY(this._reader.ValueAsDouble, line.End.Y);
								break;
							case 21:
								line.End = new XY(line.End.X, this._reader.ValueAsDouble);
								break;
							default:
								return line;
						}

						this._reader.ReadNext();
					}
				case Hatch.BoundaryPath.EdgeType.CircularArc:
					Hatch.BoundaryPath.Arc arc = new Hatch.BoundaryPath.Arc();
					while (true)
					{
						switch (this._reader.Code)
						{
							case 10:
								arc.Center = new XY(this._reader.ValueAsDouble, arc.Center.Y);
								break;
							case 20:
								arc.Center = new XY(arc.Center.X, this._reader.ValueAsDouble);
								break;
							case 40:
								arc.Radius = this._reader.ValueAsDouble;
								break;
							case 50:
								arc.StartAngle = this._reader.ValueAsDouble;
								break;
							case 51:
								arc.EndAngle = this._reader.ValueAsDouble;
								break;
							case 73:
								arc.CounterClockWise = this._reader.ValueAsBool;
								break;
							default:
								return arc;
						}

						this._reader.ReadNext();
					}
				case Hatch.BoundaryPath.EdgeType.EllipticArc:
					Hatch.BoundaryPath.Ellipse ellipse = new Hatch.BoundaryPath.Ellipse();
					while (true)
					{
						switch (this._reader.Code)
						{
							case 10:
								ellipse.Center = new XY(this._reader.ValueAsDouble, ellipse.Center.Y);
								break;
							case 20:
								ellipse.Center = new XY(ellipse.Center.X, this._reader.ValueAsDouble);
								break;
							case 11:
								ellipse.MajorAxisEndPoint = new XY(this._reader.ValueAsDouble, ellipse.Center.Y);
								break;
							case 21:
								ellipse.MajorAxisEndPoint = new XY(ellipse.Center.X, this._reader.ValueAsDouble);
								break;
							case 40:
								ellipse.MinorToMajorRatio = this._reader.ValueAsDouble;
								break;
							case 50:
								ellipse.StartAngle = this._reader.ValueAsDouble;
								break;
							case 51:
								ellipse.EndAngle = this._reader.ValueAsDouble;
								break;
							case 73:
								ellipse.CounterClockWise = this._reader.ValueAsBool;
								break;
							default:
								return ellipse;
						}

						this._reader.ReadNext();
					}
				case Hatch.BoundaryPath.EdgeType.Spline:
					Hatch.BoundaryPath.Spline spline = new Hatch.BoundaryPath.Spline();
					int nKnots = 0;
					int nCtrlPoints = 0;
					int nFitPoints = 0;

					XYZ controlPoint = new XYZ();
					XY fitPoint = new XY();

					while (true)
					{
						switch (this._reader.Code)
						{
							case 10:
								controlPoint = new XYZ(this._reader.ValueAsDouble, 0, 1);
								break;
							case 20:
								controlPoint = new XYZ(controlPoint.X, this._reader.ValueAsDouble, controlPoint.Z);
								spline.ControlPoints.Add(controlPoint);
								break;
							case 11:
								fitPoint = new XY(this._reader.ValueAsDouble, 0);
								break;
							case 21:
								fitPoint = new XY(fitPoint.X, this._reader.ValueAsDouble);
								spline.FitPoints.Add(fitPoint);
								break;
							case 42:
								var last = spline.ControlPoints[spline.ControlPoints.Count - 1];
								spline.ControlPoints[spline.ControlPoints.Count - 1] = new XYZ(last.X, last.Y, this._reader.ValueAsDouble);
								break;
							case 12:
								spline.StartTangent = new XY(this._reader.ValueAsDouble, spline.StartTangent.Y);
								break;
							case 22:
								spline.StartTangent = new XY(spline.StartTangent.X, this._reader.ValueAsDouble);
								break;
							case 13:
								spline.EndTangent = new XY(this._reader.ValueAsDouble, spline.EndTangent.Y);
								break;
							case 23:
								spline.EndTangent = new XY(spline.EndTangent.X, this._reader.ValueAsDouble);
								break;
							case 94:
								spline.Degree = this._reader.ValueAsInt;
								break;
							case 73:
								spline.Rational = this._reader.ValueAsBool;
								break;
							case 74:
								spline.Periodic = this._reader.ValueAsBool;
								break;
							case 95:
								nKnots = this._reader.ValueAsInt;
								break;
							case 96:
								nCtrlPoints = this._reader.ValueAsInt;
								break;
							case 97:
								nFitPoints = this._reader.ValueAsInt;
								break;
							case 40:
								spline.Knots.Add(this._reader.ValueAsDouble);
								break;
							default:
								return spline;
						}

						this._reader.ReadNext();
					}
			}

			return null;
		}

		private void readDefinedGroups(CadTemplate template)
		{
			this.readDefinedGroups(out ulong? xdict, out List<ulong> reactorsHandles);

			template.XDictHandle = xdict;
			template.ReactorsHandles = reactorsHandles;
		}

		private void readDefinedGroups(out ulong? xdictHandle, out List<ulong> reactors)
		{
			xdictHandle = null;
			reactors = new List<ulong>();

			switch (this._reader.ValueAsString)
			{
				case DxfFileToken.DictionaryToken:
					this._reader.ReadNext();
					xdictHandle = this._reader.ValueAsHandle;
					this._reader.ReadNext();
					Debug.Assert(this._reader.DxfCode == DxfCode.ControlString);
					return;
				case DxfFileToken.ReactorsToken:
					reactors = this.readReactors();
					break;
				case DxfFileToken.BlkRefToken:
				default:
					do
					{
						this._reader.ReadNext();
					}
					while (this._reader.DxfCode != DxfCode.ControlString);
					return;
			}
		}

		private List<ulong> readReactors()
		{
			List<ulong> reactors = new List<ulong>();

			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.ControlString)
			{
				this._reader.ReadNext();
			}

			return reactors;
		}

		protected bool tryAssignCurrentValue(CadObject cadObject, DxfClassMap map)
		{
			try
			{
				//Use this method only if the value is not a link between objects
				if (map.DxfProperties.TryGetValue(this._reader.Code, out DxfProperty dxfProperty))
				{
					if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Count))
					{
						return true;
					}

					if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Handle)
						|| dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Name))
					{
						return false;
					}

					object value = this._reader.Value;

					if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.IsAngle))
					{
						value = (double)value * MathUtils.DegToRadFactor;
					}

					dxfProperty.SetValue(this._reader.Code, cadObject, value);

					return true;
				}
			}
			catch (Exception ex)
			{
				if (!this._builder.Configuration.Failsafe)
				{
					throw ex;
				}
				else
				{
					this._builder.Notify("An error occurred while assigning a property using mapper", NotificationType.Error, ex);
				}
			}

			return false;
		}
	}
}
