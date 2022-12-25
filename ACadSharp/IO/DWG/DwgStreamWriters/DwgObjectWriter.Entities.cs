using ACadSharp.Entities;
using CSMath;

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
	}
}
