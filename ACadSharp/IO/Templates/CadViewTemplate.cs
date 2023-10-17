using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadViewTemplate : CadTableEntryTemplate<View>
	{
		public ulong? VisualStyleHandle { get; set; }

		public ulong? NamedUcsHandle { get; set; }

		public ulong? UcsHandle { get; set; }

		public CadViewTemplate() : base(new View()) { }

		public CadViewTemplate(View entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			//TODO: assing ucs for view
		}
	}
}
