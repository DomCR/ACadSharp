using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Templates
{
	internal abstract class DwgTemplate
	{
		public CadObject CadObject { get; set; }

		public DwgTemplate() { }
		public DwgTemplate(CadObject cadObject)
		{
			CadObject = cadObject;
		}
	}

	internal class DwgEntityTemplate : DwgTemplate
	{
		/// <summary>
		/// XDictionary handle linked to this object.
		/// </summary>
		public ulong XDictHandle { get; set; }
		public byte EntityMode { get; set; }
		public ulong LayerHandle { get; set; }
		public ulong LineTypeHandle { get; set; }
		public ulong PrevEntity { get; set; }
		public ulong NextEntity { get; set; }
		public ulong ColorHandle { get; set; }

		public DwgEntityTemplate(Entity entity) : base(entity) { }
	}

	internal class DwgTextEntityTemplate : DwgEntityTemplate
	{
		public ulong StyleHandle { get; set; } 

		public DwgTextEntityTemplate(TextEntity entity) : base(entity) { }
	}

	internal class DwgTableEntryTemplate : DwgTemplate
	{
		public DwgTableEntryTemplate(TableEntry entry) : base(entry) { }
	}
}
