using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System;

namespace ACadSharp.IO.Templates
{
	internal class DwgTableEntryTemplate<T> : DwgTemplate<T>
		where T : TableEntry
	{
		public DwgTableEntryTemplate(T entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}
}
