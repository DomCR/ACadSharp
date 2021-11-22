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

		public ulong? OwnerHandle { get; set; }

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
			if (this.OwnerHandle.HasValue)
			{
				if (builder.TryGetCadObject(this.OwnerHandle.Value, out CadObject owner))
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

				if (template.NextEntity.HasValue)
					template = builder.GetObjectTemplate<DwgEntityTemplate>(template.NextEntity.Value);
				else
					template = builder.GetObjectTemplate<DwgEntityTemplate>(template.CadObject.Handle + 1);

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
		public string DIMBL_Name { get; internal set; }
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
		public DwgDimensionStyleTemplate(DimensionStyle dimStyle) : base(dimStyle) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			//throw new NotImplementedException();
		}
	}

	internal class DwgColorTemplate : DwgTemplate
	{
		public string Name { get; set; }
		public string BookName { get; set; }

		public DwgColorTemplate(DwgColor color) : base(color) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			throw new NotImplementedException();
		}

		public class DwgColor : CadObject
		{
			public override ObjectType ObjectType => ObjectType.INVALID;
			public Color Color { get; set; }
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

	internal class DwgDictionaryTemplate : DwgTemplate
	{
		public Dictionary<ulong, string> HandleEntries { get; set; } = new Dictionary<ulong, string>();
		public DwgDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.OwnerHandle.HasValue && this.OwnerHandle == 0)
			{

			}

			//throw new NotImplementedException();
		}
	}
}
