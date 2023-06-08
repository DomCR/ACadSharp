using ACadSharp.Objects;
using System;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfObjectsSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.ObjectsSection; } }

		public DxfObjectsSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
		{
		}

		protected override void writeSection()
		{
			while (this.Holder.Objects.Any())
			{
				CadObject item = this.Holder.Objects.Dequeue();

				this.writeObject(item);
			}
		}

		protected void writeObject<T>(T co)
			where T : CadObject
		{
			switch (co)
			{
				case CadDictionary cadDictionary:
					this.writeDictionary(cadDictionary);
					return;
				case Layout layout:
					this.writeMappedObject<Layout>(layout);
					break;
				case DictionaryVariable dictvar:
					this.writeMappedObject<DictionaryVariable>(dictvar);
					break;
				case SortEntitiesTable sortensTable:
					this.writeSortentsTable(sortensTable);
					break;
				case XRecrod record:
					this.writeXRecord(record);
					break;
				default:
					this.notify($"Object not implemented : {co.GetType().FullName}");
					break;
			}
		}

		protected void writeDictionary(CadDictionary e)
		{
			this._writer.Write(DxfCode.Start, e.ObjectName);

			this.writeCommonObjectData(e);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Dictionary);

			this._writer.Write(280, e.HardOwnerFlag);
			this._writer.Write(281, (int)e.ClonningFlags);

			System.Diagnostics.Debug.Assert(e.EntryNames.Length == e.EntryHandles.Length);
			for (int i = 0; i < e.EntryNames.Length; i++)
			{
				this._writer.Write(3, e.EntryNames[i]);
				this._writer.Write(350, e.EntryHandles[i]);
			}

			//Add the entries as objects
			foreach (CadObject item in e)
			{
				this.Holder.Objects.Enqueue(item);
			}
		}

		private void writeSortentsTable(SortEntitiesTable e)
		{
			if (e.BlockOwner == null)
			{
				//In some cases the block onwer is null in the files, this has to be checked
				this.notify("SortEntitiesTable with handle {e.Handle} has no block owner", NotificationType.Warning);
				return;
			}

			this._writer.Write(DxfCode.Start, e.ObjectName);

			this.writeCommonObjectData(e);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.XRecord);

			this._writer.Write(330, e.BlockOwner.Handle);


		}

		protected void writeXRecord(XRecrod e)
		{
			this._writer.Write(DxfCode.Start, e.ObjectName);

			this.writeCommonObjectData(e);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.XRecord);

			foreach (var item in e.Entries)
			{
				this._writer.Write(item.Code, item.Value);
			}
		}
	}
}
