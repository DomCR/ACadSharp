using ACadSharp.IO.Templates;

namespace ACadSharp.Tables
{
	public class LineType : TableEntry
	{
		public LineType(string name) : base(name) { }

		internal LineType(DxfEntryTemplate template) : base(template) { }
	}
}