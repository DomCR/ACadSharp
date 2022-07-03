using ACadSharp.Objects;
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
			DxfMap map = null;

			switch (co)
			{
				case CadDictionary cadDictionary:
					this.writeDictionary(cadDictionary);
					return;
				case Layout layout:
					this.writeMappedObject<Layout>(layout);
					break;
				default:
					this.Notify($"Object not implemented : {co.GetType().FullName}");
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
	}
}
