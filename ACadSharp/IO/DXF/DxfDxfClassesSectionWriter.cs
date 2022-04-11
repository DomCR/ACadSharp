namespace ACadSharp.IO.DXF
{
	internal class DxfDxfClassesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.ClassesSection; } }

		public DxfDxfClassesSectionWriter(IDxfStreamWriter writer, CadDocument document) : base(writer, document)
		{
		}

		protected override void writeSection()
		{

		}
	}
}
