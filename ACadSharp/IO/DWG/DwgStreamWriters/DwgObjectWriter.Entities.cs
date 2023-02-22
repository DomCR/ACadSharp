using ACadSharp.Entities;
using CSMath;
using System;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
		private void writeEntity(Entity entity)
		{
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
							writeDimensionLinear(linear);
							break;
						case DimensionAligned aligned:
							writeDimensionAligned(aligned);
							break;
						case DimensionRadius radius:
							writeDimensionRadius(radius);
							break;
						case DimensionAngular2Line angular2Line:
							writeDimensionAngular2Line(angular2Line);
							break;
						case DimensionAngular3Pt angular3pt:
							writeDimensionAngular3Pt(angular3pt);
							break;
						case DimensionDiameter diamenter:
							writeDimensionDiameter(diamenter);
							break;
						case DimensionOrdinate ordinate:
							writeDimensionOrdinate(ordinate);
							break;
							//default:
							//	throw new NotImplementedException();
					}
					break;
				case Ellipse ellipse:
					this.writeEllipse(ellipse);
					break;
				case Line l:
					this.writeLine(l);
					break;
				case Point p:
					this.writePoint(p);
					break;
				case TextEntity text:
					this.writeTextEntity(text);
					break;
				default:
					this.notify($"Entity not implemented : {entity.GetType().FullName}", NotificationType.NotImplemented);
					return;
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
			writeDimensionAligned(dimension);

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
			this._writer.WriteBitDouble(mtext.RectangleWitdth);

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
	}
}
