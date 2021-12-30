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

		public virtual void Build(CadDocumentBuilder builder)
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

		protected IEnumerable<T> getEntitiesCollection<T>(CadDocumentBuilder builder, ulong firstHandle, ulong endHandle)
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
}
