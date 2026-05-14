using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadViewportEntityHeaderTemplate : CadTableEntryTemplate<ViewportEntityHeader>
	{
		public ulong? BlockHandle { get; set; }

		public CadViewportEntityHeaderTemplate(ViewportEntityHeader entry) : base(entry)
		{
		}
	}
}