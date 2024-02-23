using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal abstract class CadTemplate : ICadObjectTemplate
	{
		public CadObject CadObject { get; set; }

		public ulong? OwnerHandle { get; set; }

		public ulong? XDictHandle { get; set; }

		public List<ulong> ReactorsHandles { get; set; } = new List<ulong>();

		public Dictionary<ulong, ExtendedData> EDataTemplate { get; set; } = new Dictionary<ulong, ExtendedData>();

		public Dictionary<string, ExtendedData> EDataTemplateByAppName { get; set; } = new Dictionary<string, ExtendedData>();

		public CadTemplate(CadObject cadObject)
		{
			this.CadObject = cadObject;
		}

		[Obsolete]
		public virtual bool AddHandle(int dxfcode, ulong handle)
		{
			return false;
		}

		[Obsolete]
		public virtual bool AddName(int dxfcode, string name)
		{
			return false;
		}

		[Obsolete]
		public virtual bool CheckDxfCode(int dxfcode, object value)
		{
			//Will return true if the code is used by the template
			return false;
		}

		public virtual void Build(CadDocumentBuilder builder)
		{
			if (builder.TryGetCadObject(this.XDictHandle, out CadDictionary cadDictionary))
			{
				this.CadObject.XDictionary = cadDictionary;
			}

			foreach (ulong handle in this.ReactorsHandles)
			{
				if (builder.TryGetCadObject(handle, out CadObject reactor))
				{
					if (this.CadObject.Reactors.ContainsKey(handle))
					{
						builder.Notify($"Reactor with handle {handle} already exist in the object {this.CadObject.Handle}", NotificationType.Warning);
					}
					else
					{
						this.CadObject.Reactors.Add(handle, reactor);
					}
				}
				else
				{
					builder.Notify($"Reactor with handle {handle} not found", NotificationType.Warning);
				}
			}

			foreach (KeyValuePair<ulong, ExtendedData> item in this.EDataTemplate)
			{
				if (builder.TryGetCadObject(item.Key, out AppId app))
				{
					this.CadObject.ExtendedData.Add(app, item.Value);
				}
				else
				{
					builder.Notify($"AppId in extended data with handle {item.Key} not found", NotificationType.Warning);
				}
			}
		}

		protected IEnumerable<T> getEntitiesCollection<T>(CadDocumentBuilder builder, ulong firstHandle, ulong endHandle)
			where T : Entity
		{
			List<T> collection = new List<T>();

			CadEntityTemplate template = builder.GetObjectTemplate<CadEntityTemplate>(firstHandle);
			while (template != null)
			{
				collection.Add((T)template.CadObject);

				if (template.CadObject.Handle == endHandle)
					break;

				if (template.NextEntity.HasValue)
					template = builder.GetObjectTemplate<CadEntityTemplate>(template.NextEntity.Value);
				else
					template = builder.GetObjectTemplate<CadEntityTemplate>(template.CadObject.Handle + 1);

			}

			return collection;
		}

		protected bool getTableReference<T>(CadDocumentBuilder builder, ulong? handle, string name, out T reference)
			where T : TableEntry
		{
			if (builder.TryGetCadObject<T>(handle, out reference) || builder.TryGetTableEntry<T>(name, out reference))
			{
				return true;
			}
			else
			{
				if (!string.IsNullOrEmpty(name) || (handle.HasValue && handle.Value != 0))
				{
					builder.Notify($"{typeof(T).FullName} table reference with handle: {handle} | name: {name} not found for {this.CadObject.GetType().FullName} with handle {this.CadObject.Handle}", NotificationType.Warning);
				}
				return false;
			}
		}
	}
}
