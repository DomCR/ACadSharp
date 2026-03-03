using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.XData;
using CSMath;
using CSUtilities.Converters;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal abstract partial class DxfSectionWriterBase
	{
		public event NotificationEventHandler OnNotification;

		public abstract string SectionName { get; }

		public ACadVersion Version { get { return this._document.Header.Version; } }

		public CadObjectHolder Holder { get; }

		public DxfWriterConfiguration Configuration { get; }

		protected IDxfStreamWriter _writer;
		protected CadDocument _document;

		public DxfSectionWriterBase(
			IDxfStreamWriter writer,
			CadDocument document,
			CadObjectHolder holder,
			DxfWriterConfiguration configuration)
		{
			this._writer = writer;
			this._document = document;
			this.Holder = holder;
			this.Configuration = configuration;
		}

		public void Write()
		{
			this._writer.Write(DxfCode.Start, DxfFileToken.BeginSection);
			this._writer.Write(DxfCode.SymbolTableName, this.SectionName);

			this.writeSection();

			this._writer.Write(DxfCode.Start, DxfFileToken.EndSection);
		}

		protected void writeCadValue(CadValue value)
		{
			this._writer.Write(93, value.Flags);

			this._writer.Write(90, (int)value.ValueType);
			switch (value.ValueType)
			{
				case CadValueType.Unknown:
					this._writer.Write(91, 0);
					break;
				case CadValueType.Double:
					this._writer.Write(140, Convert.ToDouble(value.Value));
					break;
				case CadValueType.Date:
					//TODO: Implement date for dxf
					this._writer.Write(91, 0);
					break;
				case CadValueType.General:
				case CadValueType.String:
					this.writeLongTextValue(1, 2, (string)value.Value);
					break;
				case CadValueType.Point2D:
				case CadValueType.Point3D:
					this._writer.Write(11, (XYZ)value.Value);
					break;
				case CadValueType.Long:
					this._writer.Write(91, Convert.ToInt32(value.Value));
					break;
				case CadValueType.Handle:
					var handleObj = value.Value as IHandledCadObject;
					if (handleObj != null)
					{
						this._writer.WriteHandle(330, handleObj);
					}
					break;
				case CadValueType.Buffer:
				case CadValueType.ResultBuffer:
					//TODO: Implement date for dxf
					this._writer.Write(91, 0);
					break;
			}
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

			if (cadObject.XDictionary != null)
			{
				this._writer.Write(DxfCode.ControlString, DxfFileToken.DictionaryToken);
				this._writer.Write(DxfCode.HardOwnershipId, cadObject.XDictionary.Handle);
				this._writer.Write(DxfCode.ControlString, "}");

				//Add the dictionary in the object holder
				this.Holder.Objects.Enqueue(cadObject.XDictionary);
			}

			cadObject.CleanReactors();
			if (cadObject.Reactors.Any())
			{
				this._writer.Write(DxfCode.ControlString, DxfFileToken.ReactorsToken);
				foreach (var reactor in cadObject.Reactors)
				{
					this._writer.Write(DxfCode.SoftPointerId, reactor.Handle);
				}
				this._writer.Write(DxfCode.ControlString, "}");
			}

			this._writer.Write(DxfCode.SoftPointerId, cadObject.Owner.Handle);
		}

		protected void writeExtendedData(ExtendedDataDictionary xdata)
		{
			if (xdata == null || !this.Configuration.WriteXData)
			{
				return;
			}

			foreach (var entry in xdata)
			{
				this._writer.Write(DxfCode.ExtendedDataRegAppName, entry.Key.Name);

				foreach (ExtendedDataRecord record in entry.Value.Records)
				{
					switch (record)
					{
						case ExtendedDataBinaryChunk binaryChunk:
							this._writer.Write(binaryChunk.Code, binaryChunk.Value);
							break;
						case ExtendedDataControlString control:
							this._writer.Write(control.Code, control.Value);
							break;
						case ExtendedDataInteger16 s16:
							this._writer.Write(s16.Code, s16.Value);
							break;
						case ExtendedDataInteger32 s32:
							this._writer.Write(s32.Code, s32.Value);
							break;
						case ExtendedDataReal real:
							this._writer.Write(real.Code, real.Value);
							break;
						case ExtendedDataScale scale:
							this._writer.Write(scale.Code, scale.Value);
							break;
						case ExtendedDataDistance dist:
							this._writer.Write(dist.Code, dist.Value);
							break;
						case ExtendedDataDisplacement disp:
							this._writer.Write(disp.Code, disp.Value);
							break;
						case ExtendedDataDirection dir:
							this._writer.Write(dir.Code, (IVector)dir.Value);
							break;
						case ExtendedDataCoordinate coord:
							this._writer.Write(coord.Code, (IVector)coord.Value);
							break;
						case ExtendedDataWorldCoordinate wcoord:
							this._writer.Write(wcoord.Code, (IVector)wcoord.Value);
							break;
						case IExtendedDataHandleReference handle:
							ulong h = handle.Value;
							if (handle.ResolveReference(this._document) == null)
							{
								h = 0;
							}
							this._writer.Write(DxfCode.ExtendedDataHandle, h);
							break;
						case ExtendedDataString str:
							this._writer.Write(str.Code, str.Value);
							break;
						default:
							throw new System.NotSupportedException($"ExtendedDataRecord of type {record.GetType().FullName} not supported.");
					}
				}
			}
		}

		protected void writeCommonEntityData(Entity entity)
		{
			DxfClassMap map = DxfClassMap.Create<Entity>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Entity);

			this._writer.Write(8, entity.Layer.Name);

			this._writer.Write(6, entity.LineType.Name);

			if (entity.BookColor != null)
			{
				this._writer.Write(62, entity.BookColor.Color.GetApproxIndex());
				this._writer.WriteTrueColor(420, entity.BookColor.Color);
				this._writer.Write(430, entity.BookColor.Name);
			}
			else if (entity.Color.IsTrueColor)
			{
				this._writer.WriteTrueColor(420, entity.Color);
			}
			else
			{
				this._writer.Write(62, entity.Color.Index);
			}

			if (entity.Transparency.Value >= 0)
			{
				this._writer.Write(440, Transparency.ToAlphaValue(entity.Transparency));
			}

			this._writer.Write(48, entity.LineTypeScale, map);

			this._writer.Write(60, entity.IsInvisible ? (short)1 : (short)0, map);

			//TODO: Write if the layout is paperspace
			if (false)
			{
				this._writer.Write(67, (short)1);
			}

			this._writer.Write(370, entity.LineWeight);
		}

		protected abstract void writeSection();

		protected void writeLongTextValue(int code, int subcode, string text)
		{
			while (text.Length > 250)
			{
				this._writer.Write(subcode, text.Substring(0, 250));
				text = text.Remove(0, 250);
			}
			this._writer.Write(code, text);
		}

		protected void notify(string message, NotificationType notificationType = NotificationType.None, Exception ex = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, notificationType, ex));
		}
	}
}
