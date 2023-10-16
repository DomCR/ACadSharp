using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntryTemplate<T> : CadTemplate<T>
		where T : TableEntry
	{
		public CadTableEntryTemplate(T entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}
}
