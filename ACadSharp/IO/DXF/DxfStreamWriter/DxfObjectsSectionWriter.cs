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
	}
}
