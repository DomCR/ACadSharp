using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadTableContentTemplate : CadTemplate<TableContent>
	{
		public CadTableContentTemplate() : base(new TableContent()) { }

		public CadTableContentTemplate(TableContent cadObject) : base(cadObject) { }

		public ulong SytleHandle { get; internal set; }
	}
}
