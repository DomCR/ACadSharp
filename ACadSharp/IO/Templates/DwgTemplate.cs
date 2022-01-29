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

		public ulong? XDictHandle { get; set; }

		public List<ulong> ReactorsHandles { get; } = new List<ulong>();

		public Dictionary<ulong, ExtendedData> EDataTemplate { get; } = new Dictionary<ulong, ExtendedData>();

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

			if (builder.TryGetCadObject(this.XDictHandle, out CadDictionary cadDictionary))
			{
				this.CadObject.Dictionary = cadDictionary;
			}
			else if (this.XDictHandle.HasValue && this.XDictHandle.Value != 0)
			{
				builder.NotificationHandler?.Invoke(this.CadObject, new NotificationEventArgs($"Dictionary couldn't be found, handle : {this.XDictHandle}"));
			}

			foreach (ulong handle in this.ReactorsHandles)
			{
				CadObject reactor = builder.GetCadObject(handle);
				if (reactor != null)
					this.CadObject.Reactors.Add(handle, reactor);
			}

			foreach (KeyValuePair<ulong, ExtendedData> item in this.EDataTemplate)
			{
				if (builder.TryGetCadObject(item.Key, out AppId app))
				{

				}
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
