using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using CSMath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfSectionReaderBase
	{
		public delegate bool ReadEntityDelegate<T>(CadEntityTemplate template, DxfMap map, string subclass = null) where T : Entity;

		/// <summary>
		/// Object reactors, list of handles
		/// </summary>
		public const string ReactorsToken = "{ACAD_REACTORS";

		/// <summary>
		/// Handle for the xdictionary
		/// </summary>
		public const string DictionaryToken = "{ACAD_XDICTIONARY";

		/// <summary>
		/// Block references
		/// </summary>
		public const string BlkRefToken = "{BLKREFS";

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
						this._builder.Notify($"Unhandeled dxf code {this._reader.Code} at line {this._reader.Position}.");
						break;
				}

				this._reader.ReadNext();
			}
		}

		[Obsolete]
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
						this._builder.Notify($"Unhandeled dxf code {this._reader.Code} at line {this._reader.Position}.", NotificationType.None);
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
					this._builder.Notify($"[{template.CadObject.SubclassMarker}] Unhandeled dxf code {this._reader.Code} with value {this._reader.ValueAsString}", NotificationType.None);
					break;
			}
		}

		protected CadEntityTemplate readEntity()
		{
			switch (this._reader.ValueAsString)
			{
				case DxfFileToken.EntityAttribute:
					return this.readEntityCodes<AttributeEntity>(new CadTextEntityTemplate(new AttributeEntity()), readAttributeDefinition);
				case DxfFileToken.EntityAttributeDefinition:
					return this.readEntityCodes<AttributeDefinition>(new CadTextEntityTemplate(new AttributeDefinition()), readAttributeDefinition);
				case DxfFileToken.EntityArc:
					return this.readEntityCodes<Arc>(new CadEntityTemplate<Arc>(), readArc);
				case DxfFileToken.EntityCircle:
					return this.readEntityCodes<Circle>(new CadEntityTemplate<Circle>(), readEntitySubclassMap);
				case DxfFileToken.EntityDimension:
					return this.readEntityCodes<Dimension>(new CadDimensionTemplate(), readDimension);
				case DxfFileToken.Entity3DFace:
					return this.readEntityCodes<Face3D>(new CadEntityTemplate<Face3D>(), readEntitySubclassMap);
				case DxfFileToken.EntityEllipse:
					return this.readEntityCodes<Ellipse>(new CadEntityTemplate<Ellipse>(), readEntitySubclassMap);
				case DxfFileToken.EntityLine:
					return this.readEntityCodes<Line>(new CadEntityTemplate<Line>(), readEntitySubclassMap);
				case DxfFileToken.EntityLwPolyline:
					return this.readEntityCodes<LwPolyline>(new CadEntityTemplate<LwPolyline>(), readLwPolyline);
				case DxfFileToken.EntityHatch:
					return this.readEntityCodes<Hatch>(new CadHatchTemplate(), readHatch);
				case DxfFileToken.EntityInsert:
					return this.readEntityCodes<Insert>(new CadInsertTemplate(), readInsert);
				case DxfFileToken.EntityMText:
					return this.readEntityCodes<MText>(new CadTextEntityTemplate(new MText()), readTextEntity);
				case DxfFileToken.EntityMLine:
					return this.readEntityCodes<MLine>(new CadMLineTemplate(), readMLine);
				case DxfFileToken.EntityPoint:
					return this.readEntityCodes<Point>(new CadEntityTemplate<Point>(), readEntitySubclassMap);
				case DxfFileToken.EntityPolyline:
					var template = this.readEntityCodes<Entity>(new CadPolyLineTemplate(), readPolyline);
					if (template.CadObject is CadPolyLineTemplate.PolyLinePlaceholder)
					{
						this._builder.Notify($"[{DxfFileToken.EntityPolyline}] Subclass not found, entity discarded", NotificationType.Warning);
						return null;
					}
					else
					{
						return template;
					}
				case DxfFileToken.EntityRay:
					return this.readEntityCodes<Ray>(new CadEntityTemplate<Ray>(), readEntitySubclassMap);
				case DxfFileToken.EndSequence:
					return this.readEntityCodes<Seqend>(new CadEntityTemplate<Seqend>(), readEntitySubclassMap);
				case DxfFileToken.EntitySolid:
					return this.readEntityCodes<Solid>(new CadEntityTemplate<Solid>(), readEntitySubclassMap);
				case DxfFileToken.EntityText:
					return this.readEntityCodes<TextEntity>(new CadTextEntityTemplate(new TextEntity()), readTextEntity);
				case DxfFileToken.EntityVertex:
					return this.readEntityCodes<Entity>(new CadVertexTemplate(), readVertex);
				case DxfFileToken.EntityViewport:
					return this.readEntityCodes<Viewport>(new CadViewportTemplate(), this.readViewport);
				case DxfFileToken.EntityXline:
					return this.readEntityCodes<XLine>(new CadEntityTemplate<XLine>(), readEntitySubclassMap);
				case DxfFileToken.EntitySpline:
					return this.readEntityCodes<Spline>(new CadSplineTemplate(), readSpline);
				default:
					this._builder.Notify($"Entity not implemented: {this._reader.ValueAsString}", NotificationType.NotImplemented);
					do
					{
						this._reader.ReadNext();
					}
					while (this._reader.DxfCode != DxfCode.Start);
					return null;
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
				case 347:
					template.MaterialHandle = this._reader.ValueAsHandle;
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
			CadTextEntityTemplate tmp = template as CadTextEntityTemplate;

			switch (this._reader.Code)
			{
				//TODO: Implement multiline attribute def codes
				case 44:
				case 46:
				case 101:
					return true;
				default:
					if (!this.tryAssignCurrentValue(template.CadObject, emap))
					{
						return this.readTextEntity(template, map, DxfSubclassMarker.Text);
					}
					return true;
			}
		}

		private bool readTextEntity(CadEntityTemplate template, DxfMap map, string subclass = null)
		{
			string mapName = string.IsNullOrEmpty(subclass) ? template.CadObject.SubclassMarker : subclass;
			CadTextEntityTemplate tmp = template as CadTextEntityTemplate;

			switch (this._reader.Code)
			{
				//TODO: Implement multiline text def codes
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
					dim.Rotation = this._reader.ValueAsDouble;
					map.SubClasses.Add(DxfSubclassMarker.LinearDimension, DxfClassMap.Create<DimensionLinear>());
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
					this.readLoops(hatch, tmp, this._reader.ValueAsInt);
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
				case 90:
					//Vertex count
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

		protected void readMapped<T>(CadObject cadObject, CadTemplate template)
			where T : CadObject
		{
			DxfClassMap map = DxfClassMap.Create<T>();

			Debug.Assert(map.Name == this._reader.ValueAsString);
			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start
				&& this._reader.DxfCode != DxfCode.Subclass)
			{
				//Check for an extended data code
				if (this._reader.DxfCode == DxfCode.ExtendedDataRegAppName)
				{
					this.readExtendedData(template.EDataTemplateByAppName);
					continue;
				}
				else if (this._reader.DxfCode >= DxfCode.ExtendedDataAsciiString)
				{
					this._builder.Notify($"Extended data should start witth : {DxfCode.ExtendedDataRegAppName}");
					this._reader.ReadNext();
					continue;
				}
				else if (this._reader.DxfCode == DxfCode.ControlString)
				{
					if (!template.CheckDxfCode(this._reader.Code, this._reader.Value))
					{
						this.readDefinedGroups(template);
						this._reader.ReadNext();
					}
					else
					{
						this._reader.ReadNext();
					}

					continue;
				}

				if (!map.DxfProperties.TryGetValue(this._reader.Code, out DxfProperty dxfProperty))
				{
					if (!template.CheckDxfCode(this._reader.Code, this._reader.Value))
						this._builder.Notify($"Dxf code {this._reader.Code} not found in map for {typeof(T)} | value : {this._reader.ValueAsString}");

					this._reader.ReadNext();
					continue;
				}

				if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Handle))
				{
					if (!template.AddHandle(this._reader.Code, this._reader.ValueAsHandle))
						this._builder.Notify($"Dxf referenced code {this._reader.Code} not implemented in the {template.GetType().Name} for {typeof(T)} | value : {this._reader.ValueAsHandle}");
				}
				else if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Name))
				{
					if (!template.AddName(this._reader.Code, this._reader.ValueAsString))
						this._builder.Notify($"Dxf named referenced code {this._reader.Code} not implemented in the {template.GetType().Name} for {typeof(T)} | value : {this._reader.ValueAsString}");
				}
				else if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Count))
				{
					//Do nothing just marks the amount
				}
				else if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Unprocess) || dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Ignored))
				{
					this._reader.ReadNext();
					continue;
				}
				else
				{
					object value = this._reader.Value;

					if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.IsAngle))
					{
						value = (double)value * MathUtils.DegToRad;
					}

					switch (this._reader.GroupCodeValue)
					{
						case GroupCodeValueType.String:
						case GroupCodeValueType.Point3D:
						case GroupCodeValueType.Double:
						case GroupCodeValueType.Int16:
						case GroupCodeValueType.Int32:
						case GroupCodeValueType.Int64:
						case GroupCodeValueType.Chunk:
						case GroupCodeValueType.Bool:
							dxfProperty.SetValue(this._reader.Code, cadObject, value);
							break;
						case GroupCodeValueType.Comment:
							this._builder.Notify($"Comment in the file:  {this._reader.ValueAsString}");
							break;
						case GroupCodeValueType.Handle:
						case GroupCodeValueType.ObjectId:
						case GroupCodeValueType.None:
						default:
							this._builder.Notify($"Group Code not handled {this._reader.GroupCodeValue} for {typeof(T)}, code : {this._reader.Code} | value : {this._reader.ValueAsString}");
							break;
					}
				}

				this._reader.ReadNext();
			}
		}

		protected void readExtendedData(Dictionary<string, ExtendedData> edata)
		{
			ExtendedData extendedData = new ExtendedData();
			edata.Add(this._reader.ValueAsString, extendedData);

			this._reader.ReadNext();

			while (this._reader.DxfCode >= DxfCode.ExtendedDataAsciiString)
			{
				if (this._reader.DxfCode == DxfCode.ExtendedDataRegAppName)
				{
					this.readExtendedData(edata);
					break;
				}

				extendedData.Data.Add(new ExtendedDataRecord(this._reader.DxfCode, this._reader.Value));

				this._reader.ReadNext();
			}
		}

		protected void readHatch(Hatch hatch, CadHatchTemplate template)
		{
			bool isFirstSeed = true;
			XY seedPoint = new XY();
			DxfClassMap map = DxfClassMap.Create<Hatch>();

			//Jump sublcass
			this._reader.ReadNext();

			while (this._reader.DxfCode != DxfCode.Start)
			{
				map.DxfProperties.TryGetValue(this._reader.Code, out DxfProperty dxfProperty);

				switch (this._reader.Code)
				{
					//TODO: Check hatch undocumented codes
					case 43:
					case 44:
					case 45:
					case 46:
					case 49:
					case 53:
					case 79:
					case 90:
						break;
					case 2:
						template.HatchPatternName = this._reader.ValueAsString;
						break;
					case 10:
						seedPoint = new XY(this._reader.ValueAsDouble, seedPoint.Y);
						break;
					case 20:
						if (!isFirstSeed)
						{
							seedPoint = new XY(seedPoint.X, this._reader.ValueAsDouble);
							hatch.SeedPoints.Add(seedPoint);
						}
						break;
					case 30:
						hatch.Elevation = this._reader.ValueAsDouble;
						isFirstSeed = false;
						break;
					case 78:    //Number of pattern definition lines
						break;
					case 91:    //Number of boundary paths (loops)
						this.readLoops(hatch, template, this._reader.ValueAsInt);
						continue;
					case 98:    //Number of seed points
						break;
					case 450:
						hatch.GradientColor.Enabled = this._reader.ValueAsBool;
						break;
					case 451:
						hatch.GradientColor.Reserved = this._reader.ValueAsInt;
						break;
					case 452:
						hatch.GradientColor.IsSingleColorGradient = this._reader.ValueAsBool;
						break;
					case 453:
						//Number of colors
						break;
					case 460:
						hatch.GradientColor.Angle = this._reader.ValueAsDouble;
						break;
					case 461:
						hatch.GradientColor.Shift = this._reader.ValueAsDouble;
						break;
					case 462:
						hatch.GradientColor.ColorTint = this._reader.ValueAsDouble;
						break;
					case 463:
						GradientColor gradient = new GradientColor();
						gradient.Value = this._reader.ValueAsDouble;
						hatch.GradientColor.Colors.Add(gradient);
						break;
					case 63:
						GradientColor colorByIndex = hatch.GradientColor.Colors.LastOrDefault();
						if (colorByIndex != null)
						{
							colorByIndex.Color = new Color((short)this._reader.ValueAsUShort);
						}
						break;
					case 421:
						GradientColor colorByRgb = hatch.GradientColor.Colors.LastOrDefault();
						if (colorByRgb != null)
						{
							//TODO: Hatch assign color by true color
							//TODO: Is always duplicated by 63, is it needed??
							//colorByRgb.Color = new Color(this._reader.LastValueAsShort);
						}
						break;
					case 470:
						hatch.GradientColor.Name = this._reader.ValueAsString;
						break;
					default:
						if (dxfProperty != null)
						{
							dxfProperty.SetValue(hatch, this._reader.Value);
							break;
						}
						else if (this._reader.DxfCode >= DxfCode.ExtendedDataAsciiString)
						{
							this.readExtendedData(template.EDataTemplateByAppName);
							continue;
						}
						this._builder.Notify($"Unhandeled dxf code : {this._reader.Code} with value : {this._reader.Value} for subclass {DxfSubclassMarker.Hatch}");
						break;
				}

				this._reader.ReadNext();
			}
		}

		private void readLoops(Hatch hatch, CadHatchTemplate template, int count)
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
				Hatch.BoundaryPath.Edge pl = new Hatch.BoundaryPath.Polyline();
				this._builder.Notify($"Hatch.BoundaryPath.Polyline not implemented", NotificationType.Error);

				return null;
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

		private void readPolylineBoundary()
		{

		}

		private Hatch.BoundaryPath.Edge readEdge()
		{
			if (this._reader.Code != 72)
			{
				this._builder.Notify($"Edge Boundary path should should define the type with code 72 but was {this._reader.Code}");
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
				case DxfSectionReaderBase.DictionaryToken:
					this._reader.ReadNext();
					xdictHandle = this._reader.ValueAsHandle;
					this._reader.ReadNext();
					Debug.Assert(this._reader.DxfCode == DxfCode.ControlString);
					return;
				case DxfSectionReaderBase.ReactorsToken:
					reactors = readReactors();
					break;
				case DxfSectionReaderBase.BlkRefToken:
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
					if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Handle)
						|| dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Name)
						|| dxfProperty.ReferenceType.HasFlag(DxfReferenceType.Count))
					{
						return false;
					}

					object value = this._reader.Value;

					if (dxfProperty.ReferenceType.HasFlag(DxfReferenceType.IsAngle))
					{
						value = (double)value * MathUtils.DegToRad;
					}

					dxfProperty.SetValue(this._reader.Code, cadObject, value);

					return true;
				}
			}
			catch (Exception ex)
			{
				if (!_builder.Configuration.Failsafe)
				{
					throw ex;
				}
				else
				{
					this._builder.Notify("An error occurred while assiging a property using mapper", NotificationType.Error, ex);
				}
			}

			return false;
		}
	}
}
