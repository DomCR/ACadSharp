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
	internal abstract class CadTemplate : ICadObjectTemplate
	{
		public CadObject CadObject { get; set; }

		public ulong? OwnerHandle { get; set; }

		public ulong? XDictHandle { get; set; }

		public List<ulong> ReactorsHandles { get; } = new List<ulong>();

		public Dictionary<ulong, ExtendedData> EDataTemplate { get; } = new Dictionary<ulong, ExtendedData>();

		public CadTemplate(CadObject cadObject)
		{
			this.CadObject = cadObject;
		}

		public virtual bool AddHandle(int dxfcode, ulong handle)
		{
			return false;
		}

		public virtual bool AddName(int dxfcode, string name)
		{
			return false;
		}

		public virtual bool CheckDxfCode(int dxfcode, object value)
		{
			//Will return true if the code is used by the template
			return false;
		}

		public virtual void Build(CadDocumentBuilder builder)
		{
			if (false)
				if (this.OwnerHandle.HasValue)
				{
					if (builder.TryGetCadObject(this.OwnerHandle.Value, out CadObject owner))
						this.CadObject.Owner = owner;
				}

			if (builder.TryGetCadObject(this.XDictHandle, out CadDictionary cadDictionary))
			{
				this.CadObject.XDictionary = cadDictionary;
			}
			else if (this.XDictHandle.HasValue && this.XDictHandle.Value != 0)
			{
				builder.NotificationHandler?.Invoke(this.CadObject, new NotificationEventArgs($"Dictionary couldn't be found, handle : {this.XDictHandle}"));
			}

			foreach (ulong handle in this.ReactorsHandles)
			{
				if (builder.TryGetCadObject(handle, out CadObject reactor))
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
	}
}
