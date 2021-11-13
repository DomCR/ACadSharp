using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System;

namespace ACadSharp.IO.Templates
{
	internal class DwgTableEntryTemplate<T> : DwgTemplate<T>
		where T : TableEntry
	{
		public ulong? LtypeControlHandle { get; set; }

		public DwgTableEntryTemplate(T entry) : base(entry) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.LtypeControlHandle.HasValue && this.LtypeControlHandle.Value > 0)
			{
				throw new NotImplementedException();
			}
		}
	}
}
