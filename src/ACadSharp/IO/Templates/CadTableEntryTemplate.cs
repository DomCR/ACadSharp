﻿using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntryTemplate<T> : CadTemplate<T>, ICadTableEntryTemplate
		where T : TableEntry
	{
		public string Type { get { return typeof(T).Name; } }

		public string Name { get { return this.CadObject.Name; } }

		public CadTableEntryTemplate(T entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}
}
