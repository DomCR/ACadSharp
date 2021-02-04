using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class AppId : TableEntry
	{
		public AppId(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "App id must have a name.");
		}

		internal AppId(DxfEntryTemplate template) : base(template)
		{
			if (string.IsNullOrEmpty(template.TableName))
				throw new ArgumentNullException(nameof(template.TableName), "App id must have a name.");
		}
	}
}
