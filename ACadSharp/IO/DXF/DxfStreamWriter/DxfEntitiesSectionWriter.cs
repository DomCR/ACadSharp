namespace ACadSharp.IO.DXF
{
	internal class DxfEntitiesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.EntitiesSection; } }

		public DxfEntitiesSectionWriter(IDxfStreamWriter writer, CadDocument document) : base(writer, document)
		{
		}

		protected override void writeSection()
		{

		}
	}
}
