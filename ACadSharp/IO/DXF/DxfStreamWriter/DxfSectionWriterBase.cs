﻿using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfSectionWriterBase
	{
		public event NotificationEventHandler OnNotification;

		public abstract string SectionName { get; }

		public CadObjectHolder Holder { get; }

		protected IDxfStreamWriter _writer;
		protected CadDocument _document;

		public DxfSectionWriterBase(
			IDxfStreamWriter writer,
			CadDocument document,
			CadObjectHolder holder)
		{
			this._writer = writer;
			this._document = document;
			this.Holder = holder;
		}

		public void Write()
		{
			this._writer.Write(DxfCode.Start, DxfFileToken.BeginSection);
			this._writer.Write(DxfCode.SymbolTableName, this.SectionName);

			this.writeSection();

			this._writer.Write(DxfCode.Start, DxfFileToken.EndSection);
		}

		public void Notify(string message)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message));
		}

		protected void writeCommonObjectData(CadObject cadObject)
		{
			if (cadObject is DimensionStyle)
			{
				this._writer.Write(DxfCode.DimVarHandle, cadObject.Handle);
			}
			else
			{
				this._writer.Write(DxfCode.Handle, cadObject.Handle);
			}

			this._writer.Write(DxfCode.SoftPointerId, cadObject.Owner.Handle);

			//TODO: Write exended data
			if (cadObject.ExtendedData != null)
			{
				//this._writer.Write(DxfCode.ControlString, "{ACAD_REACTORS");
				//this._writer.Write(DxfCode.HardOwnershipId, cadObject.ExtendedData);
				//this._writer.Write(DxfCode.ControlString, "}");
			}

			if (cadObject.XDictionary != null)
			{
				this._writer.Write(DxfCode.ControlString, "{ACAD_XDICTIONARY");
				this._writer.Write(DxfCode.HardOwnershipId, cadObject.XDictionary.Handle);
				this._writer.Write(DxfCode.ControlString, "}");

				//Add the dictionary in the object holder
				this.Holder.Objects.Enqueue(cadObject.XDictionary);
			}
		}

		protected void writeMap(DxfMap map, CadObject cadObject)
		{
			foreach (var item in map.SubClasses)
			{
				writeClassMap(item.Value, cadObject);
			}
		}

		protected void writeClassMap(DxfClassMap cmap, CadObject cadObject)
		{
			this._writer.Write(DxfCode.Subclass, cmap.Name);

			foreach (KeyValuePair<int, DxfProperty> v in cmap.DxfProperties)
			{
				int code = v.Key;
				DxfProperty prop = v.Value;

				if (prop.ReferenceType.HasFlag(DxfReferenceType.Ignored)
					|| prop.ReferenceType.HasFlag(DxfReferenceType.Optional))
					continue;

				object value = v.Value.GetValue(v.Key, cadObject);
				if (value == null)
				{
					continue;
				}

				this._writer.Write(v.Key, value);

				if (prop.ReferenceType.HasFlag(DxfReferenceType.Count))
				{
					this.writeCollection((IEnumerable)prop.GetValue(cadObject), prop.GetCollectionCodes());
				}
			}
		}

		protected void writeCollection(IEnumerable arr, DxfCode[] codes = null)
		{
			foreach (var item in arr)
			{
				switch (item)
				{
					case double d:
						this.writeDoubles(codes, d);
						break;
					case XY xy:
						this.writeDoubles(codes, xy.GetComponents());
						break;
					case XYZ xyz:
						this.writeDoubles(codes, xyz.GetComponents());
						break;
					case LineType.Segment segment:
						this.writeSegment(segment);
						break;
					case LwPolyline.Vertex vertex:
						this.writeLwVertex(vertex);
						break;
					case MLine.Vertex mvertex:
						this.writeLwVertex(mvertex);
						break;
					case AttributeEntity att:
						this.writeMappedObject(att);
						break;
					case Vertex vertex:
						this.writeVertex(vertex);
						break;
					default:
						this.Notify($"counter value for : {item.GetType().FullName} not implemented");
						break;
				}
			}

			if (arr is ISeqendColleciton colleciton)
			{
				this.writeMappedObject(colleciton.Seqend);
			}
		}

		protected void writeMappedObject<T>(T e)
			where T : CadObject
		{
			switch (e)
			{
				case Hatch:
				case MText:
				case Dimension:
				case MLine:
					this.Notify($"mapped object : {e.GetType().FullName} not implemented | handle: {e.Handle}");
#if TEST
					throw new NotImplementedException($"mapped object : {e.GetType().FullName} not implemented | handle: {e.Handle}");
#endif
					return;
				case Insert insert:
					this.writeInsert(insert);
					return;
				case LwPolyline lwPolyline:
					this.writeLwPolyline(lwPolyline);
					return;
				case Polyline polyline:
					this.writePolyline(polyline);
					return;
				case TextEntity textEntity:
					this.writeTextEntity(textEntity);
					return;
				default:
					break;
			}

			DxfMap map = DxfMap.Create(e.GetType());

			this._writer.Write(DxfCode.Start, e.ObjectName);

			this.writeCommonObjectData(e);

			this.writeMap(map, e);
		}

		protected abstract void writeSection();

		private void writeDoubles(DxfCode[] codes, params double[] arr)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				this._writer.Write(codes[i], arr[i]);
			}
		}

		private void writeSegment(LineType.Segment segment)
		{
			this._writer.Write(49, segment.Length);

			this._writer.Write(74, (short)segment.Shapeflag);

			if ((short)segment.Shapeflag == 0)
				return;

			this._writer.Write(75, segment.ShapeNumber);
			this._writer.Write(340, segment.Style.Handle);
			this._writer.Write(46, segment.Scale);
			this._writer.Write(50, segment.Rotation);
			this._writer.Write(44, segment.Offset.X);
			this._writer.Write(45, segment.Offset.Y);

			if (segment.Shapeflag.HasFlag(LinetypeShapeFlags.Text))
			{
				this._writer.Write(9, string.IsNullOrEmpty(segment.Text) ? string.Empty : segment.Text);
			}
		}

		private void writeLwVertex(MLine.Vertex v)
		{
			this._writer.Write(11, v.Position.X);
			this._writer.Write(21, v.Position.Y);
			this._writer.Write(31, v.Position.Z);

			this._writer.Write(12, v.Direction.X);
			this._writer.Write(22, v.Direction.Y);
			this._writer.Write(32, v.Direction.Z);

			this._writer.Write(13, v.Miter.X);
			this._writer.Write(23, v.Miter.Y);
			this._writer.Write(33, v.Miter.Z);
		}

		private void writeLwVertex(LwPolyline.Vertex v)
		{
			this._writer.Write(10, v.Location.X);
			this._writer.Write(20, v.Location.Y);
			this._writer.Write(40, v.StartWidth);
			this._writer.Write(41, v.EndWidth);
			this._writer.Write(42, v.Bulge);
			this._writer.Write(70, (short)v.Flags);
			this._writer.Write(50, v.CurveTangent);
			this._writer.Write(91, v.Id);
		}

		private void writeInsert(Insert insert)
		{
			DxfClassMap entityMap = DxfClassMap.Create<Entity>();
			DxfClassMap insertMap = DxfClassMap.Create<Insert>();

			this._writer.Write(DxfCode.Start, insert.ObjectName);

			this.writeCommonObjectData(insert);

			this.writeClassMap(entityMap, insert);

			this.writeClassMap(insertMap, insert);

			//this._writer.Write(66, 0);

			//if (insert.Attributes.Any())
			if (false)
			{
				this._writer.Write(66, 1);
				this.writeCollection(insert.Attributes);
			}
		}

		private void writeLwPolyline(LwPolyline polyline)
		{
			DxfClassMap entityMap = DxfClassMap.Create<Entity>();
			DxfClassMap plineMap = DxfClassMap.Create<LwPolyline>();

			this._writer.Write(DxfCode.Start, polyline.ObjectName);

			this.writeCommonObjectData(polyline);

			this.writeClassMap(entityMap, polyline);

			this.writeClassMap(plineMap, polyline);

			this.writeCollection(polyline.Vertices);
		}

		private void writePolyline(Polyline polyline)
		{
			DxfClassMap entityMap = DxfClassMap.Create<Entity>();
			DxfClassMap plineMap = null;

			this._writer.Write(DxfCode.Start, polyline.ObjectName);

			this.writeCommonObjectData(polyline);

			this.writeClassMap(entityMap, polyline);

			switch (polyline)
			{
				case Polyline2D:
					plineMap = DxfClassMap.Create<Polyline2D>();
					break;
				case Polyline3D:
					plineMap = DxfClassMap.Create<Polyline3D>();
					break;
			}

			//Remove elevation
			plineMap.DxfProperties.Remove(30);

			this.writeClassMap(plineMap, polyline);

			this._writer.Write(DxfCode.XCoordinate, 0);
			this._writer.Write(DxfCode.YCoordinate, 0);
			this._writer.Write(DxfCode.ZCoordinate, polyline.Elevation);

			this.writeCollection(polyline.Vertices);
		}

		private void writeTextEntity(TextEntity text)
		{
			DxfClassMap entityMap = DxfClassMap.Create<Entity>();
			DxfClassMap textMap = DxfClassMap.Create<TextEntity>();

			this._writer.Write(DxfCode.Start, text.ObjectName);

			this.writeCommonObjectData(text);

			this.writeClassMap(entityMap, text);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Text);

			this._writer.Write(1, text.Value);

			this._writer.Write(10, text.InsertPoint.X);
			this._writer.Write(20, text.InsertPoint.Y);
			this._writer.Write(30, text.InsertPoint.Z);

			this._writer.Write(40, text.Height);

			if (text.WidthFactor != 1.0)
			{
				this._writer.Write(41, text.WidthFactor);
			}

			if (text.Rotation != 0.0)
			{
				this._writer.Write(50, text.Rotation);
			}

			if (text.ObliqueAngle != 0.0)
			{
				this._writer.Write(51, text.ObliqueAngle);
			}

			if (text.Style != null)
			{
				//TODO: Implement text style in the writer
				//this._writer.Write(7, text.Style.Name);
			}

			this._writer.Write(11, text.AlignmentPoint.X);
			this._writer.Write(21, text.AlignmentPoint.Y);
			this._writer.Write(31, text.AlignmentPoint.Z);

			this._writer.Write(210, text.Normal.X);
			this._writer.Write(220, text.Normal.Y);
			this._writer.Write(230, text.Normal.Z);

			if (text is not AttributeBase)
			{
				if (text.Mirror != 0)
				{
					this._writer.Write(71, text.Mirror);
				}
				if (text.HorizontalAlignment != 0)
				{
					this._writer.Write(72, text.HorizontalAlignment);
				}

				this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Text);

				if (text.VerticalAlignment != 0)
				{
					this._writer.Write(73, text.VerticalAlignment);
				}
			}
			else
			{
				DxfClassMap attMap = null;

				switch (text)
				{
					case AttributeEntity:
						attMap = DxfClassMap.Create<AttributeEntity>();
						break;
					case AttributeDefinition:
						attMap = DxfClassMap.Create<AttributeDefinition>();
						break;
				}

				this.writeClassMap(attMap, text);
			}
		}

		private void writeVertex(Vertex v)
		{
			DxfMap map = DxfMap.Create(v.GetType());

			this._writer.Write(DxfCode.Start, v.ObjectName);

			this.writeCommonObjectData(v);

			this.writeMap(map, v);
		}
	}
}
