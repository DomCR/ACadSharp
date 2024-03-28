using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntityTemplate : CadInsertTemplate
	{
		public ulong? StyleHandle { get; set; }
		public ulong NullHandle { get; internal set; }

		public CadTableEntityTemplate(TableEntity table) : base(table) { }
	}
}
