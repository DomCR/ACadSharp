using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Objects;
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

		/// <summary>
		/// XDictionary handle linked to this object.
		/// </summary>
		public ulong XDictHandle { get; set; }

		public DwgTemplate() { }
		public DwgTemplate(CadObject cadObject)
		{
			CadObject = cadObject;
		}
	}

	internal class DwgEntityTemplate : DwgTemplate
	{
		public byte EntityMode { get; set; }
		public ulong LayerHandle { get; set; }
		public ulong LineTypeHandle { get; set; }
		public ulong PrevEntity { get; set; }
		public ulong NextEntity { get; set; }
		public ulong ColorHandle { get; set; }

		public DwgEntityTemplate(Entity entity) : base(entity) { }
	}

	internal class DwgPolyLineTemplate : DwgEntityTemplate
	{
		public ulong FirstVertexHandle { get; internal set; }
		public ulong LastVertexHandle { get; internal set; }
		public ulong SeqendHandle { get; internal set; }
		public List<ulong> VertexHandles { get; set; } = new List<ulong>();

		public DwgPolyLineTemplate(PolyLine entity) : base(entity) { }
	}

	internal class DwgTextEntityTemplate : DwgEntityTemplate
	{
		public ulong StyleHandle { get; set; }

		public DwgTextEntityTemplate(Entity entity) : base(entity) { }
	}

	internal class DwgTableEntryTemplate : DwgTemplate
	{
		public ulong LtypeControlHandle { get; set; }
		public DwgTableEntryTemplate(TableEntry entry) : base(entry) { }
	}

	internal class DwgLayerTemplate : DwgTableEntryTemplate
	{
		public ulong LayerControlHandle { get; internal set; }
		public object PlotStyleHandle { get; internal set; }
		public ulong MaterialHandle { get; internal set; }
		public ulong LineTypeHandle { get; internal set; }

		public DwgLayerTemplate(TableEntry entry) : base(entry) { }
	}

	internal class DwgObjectTemplate : DwgTemplate
	{
		public DwgObjectTemplate(CadObject cadObject) : base(cadObject) { }
	}

	internal class DwgBlockTemplate : DwgTableEntryTemplate
	{
		public ulong FirstEntityHandle { get; internal set; }
		public ulong SecondEntityHandle { get; internal set; }
		public ulong EndBlockHandle { get; internal set; }
		public ulong LayoutHandle { get; internal set; }
		public List<ulong> OwnedObjectsHandlers { get; set; } = new List<ulong>();
		public List<ulong> Entries { get; set; } = new List<ulong>();

		public DwgBlockTemplate(Block block) : base(block) { }
	}

	internal class DwgDictionaryTemplate : DwgTemplate
	{
		public Dictionary<string, ulong> HandleEntries { get; set; } = new Dictionary<string, ulong>();
		public DwgDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }
	}
}
