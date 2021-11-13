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

		protected IEnumerable<T> getEntitiesCollection<T>(DwgDocumentBuilder builder, ulong firstHandle, ulong endHandle)
			where T : Entity
		{
			List<T> collection = new List<T>();

			DwgEntityTemplate template = builder.GetObjectTemplate<DwgEntityTemplate>(firstHandle);
			while (template != null)
			{
				collection.Add((T)template.CadObject);

				if (template.CadObject.Handle == endHandle)
					break;

				template = builder.GetObjectTemplate<DwgEntityTemplate>(template.NextEntity.Value);
			}

			return collection;
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

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var handle in this.Handles)
			{
				CadObject member = builder.GetCadObject(handle);
				if (member != null)
				{
					this.CadObject.Members.Add(handle, member);
				}
			}
		}
	}

	internal class DwgViewportTableTemplate : DwgTemplate<ViewPortsTable>
	{
		public List<ulong> Handles { get; set; } = new List<ulong>();

		public DwgViewportTableTemplate(ViewPortsTable table) : base(table) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			throw new NotImplementedException();
		}
	}

	[Obsolete]
	internal class DwgBlockBeginTemplate : DwgEntityTemplate
	{
		public DwgBlockBeginTemplate(Entity block) : base(block) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			throw new NotImplementedException();
		}
	}

	internal class DwgHatchTemplate : DwgEntityTemplate
	{
		public class DwgBoundaryPathTemplate
		{
			public HatchBoundaryPath Path { get; set; } = new HatchBoundaryPath();
			public List<ulong> Handles { get; set; } = new List<ulong>();
		}

		private List<DwgBoundaryPathTemplate> _pathTempaltes { get; set; } = new List<DwgBoundaryPathTemplate>();

		public DwgHatchTemplate(Hatch hatch) : base(hatch) { }

		/// <summary>
		/// Add the path to the hatch and the templates list.
		/// </summary>
		/// <param name="template"></param>
		public void AddPath(DwgBoundaryPathTemplate template)
		{
			(this.CadObject as Hatch).Paths.Add(template.Path);
			this._pathTempaltes.Add(template);
		}

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			throw new NotImplementedException();
		}
	}

	internal class DwgDictionaryTemplate : DwgTemplate
	{
		public Dictionary<ulong, string> HandleEntries { get; set; } = new Dictionary<ulong, string>();
		public DwgDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);


		}
	}
}
