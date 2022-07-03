using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DXF;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections;
using System.Collections.Generic;

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
				this._writer.Write(DxfCode.Subclass, item.Key);

				foreach (KeyValuePair<int, DxfProperty> v in item.Value.DxfProperties)
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
						this.writeCollection((IEnumerable)prop.GetValue(cadObject));
					}
				}
			}
		}

		protected void writeCollection(IEnumerable arr)
		{
			foreach (var item in arr)
			{
				switch (item)
				{
					case double d:
						break;
					case XY xy:
						break;
					case XYZ xyz:
						break;
					case LineType.Segment segment:
						this.writeSegment(segment);
						break;
					case LwPolyline.Vertex vertex:
						this.writeLwVertex(vertex);
						break;
					case MLine.Vertex vertex:
						break;
					default:
						this.Notify($"counter value for : {item.GetType().FullName} not implemented");
						break;
				}
			}
		}

		protected void writeMappedObject<T>(T e)
			where T : CadObject
		{
			//TODO: Finish write implementation
			if (
				e is Hatch
				|| e is MText
				|| e is AttributeDefinition
				)
			{
				return;
			}

			DxfMap map = DxfMap.Create(e.GetType());

			this._writer.Write(DxfCode.Start, e.ObjectName);

			this.writeCommonObjectData(e);

			this.writeMap(map, e);
		}

		protected abstract void writeSection();

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
	}
}
