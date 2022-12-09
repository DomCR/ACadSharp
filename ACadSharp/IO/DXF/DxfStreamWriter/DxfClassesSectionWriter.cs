namespace ACadSharp.IO.DXF
{
	internal class DxfClassesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.ClassesSection; } }

		public DxfClassesSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
		{
		}

		protected override void writeSection()
		{
			foreach (Classes.DxfClass c in this._document.Classes)
			{
				this._writer.Write(0, DxfFileToken.ClassEntry);
				this._writer.Write(1, c.DxfName);
				this._writer.Write(2, c.CppClassName);
				this._writer.Write(3, c.ApplicationName);
				this._writer.Write(90, (int)c.ProxyFlags);
				this._writer.Write(91, c.InstanceCount);
				this._writer.Write(280, c.WasZombie);
				this._writer.Write(281, c.IsAnEntity);
			}
		}
	}
}
