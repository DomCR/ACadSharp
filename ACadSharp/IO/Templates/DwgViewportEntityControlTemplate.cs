using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgViewportEntityControlTemplate : CadTemplate
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public DwgViewportEntityControlTemplate() : base(null) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

		}
	}
}
