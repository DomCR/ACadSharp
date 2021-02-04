using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class Style : TableEntry
	{
		public Style(string name) : base(name) { }

		internal Style(DxfEntryTemplate template) : base(template) { }
	}
}
