using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Templates
{
	internal abstract class DwgTemplate : ICadObjectBuilder
	{
		public CadObject CadObject { get; set; }

		/// <summary>
		/// XDictionary handle linked to this object.
		/// </summary>
		public ulong? XDictHandle { get; set; }
		public List<ulong> ReactorsHandles { get; } = new List<ulong>();

		public DwgTemplate(CadObject cadObject)
		{
			this.CadObject = cadObject;
		}

		public virtual void Build(DwgDocumentBuilder builder)
		{
			if (this.CadObject.OwnerHandle.HasValue)
			{
				CadObject owner = builder.GetCadObject(this.CadObject.OwnerHandle.Value);
				this.CadObject.Owner = owner;
			}

			if (this.XDictHandle.HasValue)
			{
				//CadObject.XDict = builder.GetCadObject<XDictionary>(XDictHandle)
			}

			foreach (ulong handle in this.ReactorsHandles)
			{
				CadObject reactor = builder.GetCadObject(handle);
				if (reactor != null)
					this.CadObject.Reactors.Add(handle, reactor);
			}
		}
	}

	internal class DwgTemplate<T> : DwgTemplate
		where T : CadObject
	{
		public new T CadObject { get { return (T)base.CadObject; } set { base.CadObject = value; } }
		public DwgTemplate(T cadObject) : base(cadObject) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}

	internal class DwgDimensionStyleTemplate : DwgTemplate<DimensionStyle>
	{
		public DwgDimensionStyleTemplate(DimensionStyle dimStyle) : base(dimStyle) { }

		public string DIMBL_KName { get; internal set; }
		public string DIMBLK1_Name { get; internal set; }
		public string DIMBLK2_Name { get; internal set; }
		public ulong DIMTXSTY { get; internal set; }
		public ulong DIMLDRBLK { get; internal set; }
		public ulong DIMBLK { get; internal set; }
		public ulong DIMBLK1 { get; internal set; }
		public ulong DIMBLK2 { get; internal set; }
		public ulong Dimltype { get; internal set; }
		public ulong Dimltex1 { get; internal set; }
		public ulong Dimltex2 { get; internal set; }
	}

	internal class DwgViewportTemplate : DwgEntityTemplate
	{
		public ulong? ViewportHeaderHandle { get; set; }
		public ulong? BoundaryHandle { get; set; }
		public ulong? NamedUcsHandle { get; set; }
		public ulong? BaseUcsHandle { get; set; }
		public List<ulong> FrozenLayerHandles { get; set; } = new List<ulong>();
		public DwgViewportTemplate(Viewport entity) : base(entity) { }
	}

	internal class DwgColorTemplate : DwgTemplate
	{
		public string Name { get; set; }
		public string BookName { get; set; }

		public DwgColorTemplate(DwgColor color) : base(color) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			return;
		}

		public class DwgColor : CadObject
		{
			public override ObjectType ObjectType => ObjectType.INVALID;
			public Color Color { get; set; }
		}
	}

	internal class DwgGroupTemplate : DwgTemplate<Group>
	{
		public List<ulong> Handles { get; set; } = new List<ulong>();

		public DwgGroupTemplate(Group group) : base(group) { }
	}

	internal class DwgViewportTableTemplate : DwgTemplate<ViewPortsTable>
	{
		public List<ulong> Handles { get; set; } = new List<ulong>();

		public DwgViewportTableTemplate(ViewPortsTable table) : base(table) { }
	}

	internal class DwgInsertTemplate : DwgEntityTemplate
	{
		public bool HasAtts { get; internal set; }
		public int OwnedObjectsCount { get; internal set; }
		public ulong BlockHeaderHandle { get; internal set; }
		public ulong FirstAttributeHandle { get; internal set; }
		public ulong EndAttributeHandle { get; internal set; }
		public ulong SeqendHandle { get; internal set; }
		public List<ulong> OwnedHandles { get; set; } = new List<ulong>();

		public DwgInsertTemplate(Insert insert) : base(insert) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			Insert insert = this.CadObject as Insert;

			foreach (var item in this.OwnedHandles)
			{

			}
		}
	}

	internal class DwgBlockBeginTemplate : DwgEntityTemplate
	{
		public DwgBlockBeginTemplate(Entity block) : base(block) { }
	}

	internal class DwgHatchTemplate : DwgEntityTemplate
	{
		public class DwgBoundaryPathTemplate
		{
			public HatchBoundaryPath Path { get; set; } = new HatchBoundaryPath();
			public List<ulong> Handles { get; set; } = new List<ulong>();
		}

		private List<DwgBoundaryPathTemplate> m_pathTempaltes { get; set; } = new List<DwgBoundaryPathTemplate>();
		public DwgHatchTemplate(Hatch hatch) : base(hatch) { }
		/// <summary>
		/// Add the path to the hatch and the templates list.
		/// </summary>
		/// <param name="template"></param>
		public void AddPath(DwgBoundaryPathTemplate template)
		{
			(this.CadObject as Hatch).Paths.Add(template.Path);
			this.m_pathTempaltes.Add(template);
		}
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

	internal class DwgTableEntryTemplate<T> : DwgTemplate<T>
		where T : TableEntry
	{
		public ulong LtypeControlHandle { get; set; }

		public DwgTableEntryTemplate(T entry) : base(entry) { }
	}

	internal class DwgLayerTemplate : DwgTableEntryTemplate<Layer>
	{
		public ulong LayerControlHandle { get; internal set; }
		public object PlotStyleHandle { get; internal set; }
		public ulong MaterialHandle { get; internal set; }
		public ulong LineTypeHandle { get; internal set; }

		public DwgLayerTemplate(Layer entry) : base(entry) { }
	}

	internal class DwgObjectTemplate : DwgTemplate
	{
		public DwgObjectTemplate(CadObject cadObject) : base(cadObject) { }
	}

	internal class DwgBlockTemplate : DwgTableEntryTemplate<Block>
	{
		public ulong? FirstEntityHandle { get; set; }
		public ulong? SecondEntityHandle { get; set; }
		public ulong EndBlockHandle { get; set; }
		public ulong? LayoutHandle { get; set; }
		public List<ulong> OwnedObjectsHandlers { get; set; } = new List<ulong>();
		public List<ulong> Entries { get; set; } = new List<ulong>();
		public ulong? HardOwnerHandle { get; set; }

		public DwgBlockTemplate(Block block) : base(block) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			//if (HardOwnerHandle.HasValue)
			//	TypedObject.BlockBegin = builder.GetCadObject<BlockBegin>(this.HardOwnerHandle);

			if (this.LayoutHandle.HasValue && builder.TryGetCadObject<Layout>(this.LayoutHandle.Value, out Layout layout))
			{
				layout.AssociatedBlock = this.CadObject;
			}

			if (this.FirstEntityHandle.HasValue
				&& this.SecondEntityHandle.HasValue
				&& builder.TryGetObjectBuilder(this.FirstEntityHandle.Value, out DwgEntityTemplate template))
			{
				do
				{
					if (template.NextEntity == null)
						break;

					this.CadObject.Entities.Add(template.CadObject);
					template = builder.GetObjectBuilder<DwgEntityTemplate>(template.NextEntity.Value);
				} while (template != null);
			}

			foreach (ulong handle in this.OwnedObjectsHandlers)
			{
				if (builder.TryGetCadObject<Entity>(handle, out Entity child))
				{
					this.CadObject.Entities.Add(child);
				}
			}

			//TODO: Process EndBlockHandle ?? 
		}
	}

	internal class DwgDictionaryTemplate : DwgTemplate
	{
		public Dictionary<ulong, string> HandleEntries { get; set; } = new Dictionary<ulong, string>();
		public DwgDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }
	}

	internal class DwgBlockCtrlObjectTemplate : DwgTemplate
	{
		public ulong ModelSpaceHandle { get; set; }
		public ulong PaperSpaceHandle { get; set; }
		public List<ulong> Handles { get; set; } = new List<ulong>();
		public DwgBlockCtrlObjectTemplate() : base(new BlockRecordsTable()) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			addBlockToModel(builder, ModelSpaceHandle);
			addBlockToModel(builder, PaperSpaceHandle);

			foreach (ulong handle in this.Handles)
				this.addBlockToModel(builder, handle);
		}

		private void addBlockToModel(DwgDocumentBuilder builder, ulong handle)
		{
			if (builder.TryGetCadObject<Block>(handle, out Block block))
				builder.DocumentToBuild.AddBlock(block, true);
		}
	}

	internal class DwgLayoutTemplate : DwgTemplate
	{
		public ulong PaperSpaceBlockHandle { get; set; }
		public ulong ActiveViewportHandle { get; set; }
		public ulong BaseUcsHandle { get; set; }
		public ulong NamesUcsHandle { get; set; }
		public List<ulong> ViewportHandles { get; set; } = new List<ulong>();
		public DwgLayoutTemplate(Layout layout) : base(layout) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			throw new NotImplementedException();
		}
	}

	internal class DwgTableTemplate<T> : DwgTemplate<Table<T>>
		where T : TableEntry
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public DwgTableTemplate(Table<T> tableControl) : base(tableControl) { }
	}
}
