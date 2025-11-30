using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.XData;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal abstract class CadTemplate : ICadObjectTemplate
	{
		public bool HasBeenBuilt { get; private set; } = false;

		public CadObject CadObject { get; set; }

		public ulong? OwnerHandle { get; set; }

		public ulong? XDictHandle { get; set; }

		public HashSet<ulong> ReactorsHandles { get; set; } = new();

		public Dictionary<ulong, List<ExtendedDataRecord>> EDataTemplate { get; set; } = new();

		public Dictionary<string, List<ExtendedDataRecord>> EDataTemplateByAppName { get; set; } = new();

		public CadTemplate(CadObject cadObject)
		{
			this.CadObject = cadObject;
		}

		public void Build(CadDocumentBuilder builder)
		{
			if (this.HasBeenBuilt)
			{
				return;
			}
			else
			{
				this.HasBeenBuilt = true;
			}

			this.build(builder);
		}

		protected virtual void build(CadDocumentBuilder builder)
		{
			if (builder.TryGetCadObject(this.XDictHandle, out CadDictionary cadDictionary))
			{
				this.CadObject.XDictionary = cadDictionary;
			}

			foreach (ulong handle in this.ReactorsHandles)
			{
				if (builder.TryGetCadObject(handle, out CadObject reactor))
				{
					this.CadObject.AddReactor(reactor);
				}
				else
				{
					builder.Notify($"Reactor with handle {handle} not found", NotificationType.Warning);
				}
			}

			foreach (var item in this.EDataTemplate)
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

			foreach (var item in this.EDataTemplateByAppName)
			{
				if (builder.TryGetTableEntry(item.Key, out AppId app))
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
			CadEntityTemplate template = builder.GetObjectTemplate<CadEntityTemplate>(firstHandle);

			if (template == null)
			{
				builder.Notify($"Leading entity with handle {firstHandle} not found.", NotificationType.Warning);
				template = builder.GetObjectTemplate<CadEntityTemplate>(endHandle);
			}

			while (template != null)
			{
				yield return (T)template.CadObject;

				if (template.CadObject.Handle == endHandle)
				{
					break;
				}

				if (template.NextEntity.HasValue)
				{
					template = builder.GetObjectTemplate<CadEntityTemplate>(template.NextEntity.Value);
				}
				else
				{
					template = builder.GetObjectTemplate<CadEntityTemplate>(template.CadObject.Handle + 1);
				}
			}
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
