using ACadSharp.Entities;
using ACadSharp.Tables;
using System;

namespace ACadSharp.IO.DXF
{
	internal abstract partial class DxfSectionWriterBase
	{
		public event NotificationEventHandler OnNotification;

		public abstract string SectionName { get; }

		public ACadVersion Version { get { return this._document.Header.Version; } }

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
				//this._writer.Write(DxfCode.ControlString,DxfFileToken.ReactorsToken);
				//this._writer.Write(DxfCode.HardOwnershipId, cadObject.ExtendedData);
				//this._writer.Write(DxfCode.ControlString, "}");
			}

			if (cadObject.XDictionary != null)
			{
				this._writer.Write(DxfCode.ControlString, DxfFileToken.DictionaryToken);
				this._writer.Write(DxfCode.HardOwnershipId, cadObject.XDictionary.Handle);
				this._writer.Write(DxfCode.ControlString, "}");

				//Add the dictionary in the object holder
				this.Holder.Objects.Enqueue(cadObject.XDictionary);
			}
		}

		protected void writeExtendedData(CadObject cadObject)
		{
		}

		protected void writeCommonEntityData(Entity entity)
		{
			DxfClassMap map = DxfClassMap.Create<Entity>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Entity);

			this._writer.Write(8, entity.Layer.Name);

			this._writer.Write(6, entity.LineType.Name);

			if (entity.Color.IsTrueColor)
			{
				this._writer.Write(420, entity.Color.TrueColor);
			}
			else
			{
				this._writer.Write(62, entity.Color.Index);
			}

			if (entity.Transparency.Value >= 0)
			{
				//this._writer.Write(440, entity.Transparency.Value);
			}

			this._writer.Write(48, entity.LinetypeScale, map);

			this._writer.Write(60, entity.IsInvisible ? (short)1 : (short)0, map);

			//TODO: Write if the layout is paperspace
			if (false)
			{
				this._writer.Write(67, (short)1);
			}

			this._writer.Write(370, entity.LineWeight);
		}

		protected abstract void writeSection();

		protected void notify(string message, NotificationType notificationType = NotificationType.None, Exception ex = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, notificationType, ex));
		}
	}
}
