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
	internal interface ICadObjectBuilder
	{
		bool ToBuild { get; }
		void Build(DwgModelBuilder builder);
	}

	internal class DwgTemplate<T> : DwgTemplate
		where T : CadObject
	{
		public new T CadObject { get { return (T)base.CadObject; } set { base.CadObject = value; } }
		public DwgTemplate(T cadObject) : base(cadObject) { }
	}

	internal abstract class DwgTemplate : ICadObjectBuilder
	{
		public bool ToBuild { get; private set; } = true;
		public CadObject CadObject { get; set; }

		/// <summary>
		/// XDictionary handle linked to this object.
		/// </summary>
		public ulong? XDictHandle { get; set; }
		public List<ulong> ReactorsHandles { get; } = new List<ulong>();

		public DwgTemplate() { }
		public DwgTemplate(CadObject cadObject)
		{
			CadObject = cadObject;
		}

		public virtual void Build(DwgModelBuilder builder)
		{
			ToBuild = false;

			if (CadObject.OwnerHandle != null)
			{
				//TODO: Set the owner of the object??
			}

			if (XDictHandle.HasValue)
			{
				//CadObject.XDict = builder.GetCadObject<XDictionary>(XDictHandle)
			}

			foreach (ulong handle in ReactorsHandles)
			{
				CadObject reactor = builder.GetCadObject(handle);
				if (reactor != null)
					CadObject.Reactors.Add(handle, reactor);
			}
		}
	}

	internal class DwgEntityTemplate : DwgTemplate<Entity>
	{
		public byte EntityMode { get; set; }
		public byte LtypeFlags { get; set; }
		public ulong LayerHandle { get; set; }
		public ulong? LineTypeHandle { get; set; }
		public ulong? PrevEntity { get; set; }
		public ulong? NextEntity { get; set; }
		public ulong? ColorHandle { get; set; }

		public DwgEntityTemplate(Entity entity) : base(entity) { }

		public override void Build(DwgModelBuilder builder)
		{
			base.Build(builder);

			CadObject.Layer = builder.GetCadObject<Layer>(LayerHandle);

			switch (LtypeFlags)
			{
				case 0:
					CadObject.LineType = null;    //Get the linetype by layer
					break;
				case 1:
					CadObject.LineType = null;//Get the linetype by block
					break;
				case 2:
					CadObject.LineType = null;//Get the linetype by continuous
					break;
				case 3:
					if (LineTypeHandle.HasValue)
					{
						CadObject.LineType = builder.GetCadObject<LineType>(LineTypeHandle.Value);
					}
					break;
			}

			if (ColorHandle.HasValue)
			{
				var dwgColor = builder.GetCadObject<DwgColorTemplate.DwgColor>(ColorHandle.Value);

				if (dwgColor != null)
					CadObject.Color = dwgColor.Color;
			}
			else
			{
				//TODO: Set color by name, only for dxf?
			}
		}
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

		public override void Build(DwgModelBuilder builder)
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
		public List<ulong> EntitiesHandles { get; set; } = new List<ulong>();

		public DwgGroupTemplate(Group group) : base(group) { }
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

		public override void Build(DwgModelBuilder builder)
		{
			base.Build(builder);

			Insert insert = CadObject as Insert;

			foreach (var item in OwnedHandles)
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
			(CadObject as Hatch).Paths.Add(template.Path);
			m_pathTempaltes.Add(template);
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

		public override void Build(DwgModelBuilder builder)
		{
			base.Build(builder);

			//if (HardOwnerHandle.HasValue)
			//	TypedObject.BlockBegin = builder.GetCadObject<BlockBegin>(this.HardOwnerHandle);

			if (LayoutHandle.HasValue && builder.TryGetCadObject<Layout>(LayoutHandle.Value, out Layout layout))
			{
				layout.AssociatedBlock = CadObject;
			}

			if (FirstEntityHandle.HasValue
				&& SecondEntityHandle.HasValue
				&& builder.TryGetObjectBuilder(FirstEntityHandle.Value, out DwgEntityTemplate template))
			{
				do
				{
					if (template.NextEntity == null)
						break;

					CadObject.Entities.Add(template.CadObject);
					template = builder.GetObjectBuilder<DwgEntityTemplate>(template.NextEntity.Value);
				} while (template != null);
			}

			foreach (ulong handle in OwnedObjectsHandlers)
			{
				if (builder.TryGetCadObject<Entity>(handle, out Entity child))
				{
					CadObject.Entities.Add(child);
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
		public List<ulong> Handles { get; set; } = new List<ulong>();
		public ulong ModelSpaceHandle { get; set; }
		public ulong PaperSpaceHandle { get; set; }
		public DwgBlockCtrlObjectTemplate() : base(new BlockRecordsTable()) { }

		public override void Build(DwgModelBuilder builder)
		{
			base.Build(builder);


		}

		private void addBlockToModel(DwgModelBuilder builder)
		{

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
	}

	internal class DwgTableTemplate<T> : DwgTemplate<Table<T>>
		where T : TableEntry
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public DwgTableTemplate(Table<T> tableControl) : base(tableControl) { }
	}
}
