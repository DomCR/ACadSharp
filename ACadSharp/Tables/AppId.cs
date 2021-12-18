using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class AppId : TableEntry
	{
		/// <summary>
		/// Default application registry name.
		/// </summary>
		public const string DefaultName = "ACAD";

		public override ObjectType ObjectType => ObjectType.APPID;

		public AppId() : this(DefaultName) { }

		public AppId(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "App id must have a name.");
		}
	}
}
