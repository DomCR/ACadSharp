using ACadSharp.Entities;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfEntitiesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.EntitiesSection; } }

		public DxfEntitiesSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
		{
		}

		protected override void writeSection()
		{
			while (this.Holder.Entities.Any())
			{
				Entity item = this.Holder.Entities.Dequeue();

				this.writeEntity(item);
			}
		}
	}
}
