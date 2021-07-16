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
	internal abstract class DwgTypeTemplate<T> : DwgTemplate
		where T : CadObject
	{
		public T TypedObject { get { return (T)CadObject; } }
		public DwgTypeTemplate(T cadObject) : base(cadObject) { }
	}

	internal abstract class DwgTemplate
	{
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

		public virtual void Build(Dictionary<ulong, CadObject> map)
		{

		}

		public virtual void Build(DwgModelBuilder builder)
		{
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
					this.CadObject.Reactors.Add(handle, reactor);
			}
		}
	}
	internal class DwgEntityTemplate : DwgTypeTemplate<Entity>
	{
		public byte EntityMode { get; set; }
		public byte LtypeFlags { get; set; }
		public ulong LayerHandle { get; set; }
		public ulong? LineTypeHandle { get; set; }
		public ulong PrevEntity { get; set; }
		public ulong NextEntity { get; set; }
		public ulong? ColorHandle { get; set; }

		public DwgEntityTemplate(Entity entity) : base(entity) { }

		public override void Build(Dictionary<ulong, CadObject> map)
		{
			base.Build(map);

			Entity entity = CadObject as Entity;

			//TODO: Find the owner
			switch (EntityMode)
			{
				//Entity has a direct owner
				case 0b00:
					break;
				//Entity is owned by the PSPACE
				case 0b01:
					break;
				//Entity is owned by the MSPACE
				case 0b10:
					break;
				//Not used
				case 0b11:
				default:
					break;
			}

			if (map.TryGetValue(LayerHandle, out CadObject layer))
			{
				entity.Layer = (Layer)layer;
			}

			if (map.TryGetValue(LineTypeHandle.Value, out CadObject ltype))
			{
				entity.LineType = (LineType)ltype;
			}
		}

		public override void Build(DwgModelBuilder builder)
		{
			base.Build(builder);

			TypedObject.Layer = builder.GetCadObject<Layer>(LayerHandle);

			switch (LtypeFlags)
			{
				case 0:
					TypedObject.LineType = null;    //Get the linetype by layer
					break;
				case 1:
					TypedObject.LineType = null;//Get the linetype by block
					break;
				case 2:
					TypedObject.LineType = null;//Get the linetype by continuous
					break;
				case 3:
					if (LineTypeHandle.HasValue)
					{
						TypedObject.LineType = builder.GetCadObject<LineType>(LineTypeHandle.Value);
					}
					break;
			}

			if (ColorHandle.HasValue)
			{
				var dwgColor = builder.GetCadObject<DwgColorTemplate.DwgColor>(ColorHandle.Value);

				if (dwgColor != null)
					TypedObject.Color = dwgColor.Color;
			}
			else
			{
				//TODO: Set color by name, only for dxf?
			}
		}
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

		public override void Build(Dictionary<ulong, CadObject> map)
		{
			base.Build(map);

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
		public Dictionary<ulong, string> HandleEntries { get; set; } = new Dictionary<ulong, string>();
		public DwgDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }
	}

	internal class DwgBlockCtrlObjectTemplate : DwgTemplate
	{
		public List<ulong> Handles { get; set; } = new List<ulong>();
		public ulong ModelSpaceHandle { get; set; }
		public ulong PaperSpaceHandle { get; set; }
		public DwgBlockCtrlObjectTemplate() : base(new BlockRecordsTable()) { }
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
}
