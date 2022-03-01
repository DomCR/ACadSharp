using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

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

	internal class CadUcsTemplate : DwgTableEntryTemplate<UCS>
	{
		public CadUcsTemplate(UCS entry) : base(entry) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return true;

			switch (dxfcode)
			{
				case 79:
					found = true;
					break;
				default:
					break;
			}

			return found;
		}
	}
}
