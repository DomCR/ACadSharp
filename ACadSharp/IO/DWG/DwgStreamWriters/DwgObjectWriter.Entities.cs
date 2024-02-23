using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
		private void writeEntity(Entity entity)
		{
			List<Entity> children = new List<Entity>();
			Seqend seqend = null;

			//Ignored Entities
			switch (entity)
			{
				case AttributeEntity:
				case Shape:
				case Solid3D:
				case MultiLeader:
				case Mesh:
				//Unlisted
				case Wipeout:
					this.notify($"Entity type not implemented {entity.GetType().FullName}", NotificationType.NotImplemented);
					return;
			}

			this.writeCommonEntityData(entity);

			switch (entity)
			{
				case Arc arc:
					this.writeArc(arc);
					break;
				case Circle circle:
					this.writeCircle(circle);
					break;
				case Dimension dimension:
					this.writeCommonDimensionData(dimension);
					switch (dimension)
					{
						case DimensionLinear linear:
							this.writeDimensionLinear(linear);
							break;
						case DimensionAligned aligned:
							this.writeDimensionAligned(aligned);
							break;
						case DimensionRadius radius:
							this.writeDimensionRadius(radius);
							break;
						case DimensionAngular2Line angular2Line:
							this.writeDimensionAngular2Line(angular2Line);
							break;
						case DimensionAngular3Pt angular3pt:
							this.writeDimensionAngular3Pt(angular3pt);
							break;
						case DimensionDiameter diamenter:
							this.writeDimensionDiameter(diamenter);
							break;
						case DimensionOrdinate ordinate:
							this.writeDimensionOrdinate(ordinate);
							break;
						default:
							throw new NotImplementedException($"Dimension not implemented : {entity.GetType().FullName}");
					}
					break;
				case Ellipse ellipse:
					this.writeEllipse(ellipse);
					break;
				case Insert insert:
					this.writeInsert(insert);

					children.AddRange(insert.Attributes);

					seqend = insert.Attributes.Seqend;
					break;
				case Face3D face3D:
					this.writeFace3D(face3D);
					break;
				case Hatch hatch:
					this.writeHatch(hatch);
					break;
				case Leader l:
					this.writeLeader(l);
					break;
				case Line l:
					this.writeLine(l);
					break;
				case LwPolyline lwpolyline:
					this.writeLwPolyline(lwpolyline);
					break;
				case MLine mLine:
					this.writeMLine(mLine);
					break;
				case MText mtext:
					this.writeMText(mtext);
					break;
				case Point p:
					this.writePoint(p);
					break;
				case Polyline pline:
					switch (pline)
					{
						case PolyfaceMesh faceMesh:
							this.writePolyfaceMesh(faceMesh);
							children.AddRange(faceMesh.Faces);
							break;
						case Polyline2D pline2d:
							this.writePolyline2D(pline2d);
							break;
						case Polyline3D pline3d:
							this.writePolyline3D(pline3d);
							break;
						default:
							throw new NotImplementedException($"Polyline not implemented : {entity.GetType().FullName}");
					}
					children.AddRange(pline.Vertices);
					seqend = pline.Vertices.Seqend;
					break;
				case Ray ray:
					this.writeRay(ray);
					break;
				case Shape shape:
					this.writeShape(shape);
					break;
				case Solid solid:
					this.writeSolid(solid);
					break;
				case Solid3D solid3d:
					this.writeSolid3D(solid3d);
					break;
				case Spline spline:
					this.writeSpline(spline);
					break;
				case TextEntity text:
					switch (text)
					{
						case AttributeEntity att:
							this.writeAttribute(att);
							break;
						case AttributeDefinition attdef:
							this.writeAttDefinition(attdef);
							break;
						case TextEntity textEntity:
							this.writeTextEntity(textEntity);
							break;
						default:
							throw new NotImplementedException($"TextEntity not implemented : {entity.GetType().FullName}");
					}
					break;
				case Tolerance tolerance:
					this.writeTolerance(tolerance);
					break;
				case Vertex vertex:
					switch (vertex)
					{
						case Vertex2D vertex2D:
							this.writeVertex2D(vertex2D);
							break;
						case VertexFaceRecord faceRecord:
							this.writeFaceRecord(faceRecord);
							break;
						case Vertex3D:
						case VertexFaceMesh:
							this.writeVertex(vertex);
							break;
						default:
							throw new NotImplementedException($"Vertex not implemented : {entity.GetType().FullName}");
					}
					break;
				case Viewport viewport:
					this.writeViewport(viewport);
					break;
				case XLine xline:
					this.writeXLine(xline);
					break;
				default:
					throw new NotImplementedException($"Entity not implemented : {entity.GetType().FullName}");
			}

			this.registerObject(entity);

			this.writeChildEntities(children, seqend);
		}

		private void writeArc(Arc arc)
		{
			this.writeCircle(arc);

			this._writer.WriteBitDouble(arc.StartAngle);
			this._writer.WriteBitDouble(arc.EndAngle);
		}

		private void writeAttribute(AttributeEntity att)
		{
			this.writeCommonAttData(att);
		}

		private void writeAttDefinition(AttributeDefinition attdef)
		{
			this.writeCommonAttData(attdef);

			//R2010+:
			if (this.R2010Plus)
				//Version RC ?		Repeated??
				this._writer.WriteByte(attdef.Version);

			//Common:
			//Prompt TV 3
			this._writer.WriteVariableText(attdef.Prompt);
		}

		private void writeCommonAttData(AttributeBase att)
		{
			this.writeTextEntity(att);

			//R2010+:
			if (this.R2010Plus)
			{
				//Version RC ?
				this._writer.WriteByte(att.Version);
			}

			//R2018+:
			if (this.R2018Plus)
			{
				this._writer.WriteByte((byte)att.AttributeType);

				if (att.AttributeType == AttributeType.MultiLine || att.AttributeType == AttributeType.ConstantMultiLine)
				{
					throw new NotImplementedException("Multiple line Attribute not implemented");
				}
			}

			//Common:
			//Tag TV 2
			this._writer.WriteVariableText(att.Tag);
			//Field length BS 73 unused
			this._writer.WriteBitShort(0);
			//Flags RC 70 NOT bit-pair - coded.
			this._writer.WriteByte((byte)att.Flags);

			//R2007 +:
			if (this.R2007Plus)
			{
				//Lock position flag B 280
				this._writer.WriteBit(att.IsReallyLocked);
			}
		}

		private void writeCircle(Circle circle)
		{
			this._writer.Write3BitDouble(circle.Center);
			this._writer.WriteBitDouble(circle.Radius);
			this._writer.WriteBitThickness(circle.Thickness);
			this._writer.WriteBitExtrusion(circle.Normal);
		}

		private void writeCommonDimensionData(Dimension dimension)
		{
			//R2010:
			if (this.R2010Plus)
			{
				//Version RC 280 0 = R2010
				this._writer.WriteByte(dimension.Version);
			}

			//Common:
			//Extrusion 3BD 210
			this._writer.Write3BitDouble(dimension.Normal);
			//Text midpt 2RD 11 See DXF documentation.
			this._writer.Write2RawDouble((XY)dimension.TextMiddlePoint);
			//Elevation BD 11 Z - coord for the ECS points(11, 12, 16).
			//12 (The 16 remains (0,0,0) in entgets of this entity,
			//since the 16 is not used in this type of dimension
			//and is not present in the binary form here.)
			this._writer.WriteBitDouble(dimension.InsertionPoint.Z);

			byte flags = 0;
			flags |= dimension.IsTextUserDefinedLocation ? (byte)0b00 : (byte)0b01;

			this._writer.WriteByte(flags);

			//User text TV 1
			this._writer.WriteVariableText(dimension.Text);

			//Text rot BD 53 See DXF documentation.
			this._writer.WriteBitDouble(dimension.TextRotation);
			//Horiz dir BD 51 See DXF documentation.
			this._writer.WriteBitDouble(dimension.HorizontalDirection);

			//Ins X - scale BD 41 Undoc'd. These apply to the insertion of the
			//Ins Y - scale BD 42 anonymous block. None of them can be
			//Ins Z - scale BD 43 dealt with via entget/entmake/entmod.
			this._writer.Write3BitDouble(new XYZ());
			//Ins rotation BD 54 The last 2(43 and 54) are reported by DXFOUT(when not default values).
			//ALL OF THEM can be set via DXFIN, however.
			this._writer.WriteBitDouble(0);

			//R2000 +:
			if (this.R2000Plus)
			{
				//Attachment Point BS 71
				this._writer.WriteBitShort((short)dimension.AttachmentPoint);
				//Linespacing Style BS 72
				this._writer.WriteBitShort((short)dimension.LineSpacingStyle);
				//Linespacing Factor BD 41
				this._writer.WriteBitDouble(dimension.LineSpacingFactor);
				//Actual Measurement BD 42
				this._writer.WriteBitDouble(dimension.Measurement);
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//Unknown B 73
				this._writer.WriteBit(value: false);
				//Flip arrow1 B 74
				this._writer.WriteBit(value: false);
				//Flip arrow2 B 75
				this._writer.WriteBit(value: false);
			}

			//Common:
			//12 - pt 2RD 12 See DXF documentation.
			this._writer.Write2RawDouble((XY)dimension.InsertionPoint);

			//Common Entity Handle Data
			//H 3 DIMSTYLE(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimension.Style);
			//H 2 anonymous BLOCK(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dimension.Block);
		}

		private void writeDimensionLinear(DimensionLinear dimension)
		{
			this.writeDimensionAligned(dimension);

			//Dim rot BD 50 Linear dimension rotation; see DXF documentation.
			this._writer.WriteBitDouble(dimension.Rotation);
		}

		private void writeDimensionAligned(DimensionAligned dimension)
		{
			//Common:
			//13 - pt 3BD 13 See DXF documentation.
			this._writer.Write3BitDouble(dimension.FirstPoint);
			//14 - pt 3BD 14 See DXF documentation.
			this._writer.Write3BitDouble(dimension.SecondPoint);
			//10 - pt 3BD 10 See DXF documentation.
			this._writer.Write3BitDouble(dimension.DefinitionPoint);

			//Ext ln rot BD 52 Extension line rotation; see DXF documentation.
			this._writer.WriteBitDouble(dimension.ExtLineRotation);
		}

		private void writeDimensionRadius(DimensionRadius dimension)
		{
			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			this._writer.Write3BitDouble(dimension.DefinitionPoint);
			//15-pt 3BD 15 See DXF documentation.
			this._writer.Write3BitDouble(dimension.AngleVertex);
			//Leader len D 40 Leader length.
			this._writer.WriteBitDouble(dimension.LeaderLength);
		}

		private void writeDimensionAngular2Line(DimensionAngular2Line dimension)
		{
			//Common:
			//16-pt 2RD 16 See DXF documentation.
			this._writer.Write2RawDouble((XY)dimension.DimensionArc);

			//13 - pt 3BD 13 See DXF documentation.
			this._writer.Write3BitDouble(dimension.FirstPoint);
			//14 - pt 3BD 14 See DXF documentation.
			this._writer.Write3BitDouble(dimension.SecondPoint);
			//15-pt 3BD 15 See DXF documentation.
			this._writer.Write3BitDouble(dimension.AngleVertex);
			//10 - pt 3BD 10 See DXF documentation.
			this._writer.Write3BitDouble(dimension.DefinitionPoint);
		}

		private void writeDimensionAngular3Pt(DimensionAngular3Pt dimension)
		{
			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			this._writer.Write3BitDouble(dimension.DefinitionPoint);
			//13 - pt 3BD 13 See DXF documentation.
			this._writer.Write3BitDouble(dimension.FirstPoint);
			//14 - pt 3BD 14 See DXF documentation.
			this._writer.Write3BitDouble(dimension.SecondPoint);
			//15-pt 3BD 15 See DXF documentation.
			this._writer.Write3BitDouble(dimension.AngleVertex);
		}

		private void writeDimensionDiameter(DimensionDiameter dimension)
		{
			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			this._writer.Write3BitDouble(dimension.DefinitionPoint);
			//15-pt 3BD 15 See DXF documentation.
			this._writer.Write3BitDouble(dimension.AngleVertex);
			//Leader len D 40 Leader length.
			this._writer.WriteBitDouble(dimension.LeaderLength);
		}

		private void writeDimensionOrdinate(DimensionOrdinate dimension)
		{
			//Common:
			//10 - pt 3BD 10 See DXF documentation.
			this._writer.Write3BitDouble(dimension.DefinitionPoint);
			//13 - pt 3BD 13 See DXF documentation.
			this._writer.Write3BitDouble(dimension.FeatureLocation);
			//14 - pt 3BD 14 See DXF documentation.
			this._writer.Write3BitDouble(dimension.LeaderEndpoint);

			byte flag = (byte)(dimension.IsOrdinateTypeX ? 1 : 0);
			this._writer.WriteByte(flag);
		}

		private void writeEllipse(Ellipse ellipse)
		{
			this._writer.Write3BitDouble(ellipse.Center);
			this._writer.Write3BitDouble(ellipse.EndPoint);
			this._writer.Write3BitDouble(ellipse.Normal);
			this._writer.WriteBitDouble(ellipse.RadiusRatio);
			this._writer.WriteBitDouble(ellipse.StartParameter);
			this._writer.WriteBitDouble(ellipse.EndParameter);
		}

		private void writeInsert(Insert insert)
		{
			//Ins pt 3BD 10
			this._writer.Write3BitDouble(insert.InsertPoint);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//X Scale BD 41
				this._writer.WriteBitDouble(insert.XScale);
				//Y Scale BD 42
				this._writer.WriteBitDouble(insert.YScale);
				//Z Scale BD 43
				this._writer.WriteBitDouble(insert.ZScale);
			}

			//R2000 + Only:
			if (this.R2000Plus)
			{
				//Data flags BB
				//Scale Data Varies with Data flags:
				if (insert.XScale == 1.0 && insert.YScale == 1.0 && insert.ZScale == 1.0)
				{
					//11 - scale is (1.0, 1.0, 1.0), no data stored.
					this._writer.Write2Bits(3);
				}
				else if (insert.XScale == insert.YScale && insert.XScale == insert.ZScale)
				{
					//10 – 41 value stored as a RD, and 42 & 43 values are not stored, assumed equal to 41 value.
					this._writer.Write2Bits(2);
					this._writer.WriteRawDouble(insert.XScale);
				}
				else if (insert.XScale == 1.0)
				{
					//01 – 41 value is 1.0, 2 DD’s are present, each using 1.0 as the default value, representing the 42 and 43 values.
					this._writer.Write2Bits(1);
					this._writer.WriteBitDoubleWithDefault(insert.YScale, 1.0);
					this._writer.WriteBitDoubleWithDefault(insert.ZScale, 1.0);
				}
				else
				{
					//00 – 41 value stored as a RD, followed by a 42 value stored as DD (use 41 for default value), and a 43 value stored as a DD(use 41 value for default value).
					this._writer.Write2Bits(0);
					this._writer.WriteRawDouble(insert.XScale);
					this._writer.WriteBitDoubleWithDefault(insert.YScale, insert.XScale);
					this._writer.WriteBitDoubleWithDefault(insert.ZScale, insert.XScale);
				}
			}

			//Common:
			//Rotation BD 50
			this._writer.WriteBitDouble(insert.Rotation);
			//Extrusion 3BD 210
			this._writer.Write3BitDouble(insert.Normal);
			//Has ATTRIBs B 66 Single bit; 1 if ATTRIBs follow.
			this._writer.WriteBit(insert.HasAttributes);

			//R2004+:
			if (this.R2004Plus && insert.HasAttributes)
			{
				//Owned Object Count BL Number of objects owned by this object.
				this._writer.WriteBitLong(insert.Attributes.Count);
			}

			if (insert.ObjectType == ObjectType.MINSERT)
			{
				//Common:
				//Numcols BS 70
				this._writer.WriteBitShort((short)insert.ColumnCount);
				//Numrows BS 71
				this._writer.WriteBitShort((short)insert.RowCount);
				//Col spacing BD 44
				this._writer.WriteBitDouble(insert.ColumnSpacing);
				//Row spacing BD 45
				this._writer.WriteBitDouble(insert.RowSpacing);
			}

			//Common:
			//Common Entity Handle Data
			//H 2 BLOCK HEADER(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, insert.Block);

			if (!insert.HasAttributes)
			{
				return;
			}

			//R13 - R2000:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
			{
				this._writer.HandleReference(DwgReferenceType.SoftPointer, insert.Attributes.First());
				this._writer.HandleReference(DwgReferenceType.SoftPointer, insert.Attributes.Last());
			}
			//R2004+:
			else if (this.R2004Plus)
			{
				foreach (AttributeEntity att in insert.Attributes)
				{
					//H[ATTRIB(hard owner)] Repeats “Owned Object Count” times.
					this._writer.HandleReference(DwgReferenceType.HardOwnership, att);
				}
			}

			//Common:
			//H[SEQEND(hard owner)] if 66 bit set
			this._writer.HandleReference(DwgReferenceType.HardOwnership, insert.Attributes.Seqend);
		}

		private void writeFace3D(Face3D face)
		{
			//R13 - R14 Only:
			if (this.R13_14Only)
			{
				//1st corner 3BD 10
				this._writer.Write3BitDouble(face.FirstCorner);
				//2nd corner 3BD 11
				this._writer.Write3BitDouble(face.SecondCorner);
				//3rd corner 3BD 12
				this._writer.Write3BitDouble(face.ThirdCorner);
				//4th corner 3BD 13
				this._writer.Write3BitDouble(face.FourthCorner);
				//Invis flags BS 70 Invisible edge flags
				this._writer.WriteBitShort((short)face.Flags);
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				bool noFlags = face.Flags == InvisibleEdgeFlags.None;
				//Has no flag ind. B
				this._writer.WriteBit(noFlags);

				bool zIsZero = face.FirstCorner.Z == 0.0;
				//Z is zero bit B
				this._writer.WriteBit(zIsZero);

				//1st corner x RD 10
				this._writer.WriteRawDouble(face.FirstCorner.X);
				//1st corner y RD 20
				this._writer.WriteRawDouble(face.FirstCorner.Y);

				if (!zIsZero)
				{
					//1st corner z RD 30 Present only if “Z is zero bit” is 0.
					this._writer.WriteRawDouble(face.FirstCorner.Z);
				}

				//2nd corner 3DD 11 Use 10 value as default point
				this._writer.Write3BitDoubleWithDefault(face.SecondCorner, face.FirstCorner);
				//3rd corner 3DD 12 Use 11 value as default point
				this._writer.Write3BitDoubleWithDefault(face.ThirdCorner, face.SecondCorner);
				//4th corner 3DD 13 Use 12 value as default point
				this._writer.Write3BitDoubleWithDefault(face.FourthCorner, face.ThirdCorner);

				//Invis flags BS 70 Present it “Has no flag ind.” is 0.
				if (!noFlags)
				{
					this._writer.WriteBitShort((short)face.Flags);
				}
			}
		}

		private void writeMLine(MLine mline)
		{
			//Scale BD 40
			this._writer.WriteBitDouble(mline.ScaleFactor);
			//Just EC top (0), bottom(2), or center(1)
			this._writer.WriteByte((byte)mline.Justification);
			//Base point 3BD 10
			this._writer.Write3BitDouble(mline.StartPoint);
			//Extrusion 3BD 210 etc.
			this._writer.Write3BitDouble(mline.Normal);

			//Openclosed BS open (1), closed(3)
			this._writer.WriteBitShort((short)(mline.Flags.HasFlag(MLineFlags.Closed) ? 3 : 1));

			int nlines = 0;
			if (mline.Vertices.Count > 0)
			{
				nlines = mline.Vertices.First().Segments.Count;
			}
			//Linesinstyle RC 73
			this._writer.WriteByte((byte)nlines);

			//Numverts BS 72
			this._writer.WriteBitShort((short)mline.Vertices.Count);

			foreach (var v in mline.Vertices)
			{
				//vertex 3BD
				this._writer.Write3BitDouble(v.Position);
				//vertex direction 3BD
				this._writer.Write3BitDouble(v.Direction);
				//miter direction 3BD
				this._writer.Write3BitDouble(v.Miter);

				// All the Vertices must have the same number of segments
				for (int i = 0; i < nlines; i++)
				{
					var element = v.Segments[i];

					//numsegparms BS
					this._writer.WriteBitShort((short)element.Parameters.Count);
					foreach (double p in element.Parameters)
					{
						//segparm BD segment parameter
						this._writer.WriteBitDouble(p);
					}

					//numareafillparms BS
					this._writer.WriteBitShort((short)element.AreaFillParameters.Count);
					foreach (double afp in element.AreaFillParameters)
					{
						//areafillparm BD area fill parameter
						this._writer.WriteBitDouble(afp);
					}
				}
			}

			//H mline style oject handle (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, mline.MLStyle);
		}

		private void writeLwPolyline(LwPolyline lwPolyline)
		{
			bool nbulges = false;
			bool ndiffwidth = false;
			foreach (LwPolyline.Vertex item in lwPolyline.Vertices)
			{
				if (!nbulges && item.Bulge != 0.0)
				{
					nbulges = true;
				}
				if (!ndiffwidth && (item.StartWidth != 0.0 || item.EndWidth != 0.0))
				{
					ndiffwidth = true;
				}
			}

			short flags = 0;

			if (lwPolyline.Flags.HasFlag(LwPolylineFlags.Plinegen))
			{
				flags = (short)(flags | 0x100);
			}

			if (lwPolyline.Flags.HasFlag(LwPolylineFlags.Closed))
			{
				flags = (short)(flags | 0x200);
			}

			if (lwPolyline.ConstantWidth != 0.0)
			{
				flags = (short)(flags | 0x4);
			}

			if (lwPolyline.Elevation != 0.0)
			{
				flags = (short)(flags | 0x8);
			}

			if (lwPolyline.Thickness != 0.0)
			{
				flags = (short)(flags | 2);
			}

			if (lwPolyline.Normal != XYZ.AxisZ)
			{
				flags = (short)(flags | 1);
			}

			if (nbulges)
			{
				flags = (short)(flags | 0x10);
			}
			//Skip ids, not necessary
			if (ndiffwidth)
			{
				flags = (short)(flags | 0x20);
			}

			//B : bytes containing the LWPOLYLINE entity data.
			//This excludes the common entity data.
			//More specifically: it starts at the LWPOLYLINE flags (BS), and ends with the width array (BD).
			this._writer.WriteBitShort(flags);

			if (lwPolyline.ConstantWidth != 0.0)
			{
				this._writer.WriteBitDouble(lwPolyline.ConstantWidth);
			}
			if (lwPolyline.Elevation != 0.0)
			{
				this._writer.WriteBitDouble(lwPolyline.Elevation);
			}
			if (lwPolyline.Thickness != 0.0)
			{
				this._writer.WriteBitDouble(lwPolyline.Thickness);
			}
			if (lwPolyline.Normal != XYZ.AxisZ)
			{
				this._writer.Write3BitDouble(lwPolyline.Normal);
			}

			this._writer.WriteBitLong(lwPolyline.Vertices.Count);

			if (nbulges)
			{
				this._writer.WriteBitLong(lwPolyline.Vertices.Count);
			}

			if (ndiffwidth)
			{
				this._writer.WriteBitLong(lwPolyline.Vertices.Count);
			}

			if (this.R13_14Only)
			{
				for (int i = 0; i < lwPolyline.Vertices.Count; i++)
				{
					this._writer.Write2RawDouble(lwPolyline.Vertices[i].Location);
				}
			}

			if (this.R2000Plus && lwPolyline.Vertices.Count > 0)
			{
				LwPolyline.Vertex last = lwPolyline.Vertices[0];
				this._writer.Write2RawDouble(last.Location);
				for (int j = 1; j < lwPolyline.Vertices.Count; j++)
				{
					LwPolyline.Vertex curr = lwPolyline.Vertices[j];
					this._writer.Write2BitDoubleWithDefault(curr.Location, last.Location);
					last = curr;
				}
			}

			if (nbulges)
			{
				for (int k = 0; k < lwPolyline.Vertices.Count; k++)
				{
					this._writer.WriteBitDouble(lwPolyline.Vertices[k].Bulge);
				}
			}

			if (ndiffwidth)
			{
				for (int l = 0; l < lwPolyline.Vertices.Count; l++)
				{
					this._writer.WriteBitDouble(lwPolyline.Vertices[l].StartWidth);
					this._writer.WriteBitDouble(lwPolyline.Vertices[l].EndWidth);
				}
			}
		}

		private void writeHatch(Hatch hatch)
		{
			//R2004+:
			if (this.R2004Plus)
			{
				HatchGradientPattern gradient = hatch.GradientColor; //TODO: set default ?? HatchGradientPattern.Default;

				//Is Gradient Fill BL 450 Non-zero indicates a gradient fill is used.
				this._writer.WriteBitLong(gradient.Enabled ? 1 : 0);

				//Reserved BL 451
				this._writer.WriteBitLong(gradient.Reserved);
				//Gradient Angle BD 460
				this._writer.WriteBitDouble(gradient.Angle);
				//Gradient Shift BD 461
				this._writer.WriteBitDouble(gradient.Shift);
				//Single Color Grad.BL 452
				this._writer.WriteBitLong(gradient.IsSingleColorGradient ? 1 : 0);
				//Gradient Tint BD 462
				this._writer.WriteBitDouble(gradient.ColorTint);

				//# of Gradient Colors BL 453
				this._writer.WriteBitLong(gradient.Colors.Count);
				foreach (GradientColor color in gradient.Colors)
				{
					//Gradient Value double BD 463
					this._writer.WriteBitDouble(color.Value);
					//RGB Color
					this._writer.WriteCmColor(color.Color);
				}

				//Gradient Name TV 470
				this._writer.WriteVariableText(gradient.Name);
			}

			//Common:
			//Z coord BD 30 X, Y always 0.0
			this._writer.WriteBitDouble(hatch.Elevation);
			//Extrusion 3BD 210
			this._writer.Write3BitDouble(hatch.Normal);
			//Name TV 2 name of hatch
			this._writer.WriteVariableText(hatch.Pattern.Name);
			//Solidfill B 70 1 if solidfill, else 0
			this._writer.WriteBit(hatch.IsSolid);
			//Associative B 71 1 if associative, else 0
			this._writer.WriteBit(hatch.IsAssociative);

			//Numpaths BL 91 Number of paths enclosing the hatch
			this._writer.WriteBitLong(hatch.Paths.Count);
			bool hasDerivedBoundary = false;
			foreach (Hatch.BoundaryPath boundaryPath in hatch.Paths)
			{
				//Pathflag BL 92 Path flag
				this._writer.WriteBitLong((int)boundaryPath.Flags);

				if (boundaryPath.Flags.HasFlag(BoundaryPathFlags.Derived))
				{
					hasDerivedBoundary = true;
				}

				if (boundaryPath.Flags.HasFlag(BoundaryPathFlags.Polyline))
				{
					//TODO: Polyline may need to be treated different than the regular edges
					Hatch.BoundaryPath.Polyline pline = boundaryPath.Edges.First() as Hatch.BoundaryPath.Polyline;

					//bulgespresent B 72 bulges are present if 1
					this._writer.WriteBit(pline.HasBulge);
					//closed B 73 1 if closed
					this._writer.WriteBit(pline.IsClosed);

					//numpathsegs BL 91 number of path segments
					this._writer.WriteBitLong(pline.Vertices.Count);
					foreach (var vertex in pline.Vertices)
					{
						this._writer.Write2RawDouble(vertex);
						if (pline.HasBulge)
						{
							this._writer.WriteBitDouble(pline.Bulge);
						}
					}
				}
				else
				{
					//Numpathsegs BL 93 number of segments in this path
					this._writer.WriteBitLong(boundaryPath.Edges.Count);
					foreach (var edge in boundaryPath.Edges)
					{
						//pathtypestatus RC 72 type of path
						this._writer.WriteByte((byte)edge.Type);

						switch (edge)
						{
							case Hatch.BoundaryPath.Line line:
								//pt0 2RD 10 first endpoint
								this._writer.Write2RawDouble(line.Start);
								//pt1 2RD 11 second endpoint
								this._writer.Write2RawDouble(line.End);
								break;
							case Hatch.BoundaryPath.Arc arc:
								//pt0 2RD 10 center
								this._writer.Write2RawDouble(arc.Center);
								//radius BD 40 radius
								this._writer.WriteBitDouble(arc.Radius);
								//startangle BD 50 start angle
								this._writer.WriteBitDouble(arc.StartAngle);
								//endangle BD 51 endangle
								this._writer.WriteBitDouble(arc.EndAngle);
								//isccw B 73 1 if counter clockwise, otherwise 0
								this._writer.WriteBit(arc.CounterClockWise);
								break;
							case Hatch.BoundaryPath.Ellipse ellispe:
								//pt0 2RD 10 center
								this._writer.Write2RawDouble(ellispe.Center);
								//endpoint 2RD 11 endpoint of major axis
								this._writer.Write2RawDouble(ellispe.MajorAxisEndPoint);
								//minormajoratio BD 40 ratio of minor to major axis
								this._writer.WriteBitDouble(ellispe.MinorToMajorRatio);
								//startangle BD 50 start angle
								this._writer.WriteBitDouble(ellispe.StartAngle);
								//endangle BD 51 endangle
								this._writer.WriteBitDouble(ellispe.EndAngle);
								//isccw B 73 1 if counter clockwise; otherwise 0
								this._writer.WriteBit(ellispe.CounterClockWise);
								break;
							case Hatch.BoundaryPath.Spline splineEdge:
								//degree BL 94 degree of the spline
								this._writer.WriteBitLong(splineEdge.Degree);
								//isrational B 73 1 if rational(has weights), else 0
								this._writer.WriteBit(splineEdge.Rational);
								//isperiodic B 74 1 if periodic, else 0
								this._writer.WriteBit(splineEdge.Periodic);

								//numknots BL 95 number of knots
								this._writer.WriteBitLong(splineEdge.Knots.Count);
								//numctlpts BL 96 number of control points
								this._writer.WriteBitLong(splineEdge.ControlPoints.Count);
								foreach (double k in splineEdge.Knots)
								{
									//knot BD 40 knot value
									this._writer.WriteBitDouble(k);
								}

								for (int p = 0; p < splineEdge.ControlPoints.Count; ++p)
								{
									//pt0 2RD 10 control point
									this._writer.Write2RawDouble((XY)splineEdge.ControlPoints[p]);

									if (splineEdge.Rational)
										//weight BD 40 weight
										this._writer.WriteBitDouble(splineEdge.ControlPoints[p].Z);
								}

								//R24:
								if (this.R2010Plus)
								{
									//Numfitpoints BL 97 number of fit points
									this._writer.WriteBitLong(splineEdge.FitPoints.Count);
									if (splineEdge.FitPoints.Any())
									{
										foreach (XY fp in splineEdge.FitPoints)
										{
											//Fitpoint 2RD 11
											this._writer.Write2RawDouble(fp);
										}

										//Start tangent 2RD 12
										this._writer.Write2RawDouble(splineEdge.StartTangent);
										//End tangent 2RD 13
										this._writer.Write2RawDouble(splineEdge.EndTangent);
									}
								}
								break;
							default:
								throw new ArgumentException($"Unrecognized Boundary type: {boundaryPath.GetType().FullName}");
						}
					}
				}

				//numboundaryobjhandles BL 97 Number of boundary object handles for this path
				this._writer.WriteBitLong(boundaryPath.Entities.Count);
				foreach (Entity e in boundaryPath.Entities)
				{
					//boundaryhandle H 330 boundary handle(soft pointer)
					this._writer.HandleReference(DwgReferenceType.SoftPointer, e);
				}
			}

			//style BS 75 style of hatch 0==odd parity, 1==outermost, 2==whole area
			this._writer.WriteBitShort((short)hatch.Style);
			//patterntype BS 76 pattern type 0==user-defined, 1==predefined, 2==custom
			this._writer.WriteBitShort((short)hatch.PatternType);

			if (!hatch.IsSolid)
			{
				HatchPattern pattern = hatch.Pattern;
				this._writer.WriteBitDouble(hatch.PatternAngle);
				this._writer.WriteBitDouble(hatch.PatternScale);
				this._writer.WriteBit(hatch.IsDouble);

				_writer.WriteBitShort((short)pattern.Lines.Count);
				foreach (var line in pattern.Lines)
				{
					//angle BD 53 line angle
					_writer.WriteBitDouble(line.Angle);
					//pt0 2BD 43 / 44 pattern through this point(X, Y)
					_writer.Write2BitDouble(line.BasePoint);
					//offset 2BD 45 / 56 pattern line offset
					_writer.Write2BitDouble(line.Offset);

					//  numdashes BS 79 number of dash length items
					_writer.WriteBitShort((short)line.DashLengths.Count);
					foreach (double dl in line.DashLengths)
					{
						//dashlength BD 49 dash length
						_writer.WriteBitDouble(dl);
					}
				}
			}

			if (hasDerivedBoundary)
			{
				//pixelsize BD 47 pixel size
				this._writer.WriteBitDouble(hatch.PixelSize);
			}

			//numseedpoints BL 98 number of seed points
			this._writer.WriteBitLong(hatch.SeedPoints.Count);
			foreach (XY sp in hatch.SeedPoints)
			{
				//pt0 2RD 10 seed point
				this._writer.Write2RawDouble(sp);
			}
		}

		private void writeLeader(Leader leader)
		{
			//Unknown bit B --- Always seems to be 0.
			this._writer.WriteBit(false);

			//Annot type BS --- Annotation type (NOT bit-coded):
			this._writer.WriteBitShort((short)leader.CreationType);
			//path type BS ---
			this._writer.WriteBitShort((short)leader.PathType);

			//numpts BL --- number of points
			this._writer.WriteBitLong(leader.Vertices.Count);
			foreach (XYZ v in leader.Vertices)
			{
				//point 3BD 10 As many as counter above specifies.
				this._writer.Write3BitDouble(v);
			}

			//Origin 3BD --- The leader plane origin (by default it’s the first point).
			this._writer.Write3BitDouble(leader.Vertices.FirstOrDefault());
			//Extrusion 3BD 210
			this._writer.Write3BitDouble(leader.Normal);
			//x direction 3BD 211
			this._writer.Write3BitDouble(leader.HorizontalDirection);
			//offsettoblockinspt 3BD 212 Used when the BLOCK option is used. Seems to be an unused feature.
			this._writer.Write3BitDouble(leader.BlockOffset);

			//R14+:
			if (this._version >= ACadVersion.AC1014)
			{
				//Endptproj 3BD --- A non-planar leader gives a point that projects the endpoint back to the annotation.
				this._writer.Write3BitDouble(leader.AnnotationOffset);
			}

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//DIMGAP BD --- The value of DIMGAP in the associated DIMSTYLE at the time of creation, multiplied by the dimscale in that dimstyle.
				this._writer.WriteBitDouble(leader.Style.DimensionLineGap);
			}


			//Common:
			if (this._version <= ACadVersion.AC1021)
			{
				//Box height BD 40 MTEXT extents height. (A text box is slightly taller, probably by some DIMvar amount.)
				this._writer.WriteBitDouble(leader.TextHeight);
				//Box width BD 41 MTEXT extents width. (A text box is slightly wider, probably by some DIMvar amount.)
				this._writer.WriteBitDouble(leader.TextWidth);
			}

			//Hooklineonxdir B hook line is on x direction if 1
			this._writer.WriteBit(leader.HookLineDirection);
			//Arrowheadon B arrowhead on indicator
			this._writer.WriteBit(leader.ArrowHeadEnabled);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Arrowheadtype BS arrowhead type
				this._writer.WriteBitShort(0);
				//Dimasz BD DIMASZ at the time of creation, multiplied by DIMSCALE
				this._writer.WriteBitDouble(leader.Style.ArrowSize * leader.Style.ScaleFactor);
				//Unknown B
				this._writer.WriteBit(false);
				//Unknown B
				this._writer.WriteBit(false);
				//Unknown BS
				this._writer.WriteBitShort(0);
				//Byblockcolor BS
				this._writer.WriteBitShort(0);
				//Unknown B
				this._writer.WriteBit(false);
				//Unknown B
				this._writer.WriteBit(false);
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//Unknown BS
				this._writer.WriteBitShort(0);
				//Unknown B
				this._writer.WriteBit(false);
				//Unknown B
				this._writer.WriteBit(false);
			}

			//H 340 Associated annotation
			this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			//H 2 DIMSTYLE (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, leader.Style);
		}

		private void writeLine(Line line)
		{
			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Start pt 3BD 10
				this._writer.Write3BitDouble(line.StartPoint);
				//End pt 3BD 11
				this._writer.Write3BitDouble(line.EndPoint);
			}


			//R2000+:
			if (this.R2000Plus)
			{
				//Z’s are zero bit B
				bool flag = line.StartPoint.Z == 0.0 && line.EndPoint.Z == 0.0;
				this._writer.WriteBit(flag);

				//Start Point x RD 10
				this._writer.WriteRawDouble(line.StartPoint.X);
				//End Point x DD 11 Use 10 value for default
				this._writer.WriteBitDoubleWithDefault(line.EndPoint.X, line.StartPoint.X);
				//Start Point y RD 20
				this._writer.WriteRawDouble(line.StartPoint.Y);
				//End Point y DD 21 Use 20 value for default
				this._writer.WriteBitDoubleWithDefault(line.EndPoint.Y, line.StartPoint.Y);

				if (!flag)
				{
					//Start Point z RD 30 Present only if “Z’s are zero bit” is 0
					this._writer.WriteRawDouble(line.StartPoint.Z);
					//End Point z DD 31 Present only if “Z’s are zero bit” is 0, use 30 value for default.
					this._writer.WriteBitDoubleWithDefault(line.EndPoint.Z, line.StartPoint.Z);
				}
			}

			//Common:
			//Thickness BT 39
			this._writer.WriteBitThickness(line.Thickness);
			//Extrusion BE 210
			this._writer.WriteBitExtrusion(line.Normal);
		}

		private void writePoint(Point point)
		{
			//Point 3BD 10
			this._writer.Write3BitDouble(point.Location);
			//Thickness BT 39
			this._writer.WriteBitThickness(point.Thickness);
			//Extrusion BE 210
			this._writer.WriteBitExtrusion(point.Normal);
			//X - axis ang BD 50 See DXF documentation
			this._writer.WriteBitDouble(point.Rotation);
		}

		private void writePolyfaceMesh(PolyfaceMesh fm)
		{
			//Numverts BS 71 Number of vertices in the mesh.
			this._writer.WriteBitShort((short)fm.Vertices.Count);
			//Numfaces BS 72 Number of faces
			this._writer.WriteBitShort((short)fm.Faces.Count);

			//R2004 +:
			if (this.R2004Plus)
			{
				//Owned Object Count BL Number of objects owned by this object.
				this._writer.WriteBitLong(fm.Vertices.Count + fm.Faces.Count);
				foreach (var v in fm.Vertices)
				{
					//H[VERTEX(soft pointer)] Repeats “Owned Object Count” times.
					this._writer.HandleReference(DwgReferenceType.SoftPointer, v);
				}
				foreach (var f in fm.Faces)
				{
					this._writer.HandleReference(DwgReferenceType.SoftPointer, f);
				}
			}

			//R13 - R2000:
			if (this.R13_15Only)
			{
				List<CadObject> child = new List<CadObject>(fm.Vertices);
				child.AddRange(fm.Faces);

				CadObject first = child.FirstOrDefault();
				CadObject last = child.LastOrDefault();

				//H first VERTEX(soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, first);
				//H last VERTEX(soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, last);
			}

			//Common:
			//H SEQEND(hard owner)
			this._writer.HandleReference(DwgReferenceType.SoftPointer, fm.Vertices.Seqend);
		}

		private void writePolyline2D(Polyline2D pline)
		{
			//Flags BS 70
			this._writer.WriteBitShort((short)pline.Flags);
			//Curve type BS 75 Curve and smooth surface type.
			this._writer.WriteBitShort((short)pline.SmoothSurface);
			//Start width BD 40 Default start width
			this._writer.WriteBitDouble(pline.StartWidth);
			//End width BD 41 Default end width
			this._writer.WriteBitDouble(pline.EndWidth);
			//Thickness BT 39
			this._writer.WriteBitThickness(pline.Thickness);
			//Elevation BD 10 The 10-pt is (0,0,elev)
			this._writer.WriteBitDouble(pline.Elevation);
			//Extrusion BE 210
			this._writer.WriteBitExtrusion(pline.Normal);

			int count = pline.Vertices.Count;
			//R2004+:
			if (this.R2004Plus)
			{
				//Owned Object Count BL Number of objects owned by this object.
				this._writer.WriteBitLong(count);
				for (int i = 0; i < count; i++)
				{
					this._writer.HandleReference(DwgReferenceType.HardOwnership, pline.Vertices[i]);
				}
			}

			//R13-R2000:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
			{
				//H first VERTEX (soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, pline.Vertices.FirstOrDefault());
				//H last VERTEX (soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, pline.Vertices.LastOrDefault());
			}

			//Common:
			//H SEQEND(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, pline.Vertices.Seqend);
		}

		private void writePolyline3D(Polyline3D pline)
		{
			//Flags RC 70 NOT DIRECTLY THE 75. Bit-coded (76543210):
			//75 0 : Splined(75 value is 5)
			//1 : Splined(75 value is 6)
			//Should assign pline.SmoothSurface ??
			this._writer.WriteByte(0);

			//Flags RC 70 NOT DIRECTLY THE 70. Bit-coded (76543210):
			//0 : Closed(70 bit 0(1))
			//(Set 70 bit 3(8) because this is a 3D POLYLINE.)
			this._writer.WriteByte((byte)(pline.Flags.HasFlag(PolylineFlags.ClosedPolylineOrClosedPolygonMeshInM) ? 1 : 0));

			//R2004+:
			if (this.R2004Plus)
			{
				//Owned Object Count BL Number of objects owned by this object.
				this._writer.WriteBitLong(pline.Vertices.Count);

				foreach (var vertex in pline.Vertices)
				{
					this._writer.HandleReference(DwgReferenceType.HardOwnership, vertex);
				}
			}

			//R13-R2000:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
			{
				//H first VERTEX (soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, pline.Vertices.FirstOrDefault());
				//H last VERTEX (soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, pline.Vertices.LastOrDefault());
			}

			//Common:
			//H SEQEND(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, pline.Vertices.Seqend);
		}

		private void writeSeqend(Seqend seqend)
		{
			//for empty list seqend is null
			if (seqend == null)
				return;

			//Seqend does not have links for AC1015 or before (causes errors)
			Entity prevHolder = this._prev;
			Entity nextHolder = this._next;
			this._prev = null;
			this._next = null;

			this.writeCommonEntityData(seqend);
			this.registerObject(seqend);

			this._prev = prevHolder;
			this._next = nextHolder;
		}

		private void writeShape(Shape shape)
		{
			//Ins pt 3BD 10
			this._writer.Write3BitDouble(shape.InsertionPoint);
			//Scale BD 40 Scale factor, default value 1.
			this._writer.WriteBitDouble(shape.Size);
			//Rotation BD 50 Rotation in radians, default value 0.
			this._writer.WriteBitDouble(shape.Rotation);
			//Width factor BD 41 Width factor, default value 1.
			this._writer.WriteBitDouble(shape.RelativeXScale);
			//Oblique BD 51 Oblique angle in radians, default value 0.
			this._writer.WriteBitDouble(shape.ObliqueAngle);
			//Thickness BD 39
			this._writer.WriteBitDouble(shape.Thickness);

			//Shapeno BS 2
			//This is the shape index.
			//In DXF the shape name is stored.
			//When reading from DXF, the shape is found by iterating over all the text styles
			//(SHAPEFILE, see paragraph 20.4.56) and when the text style contains a shape file,
			//iterating over all the shapes until the one with the matching name is found.
			this._writer.WriteBitShort(0);  //TODO: missing implementation for shapeIndex

			//Extrusion 3BD 210
			this._writer.Write3BitDouble(shape.Normal);

			//H SHAPEFILE (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, null);
		}

		private void writeSolid(Solid solid)
		{
			this.writeCommonEntityData(solid);

			//Thickness BT 39
			this._writer.WriteBitThickness(solid.Thickness);

			//Elevation BD ---Z for 10 - 13.
			this._writer.WriteBitDouble((double)solid.FirstCorner.Z);

			//1st corner 2RD 10
			this._writer.WriteRawDouble(solid.FirstCorner.X);
			this._writer.WriteRawDouble(solid.FirstCorner.Y);
			//2nd corner 2RD 11
			this._writer.WriteRawDouble(solid.SecondCorner.X);
			this._writer.WriteRawDouble(solid.SecondCorner.Y);
			//3rd corner 2RD 12
			this._writer.WriteRawDouble(solid.ThirdCorner.X);
			this._writer.WriteRawDouble(solid.ThirdCorner.Y);
			//4th corner 2RD 13
			this._writer.WriteRawDouble(solid.FirstCorner.X);
			this._writer.WriteRawDouble(solid.FirstCorner.Y);

			//Extrusion BE 210
			this._writer.WriteBitExtrusion(solid.Normal);
		}

		private void writeSolid3D(Solid3D solid)
		{
		}

		private void writeSpline(Spline spline)
		{
			int scenario;
			//R2013+:
			if (this.R2013Plus)
			{
				//The scenario flag becomes 1 if the knot parameter is Custom or has no fit data, otherwise 2.
				if (spline.KnotParameterization == KnotParameterization.Custom || spline.FitPoints.Count == 0)
				{
					scenario = 1;
				}
				else
				{
					scenario = 2;
				}

				this._writer.WriteBitLong(scenario);
				this._writer.WriteBitLong((int)spline.Flags1);
				this._writer.WriteBitLong((int)spline.KnotParameterization);
			}
			else
			{
				scenario = (spline.FitPoints.Count <= 0) ? 1 : 2;
				if (scenario == 2 && spline.KnotParameterization != 0)
				{
					scenario = 1;
				}

				//Scenario BL a flag which is 2 for fitpts only, 1 for ctrlpts/knots.
				this._writer.WriteBitLong(scenario);
			}

			//Common:
			//Degree BL degree of this spline
			this._writer.WriteBitLong(spline.Degree);

			bool flag = spline.Weights.Count > 0;
			switch (scenario)
			{
				case 1:
					{
						//Rational B flag bit 2
						this._writer.WriteBit(spline.Flags.HasFlag(SplineFlags.Rational));
						//Closed B flag bit 0
						this._writer.WriteBit(spline.Flags.HasFlag(SplineFlags.Closed));
						//Periodic B flag bit 1
						this._writer.WriteBit(spline.Flags.HasFlag(SplineFlags.Periodic));

						//Knot tol BD 42
						this._writer.WriteBitDouble(spline.KnotTolerance);
						//Ctrl tol BD 43
						this._writer.WriteBitDouble(spline.ControlPointTolerance);

						//Numknots BL 72 This is stored as a LONG
						this._writer.WriteBitLong(spline.Knots.Count);
						//Numctrlpts BL 73 Number of 10's (and 41's, if weighted) that follow.
						this._writer.WriteBitLong(spline.ControlPoints.Count);

						//Weight B Seems to be an echo of the 4 bit on the flag for "weights present".
						this._writer.WriteBit(flag);

						foreach (double k in spline.Knots)
						{
							//Knot BD knot value
							this._writer.WriteBitDouble(k);
						}

						for (int i = 0; i < spline.ControlPoints.Count; i++)
						{
							//Control pt 3BD 10
							this._writer.Write3BitDouble(spline.ControlPoints[i]);
							if (flag)
							{
								//Weight D 41 if present as indicated by 4 bit on flag
								this._writer.WriteBitDouble(spline.Weights[i]);
							}
						}
						break;
					}
				case 2:
					{
						//Fit Tol BD 44
						this._writer.WriteBitDouble(spline.FitTolerance);
						//Beg tan vec 3BD 12 Beginning tangent direction vector (normalized).
						this._writer.Write3BitDouble(spline.StartTangent);
						//End tan vec 3BD 13 Ending tangent direction vector (normalized).
						this._writer.Write3BitDouble(spline.EndTangent);
						//num fit pts BL 74 Number of fit points.
						this._writer.WriteBitLong(spline.FitPoints.Count);

						foreach (XYZ fp in spline.FitPoints)
						{
							//Fit pt 3BD
							this._writer.Write3BitDouble(fp);
						}
						break;
					}
			}
		}

		private void writeRay(Ray ray)
		{
			//Point 3BD 10
			this._writer.Write3BitDouble(ray.StartPoint);
			//Vector 3BD 11
			this._writer.Write3BitDouble(ray.Direction);
		}

		private void writeTextEntity(TextEntity text)
		{
			//R13-14 Only:
			if (this.R13_14Only)
			{
				//Elevation BD ---
				this._writer.WriteBitDouble(text.InsertPoint.Z);
				//Insertion pt 2RD 10
				this._writer.WriteRawDouble(text.InsertPoint.X);
				this._writer.WriteRawDouble(text.InsertPoint.Y);

				//Alignment pt 2RD 11
				this._writer.WriteRawDouble(text.AlignmentPoint.X);
				this._writer.WriteRawDouble(text.AlignmentPoint.Y);

				//Extrusion 3BD 210
				this._writer.Write3BitDouble(text.Normal);
				//Thickness BD 39
				this._writer.WriteBitDouble(text.Thickness);
				//Oblique ang BD 51
				this._writer.WriteBitDouble(text.ObliqueAngle);
				//Rotation ang BD 50
				this._writer.WriteBitDouble(text.Rotation);
				//Height BD 40
				this._writer.WriteBitDouble(text.Height);
				//Width factor BD 41
				this._writer.WriteBitDouble(text.WidthFactor);
				//Text value TV 1
				this._writer.WriteVariableText(text.Value);
				//Generation BS 71
				this._writer.WriteBitShort((short)text.Mirror);
				//Horiz align. BS 72
				this._writer.WriteBitShort((short)text.HorizontalAlignment);
				//Vert align. BS 73
				this._writer.WriteBitShort((short)text.VerticalAlignment);

			}
			else
			{
				//DataFlags RC Used to determine presence of subsquent data
				byte dataFlags = 0;

				if (text.InsertPoint.Z == 0.0)
				{
					dataFlags = (byte)(dataFlags | 0b1);
				}
				if (text.AlignmentPoint == XYZ.Zero)
				{
					dataFlags = (byte)(dataFlags | 0b10);
				}
				if (text.ObliqueAngle == 0.0)
				{
					dataFlags = (byte)(dataFlags | 0b100);
				}
				if (text.Rotation == 0.0)
				{
					dataFlags = (byte)(dataFlags | 0b1000);
				}
				if (text.WidthFactor == 1.0)
				{
					dataFlags = (byte)(dataFlags | 0b10000);
				}
				if (text.Mirror == TextMirrorFlag.None)
				{
					dataFlags = (byte)(dataFlags | 0b100000);
				}
				if (text.HorizontalAlignment == TextHorizontalAlignment.Left)
				{
					dataFlags = (byte)(dataFlags | 0b1000000);
				}
				if (text.VerticalAlignment == TextVerticalAlignmentType.Baseline)
				{
					dataFlags = (byte)(dataFlags | 0b10000000);
				}

				this._writer.WriteByte(dataFlags);

				//Elevation RD --- present if !(DataFlags & 0x01)
				if ((dataFlags & 0b1) == 0)
					this._writer.WriteRawDouble(text.InsertPoint.Z);

				//Insertion pt 2RD 10
				this._writer.WriteRawDouble(text.InsertPoint.X);
				this._writer.WriteRawDouble(text.InsertPoint.Y);

				//Alignment pt 2DD 11 present if !(DataFlags & 0x02), use 10 & 20 values for 2 default values.
				if ((dataFlags & 0x2) == 0)
				{
					this._writer.WriteBitDoubleWithDefault(text.AlignmentPoint.X, text.InsertPoint.X);
					this._writer.WriteBitDoubleWithDefault(text.AlignmentPoint.Y, text.InsertPoint.Y);
				}

				//Extrusion BE 210
				this._writer.WriteBitExtrusion(text.Normal);
				//Thickness BT 39
				this._writer.WriteBitThickness(text.Thickness);

				//Oblique ang RD 51 present if !(DataFlags & 0x04)
				if ((dataFlags & 0x4) == 0)
					this._writer.WriteRawDouble(text.ObliqueAngle);
				//Rotation ang RD 50 present if !(DataFlags & 0x08)
				if ((dataFlags & 0x8) == 0)
					this._writer.WriteRawDouble(text.Rotation);

				//Height RD 40
				this._writer.WriteRawDouble(text.Height);

				//Width factor RD 41 present if !(DataFlags & 0x10)
				if ((dataFlags & 0x10) == 0)
					this._writer.WriteRawDouble(text.WidthFactor);

				//Text value TV 1
				this._writer.WriteVariableText(text.Value);

				//Generation BS 71 present if !(DataFlags & 0x20)
				if ((dataFlags & 0x20) == 0)
					this._writer.WriteBitShort((short)text.Mirror);
				//Horiz align. BS 72 present if !(DataFlags & 0x40)
				if ((dataFlags & 0x40) == 0)
					this._writer.WriteBitShort((short)text.HorizontalAlignment);
				//Vert align. BS 73 present if !(DataFlags & 0x80)
				if ((dataFlags & 0x80) == 0)
					this._writer.WriteBitShort((short)text.VerticalAlignment);
			}

			//Common:
			//Common Entity Handle Data H 7 STYLE(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, text.Style);
		}

		private void writeMText(MText mtext)
		{

			//Insertion pt3 BD 10 First picked point. (Location relative to text depends on attachment point (71).)
			this._writer.Write3BitDouble(mtext.InsertPoint);
			//Extrusion 3BD 210 Undocumented; appears in DXF and entget, but ACAD doesn't even bother to adjust it to unit length.
			this._writer.Write3BitDouble(mtext.Normal);
			//X-axis dir 3BD 11 Apparently the text x-axis vector. (Why not just a rotation?) ACAD maintains it as a unit vector.
			this._writer.Write3BitDouble(mtext.AlignmentPoint);
			//Rect width BD 41 Reference rectangle width (width picked by the user).
			this._writer.WriteBitDouble(mtext.RectangleWidth);

			//R2007+:
			if (this.R2007Plus)
			{
				this._writer.WriteBitDouble(mtext.RectangleHeight);
			}

			//Common:
			//Text height BD 40 Undocumented
			this._writer.WriteBitDouble(mtext.Height);
			//Attachment BS 71 Similar to justification; see DXF doc
			this._writer.WriteBitShort((short)mtext.AttachmentPoint);
			//Drawing dir BS 72 Left to right, etc.; see DXF doc
			this._writer.WriteBitShort((short)mtext.DrawingDirection);

			//Extents ht BD ---Undocumented and not present in DXF or entget
			this._writer.WriteBitDouble(0);
			//Extents wid BD ---Undocumented and not present in DXF or entget
			this._writer.WriteBitDouble(0);

			//Text TV 1 All text in one long string
			this._writer.WriteVariableText(mtext.Value);

			//H 7 STYLE (hard pointer)
			this._writer.HandleReference(mtext.Style);

			//R2000+:
			if (this.R2000Plus)
			{
				//Linespacing Style BS 73
				this._writer.WriteBitShort((short)mtext.LineSpacingStyle);
				//Linespacing Factor BD 44
				this._writer.WriteBitDouble(mtext.LineSpacing);
				//Unknown bit B
				this._writer.WriteBit(false);
			}

			//R2004+:
			if (this.R2004Plus)
			{
				//Background flags BL 90 0 = no background, 1 = background fill, 2 = background fill with drawing fill color, 0x10 = text frame (R2018+)
				this._writer.WriteBitLong((int)mtext.BackgroundFillFlags);

				//background flags has bit 0x01 set, or in case of R2018 bit 0x10:
				if ((mtext.BackgroundFillFlags & BackgroundFillFlags.UseBackgroundFillColor)
					!= BackgroundFillFlags.None
					|| this._version > ACadVersion.AC1027
					&& (mtext.BackgroundFillFlags & BackgroundFillFlags.TextFrame) > 0)
				{
					//Background scale factor	BL 45 default = 1.5
					this._writer.WriteBitDouble(mtext.BackgroundScale);
					//Background color CMC 63
					this._writer.WriteCmColor(mtext.BackgroundColor);
					//Background transparency BL 441
					this._writer.WriteBitLong(mtext.BackgroundTransparency.Value);
				}
			}

			//R2018+
			if (!this.R2018Plus)
				return;

			//Is NOT annotative B
			this._writer.WriteBit(!mtext.IsAnnotative);

			//IF MTEXT is not annotative
			if (mtext.IsAnnotative)
			{
				return;
			}

			//Version BS Default 0
			this._writer.WriteBitShort(0);
			//Default flag B Default true
			this._writer.WriteBit(true);

			//BEGIN REDUNDANT FIELDS(see above for descriptions)
			//Registered application H Hard pointer
			this._writer.HandleReference(DwgReferenceType.HardPointer, null);

			//Attachment point BL
			this._writer.WriteBitLong((int)mtext.AttachmentPoint);
			//X - axis dir 3BD 10
			this._writer.Write3BitDouble(mtext.AlignmentPoint);
			//Insertion point 3BD 11
			this._writer.Write3BitDouble(mtext.InsertPoint);
			//Rect width BD 40
			this._writer.WriteBitDouble(mtext.Height);
			//Rect height BD 41
			this._writer.WriteBitDouble(mtext.RectangleWidth);
			//Extents width BD 42
			this._writer.WriteBitDouble(mtext.HorizontalWidth);
			//Extents height BD 43
			this._writer.WriteBitDouble(mtext.VerticalWidth);
			//END REDUNDANT FIELDS

			//Column type BS 71 0 = No columns, 1 = static columns, 2 = dynamic columns
			this._writer.WriteBitShort((short)mtext.Column.ColumnType);

			//IF Has Columns data(column type is not 0)
			if (mtext.Column.ColumnType != ColumnType.NoColumns)
			{
				//Column height count BL 72
				this._writer.WriteBitLong(mtext.Column.ColumnCount);
				//Columnn width BD 44
				this._writer.WriteBitDouble(mtext.Column.ColumnWidth);
				//Gutter BD 45
				this._writer.WriteBitDouble(mtext.Column.ColumnGutter);
				//Auto height? B 73
				this._writer.WriteBit(mtext.Column.ColumnAutoHeight);
				//Flow reversed? B 74
				this._writer.WriteBit(mtext.Column.ColumnFlowReversed);

				//IF not auto height and column type is dynamic columns
				if (!mtext.Column.ColumnAutoHeight && mtext.Column.ColumnType == ColumnType.DynamicColumns)
				{
					foreach (double h in mtext.Column.ColumnHeights)
					{
						//Column height BD 46
						this._writer.WriteBitDouble(h);
					}
				}
			}
		}

		private void writeFaceRecord(VertexFaceRecord face)
		{
			this.writeCommonEntityData(face);

			//Vert index BS 71 1 - based vertex index(see DXF doc)
			this._writer.WriteBitShort(face.Index1);
			//Vert index BS 72 1 - based vertex index(see DXF doc)
			this._writer.WriteBitShort(face.Index2);
			//Vert index BS 73 1 - based vertex index(see DXF doc)
			this._writer.WriteBitShort(face.Index3);
			//Vert index BS 74 1 - based vertex index(see DXF doc)
			this._writer.WriteBitShort(face.Index4);
		}

		private void writeVertex2D(Vertex2D vertex)
		{
			//Flags EC 70 NOT bit-pair-coded.
			this._writer.WriteByte((byte)(vertex.Flags));

			//Point 3BD 10 NOTE THAT THE Z SEEMS TO ALWAYS BE 0.0! The Z must be taken from the 2D POLYLINE elevation.
			this._writer.WriteBitDouble(vertex.Location.X);
			this._writer.WriteBitDouble(vertex.Location.Y);
			this._writer.WriteBitDouble(0.0);

			//Start width BD 40 If it's negative, use the abs val for start AND end widths (and note that no end width will be present).
			//This is a compression trick for cases where the start and end widths are identical and non-0.
			if (vertex.StartWidth != 0.0 && vertex.EndWidth == vertex.StartWidth)
			{
				this._writer.WriteBitDouble(0.0 - (double)vertex.StartWidth);
			}
			else
			{
				this._writer.WriteBitDouble(vertex.StartWidth);
				//End width BD 41 Not present if the start width is < 0.0; see above.
				this._writer.WriteBitDouble(vertex.EndWidth);
			}

			//Bulge BD 42
			this._writer.WriteBitDouble(vertex.Bulge);

			//R2010+:
			if (this.R2010Plus)
			{
				//Vertex ID BL 91
				this._writer.WriteBitLong(vertex.Id);
			}

			//Common:
			//Tangent dir BD 50
			this._writer.WriteBitDouble(vertex.CurveTangent);
		}

		private void writeVertex(Vertex vertex)
		{
			this.writeCommonEntityData(vertex);

			//Flags EC 70 NOT bit-pair-coded.
			this._writer.WriteByte((byte)vertex.Flags);
			//Point 3BD 10
			this._writer.Write3BitDouble(vertex.Location);
		}

		private void writeTolerance(Tolerance tolerance)
		{
			this.writeCommonEntityData(tolerance);

			//R13 - R14 Only:
			if (this.R13_14Only)
			{
				//Unknown short S
				this._writer.WriteBitShort(0);
				//Height BD --
				this._writer.WriteBitDouble(0.0);
				//Dimgap(?) BD dimgap at time of creation, *dimscale
				this._writer.WriteBitDouble(0.0);
			}

			//Common:
			//Ins pt 3BD 10
			this._writer.Write3BitDouble(tolerance.InsertionPoint);
			//X direction 3BD 11
			this._writer.Write3BitDouble(tolerance.Direction);
			//Extrusion 3BD 210 etc.
			this._writer.Write3BitDouble(tolerance.Normal);
			//Text string BS 1
			this._writer.WriteVariableText(tolerance.Text);

			//Common Entity Handle Data
			//H DIMSTYLE(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, tolerance.Style);
		}

		private void writeViewport(Viewport viewport)
		{
			//Center 3BD 10
			this._writer.Write3BitDouble(viewport.Center);
			//Width BD 40
			this._writer.WriteBitDouble(viewport.Width);
			//Height BD 41
			this._writer.WriteBitDouble(viewport.Height);

			//R2000 +:
			if (this.R2000Plus)
			{
				//View Target 3BD 17
				this._writer.Write3BitDouble(viewport.ViewTarget);
				//View Direction 3BD 16
				this._writer.Write3BitDouble(viewport.ViewDirection);
				//View Twist Angle BD 51
				this._writer.WriteBitDouble(viewport.TwistAngle);
				//View Height BD 45
				this._writer.WriteBitDouble(viewport.ViewHeight);
				//Lens Length BD 42
				this._writer.WriteBitDouble(viewport.LensLength);
				//Front Clip Z BD 43
				this._writer.WriteBitDouble(viewport.FrontClipPlane);
				//Back Clip Z BD 44
				this._writer.WriteBitDouble(viewport.BackClipPlane);
				//Snap Angle BD 50
				this._writer.WriteBitDouble(viewport.SnapAngle);
				//View Center 2RD 12
				this._writer.Write2RawDouble(viewport.ViewCenter);
				//Snap Base 2RD 13
				this._writer.Write2RawDouble(viewport.SnapBase);
				//Snap Spacing 2RD 14
				this._writer.Write2RawDouble(viewport.SnapSpacing);
				//Grid Spacing 2RD 15
				this._writer.Write2RawDouble(viewport.GridSpacing);
				//Circle Zoom BS 72
				this._writer.WriteBitShort(viewport.CircleZoomPercent);
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//Grid Major BS 61
				this._writer.WriteBitShort(viewport.MajorGridLineFrequency);
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				//Frozen Layer Count BL
				this._writer.WriteBitLong(viewport.FrozenLayers.Count);
				//Status Flags BL 90
				this._writer.WriteBitLong((int)viewport.Status);
				//Style Sheet TV 1
				this._writer.WriteVariableText(string.Empty);   //This is never used
																//Render Mode RC 281
				this._writer.WriteByte((byte)viewport.RenderMode);
				//UCS at origin B 74
				this._writer.WriteBit(viewport.DisplayUcsIcon);
				//UCS per Viewport B 71
				this._writer.WriteBit(viewport.UcsPerViewport);
				//UCS Origin 3BD 110
				this._writer.Write3BitDouble(viewport.UcsOrigin);
				//UCS X Axis 3BD 111
				this._writer.Write3BitDouble(viewport.UcsXAxis);
				//UCS Y Axis 3BD 112
				this._writer.Write3BitDouble(viewport.UcsYAxis);
				//UCS Elevation BD 146
				this._writer.WriteBitDouble(viewport.Elevation);
				//UCS Ortho View Type BS 79
				this._writer.WriteBitShort((short)viewport.UcsOrthographicType);
			}

			//R2004 +:
			if (this.R2004Plus)
			{
				//ShadePlot Mode BS 170
				this._writer.WriteBitShort((short)viewport.ShadePlotMode);
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//Use def. lights B 292
				this._writer.WriteBit(viewport.UseDefaultLighting);
				//Def.lighting type RC 282
				this._writer.WriteByte((byte)viewport.DefaultLightingType);
				//Brightness BD 141
				this._writer.WriteBitDouble(viewport.Brightness);
				//Contrast BD 142
				this._writer.WriteBitDouble(viewport.Constrast);
				//Ambient light color CMC 63
				this._writer.WriteCmColor(viewport.AmbientLightColor);
			}

			//R13 - R14 Only:
			if (this.R13_14Only)
			{
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				foreach (var layer in viewport.FrozenLayers)
				{
					if (this.R2004Plus)
					{
						//H 341 Frozen Layer Handles(use count from above)
						//(hard pointer until R2000, soft pointer from R2004 onwards)
						this._writer.HandleReference(DwgReferenceType.SoftPointer, layer);
					}
					else
					{
						this._writer.HandleReference(DwgReferenceType.HardPointer, layer);
					}
				}

				//H 340 Clip boundary handle(soft pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, viewport.Boundary);
			}

			//R2000:
			if (this._version == ACadVersion.AC1015)
			{
				//H VIEWPORT ENT HEADER((hard pointer))
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				//TODO: Implement viewport UCS
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//H 332 Background(soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, null);
				//H 348 Visual Style(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H 333 Shadeplot ID(soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, null);
				//H 361 Sun(hard owner)
				this._writer.HandleReference(DwgReferenceType.HardOwnership, null);
			}
		}

		private void writeXLine(XLine xline)
		{
			//3 RD: a point on the construction line
			this._writer.Write3BitDouble(xline.FirstPoint);
			//3 RD : another point
			this._writer.Write3BitDouble(xline.Direction);
		}

		private void writeChildEntities(IEnumerable<Entity> entities, Seqend seqend)
		{
			if (!entities.Any())
				return;

			Entity prevHolder = this._prev;
			Entity nextHolder = this._next;
			this._prev = null;
			this._next = null;

			Entity curr = entities.First();
			for (int i = 1; i < entities.Count(); i++)
			{
				this._next = entities.ElementAt(i);
				this.writeEntity(curr);
				this._prev = curr;
				curr = this._next;
			}

			this._next = null;
			this.writeEntity(curr);

			this._prev = prevHolder;
			this._next = nextHolder;

			if (seqend != null)
			{
				this.writeSeqend(seqend);
			}
		}
	}
}
