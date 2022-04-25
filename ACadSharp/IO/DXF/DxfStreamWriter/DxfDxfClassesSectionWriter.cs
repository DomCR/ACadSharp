namespace ACadSharp.IO.DXF
{
	internal class DxfDxfClassesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.ClassesSection; } }

		public DxfDxfClassesSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
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
				this._writer.Write(280, c.WasAProxy);
				this._writer.Write(281, c.IsAnEntity);
			}
		}
	}
}
