namespace ACadSharp.IO.DXF
{
	internal class DxfEntitiesSectionReader : DxfSectionReaderBase
	{
		public DxfEntitiesSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder, NotificationEventHandler notification = null) 
			: base(reader, builder, notification)
		{
		}

		public override void Read()
		{

		}
	}
}
