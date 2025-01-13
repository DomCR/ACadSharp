using ACadSharp.Entities;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfEntitiesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.EntitiesSection; } }

		public DxfEntitiesSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder objectHolder, DxfWriterConfiguration configuration)
			: base(writer, document, objectHolder, configuration) { }

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
