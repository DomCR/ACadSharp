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
			//Ignored Entities
			switch (entity)
			{
				case Insert:
				case MText:
				//Unlisted
				case Wipeout:
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
				case Face3D face3D:
					this.writeFace3D(face3D);
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
				case Point p:
					this.writePoint(p);
					break;
				case PolyfaceMesh faceMesh:
					this.writePolyfaceMesh(faceMesh);
					break;
				case Polyline3D pline3d:
					this.writePolyline3D(pline3d);
					break;
				case Ray ray:
					this.writeRay(ray);
					break;
				case Solid solid:
					this.writeSolid(solid);
					break;
				case Spline spline:
					this.writeSpline(spline);
					break;
				case TextEntity text:
					this.writeTextEntity(text);
					break;
				case Vertex vertex:
					switch (vertex)
					{
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
				case XLine xline:
					this.writeXLine(xline);
					break;
				default:
					this.notify($"Entity not implemented : {entity.GetType().FullName}", NotificationType.NotImplemented);
					throw new NotImplementedException($"Entity not implemented : {entity.GetType().FullName}");
			}

			this.registerObject(entity);
		}

		private void writeArc(Arc arc)
		{
			//this.writeCircle(arc);
			this._writer.Write3BitDouble(arc.Center);
			this._writer.WriteBitDouble(arc.Radius);
			this._writer.WriteBitThickness(arc.Thickness);
			this._writer.WriteBitExtrusion(arc.Normal);

			this._writer.WriteBitDouble(arc.StartAngle);
			this._writer.WriteBitDouble(arc.EndAngle);
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

			this._writer.WriteByte(0);

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

			this.writePolyfaceMeshEntities(fm);
		}

		[Obsolete("Use writeChildEntities instead")]
		private void writePolyfaceMeshEntities(PolyfaceMesh fm)
		{
			Entity prevHolder = this._prev;
			Entity nextHolder = this._next;

			this._prev = null;
			if (fm.Vertices.Any())
			{
				Vertex currVertex = fm.Vertices.First();
				for (int i = 1; i < fm.Vertices.Count; i++)
				{
					Vertex nextVertex = (Vertex)(this._next = fm.Vertices[i]);

					// To avoid issues is better to enforce the flags
					currVertex.Flags = VertexFlags.PolygonMesh3D | VertexFlags.PolyfaceMeshVertex;
					this.writeEntity(currVertex);

					this._prev = currVertex;
					currVertex = nextVertex;
				}

				// Get the next entity for the last vertex, the first Face
				this._next = fm.Faces.FirstOrDefault();

				// To avoid issues is better to enforce the flags
				currVertex.Flags = VertexFlags.PolygonMesh3D | VertexFlags.PolyfaceMeshVertex;
				this.writeEntity(currVertex);

				this._prev = currVertex;
			}

			if (fm.Faces.Any())
			{
				VertexFaceRecord currFace = fm.Faces.First();
				for (int j = 1; j < fm.Faces.Count; j++)
				{
					VertexFaceRecord nextFace = (VertexFaceRecord)(this._next = fm.Faces[j]);
					this.writeEntity(currFace);
					this._prev = currFace;
					currFace = nextFace;
				}
				this._next = null;
				this.writeEntity(currFace);
			}

			this.writeSeqend(fm.Vertices.Seqend);

			this._prev = prevHolder;
			this._next = nextHolder;
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

			this.writeChildEntities(pline.Vertices);

			this.writeSeqend(pline.Vertices.Seqend);
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
				this._writer.WriteBitDouble(mtext.ReferenceRectangleHeight);
			}

			//Common:
			//Text height BD 40 Undocumented
			this._writer.WriteBitDouble(mtext.Height);
			//Attachment BS 71 Similar to justification; see DXF doc
			this._writer.WriteBitShort((short)mtext.AttachmentPoint);
			//Drawing dir BS 72 Left to right, etc.; see DXF doc
			this._writer.WriteBitShort((short)mtext.DrawingDirection);

			//TODO: Check undocumented values for MText
			//Extents ht BD ---Undocumented and not present in DXF or entget
			this._writer.WriteBitDouble(0);
			//Extents wid BD ---Undocumented and not present in DXF or entget
			this._writer.WriteBitDouble(0);

			//Text TV 1 All text in one long string (Autocad format)
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

			throw new System.NotImplementedException("Annotative MText not implemented for the writer");
			//TODO: missing values depending on the reader to get them and process to be able to write
#if false
			//Version BS Default 0
			this._writer.WriteBitShort(0);
			//Default flag B Default true
			this._writer.WriteBit(true);

			//BEGIN REDUNDANT FIELDS(see above for descriptions)
			//Registered application H Hard pointer
			this._writer.HandleReference(DwgReferenceType.HardPointer, null);

			//TODO: finish Mtext Writer, save redundant fields??

			//Attachment point BL
			AttachmentPointType attachmentPoint = (AttachmentPointType)this._writer.WriteBitLong();
			//X - axis dir 3BD 10
			this._writer.Write3BitDouble();
			//Insertion point 3BD 11
			this._writer.Write3BitDouble();
			//Rect width BD 40
			this._writer.WriteBitDouble();
			//Rect height BD 41
			this._writer.WriteBitDouble();
			//Extents width BD 42
			this._writer.WriteBitDouble();
			//Extents height BD 43
			this._writer.WriteBitDouble();
			//END REDUNDANT FIELDS

			//Column type BS 71 0 = No columns, 1 = static columns, 2 = dynamic columns
			this._writer.WriteBitShort((short)mtext.Column.ColumnType);
			//IF Has Columns data(column type is not 0)
			if (mtext.Column.ColumnType != ColumnType.NoColumns)
			{
				//Column height count BL 72
				int count = this._writer.WriteBitLong();
				//Columnn width BD 44
				mtext.Column.ColumnWidth = this._writer.WriteBitDouble();
				//Gutter BD 45
				mtext.Column.ColumnGutter = this._writer.WriteBitDouble();
				//Auto height? B 73
				mtext.Column.ColumnAutoHeight = this._writer.WriteBit();
				//Flow reversed? B 74
				mtext.Column.ColumnFlowReversed = this._writer.WriteBit();

				//IF not auto height and column type is dynamic columns
				if (!mtext.Column.ColumnAutoHeight && mtext.Column.ColumnType == ColumnType.DynamicColumns && count > 0)
				{
					for (int i = 0; i < count; ++i)
					{
						//Column height BD 46
						mtext.Column.ColumnHeights.Add(this._writer.WriteBitDouble());
					}
				}
			}
#endif
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

		private void writeVertex(Vertex vertex)
		{
			this.writeCommonEntityData(vertex);

			//Flags EC 70 NOT bit-pair-coded.
			this._writer.WriteByte((byte)vertex.Flags);
			//Point 3BD 10
			this._writer.Write3BitDouble(vertex.Location);
		}

		private void writeXLine(XLine xline)
		{
			//3 RD: a point on the construction line
			this._writer.Write3BitDouble(xline.FirstPoint);
			//3 RD : another point
			this._writer.Write3BitDouble(xline.Direction);
		}

		private void writeChildEntities(IEnumerable<Entity> entities)
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
		}
	}
}
