using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class View : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.VIEW;

		public View() : base() { }

		public View(string name) : base(name) { }
	}
}
