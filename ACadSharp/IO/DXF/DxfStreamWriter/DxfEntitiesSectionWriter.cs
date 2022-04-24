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
			foreach (Entities.Entity item in this.Holder.Entities)
			{
				this.writeEntity(item);
			}
		}
	}
}
