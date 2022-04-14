using ACadSharp.Blocks;
using ACadSharp.IO.DWG;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO
{
	internal abstract class CadDocumentBuilder
	{
		// Stores all the templates to build the document, some of the elements can be null due a missing implementation
		public Dictionary<ulong, CadTemplate> Templates { get; } = new Dictionary<ulong, CadTemplate>();

		public Dictionary<ulong, BlockRecord> BlockRecords { get; } = new Dictionary<ulong, BlockRecord>();

		public Dictionary<string, LineType> LineTypes { get; set; } = new Dictionary<string, LineType>(StringComparer.OrdinalIgnoreCase);

		public CadDocument DocumentToBuild { get; }

		public NotificationEventHandler NotificationHandler { get; }

		public CadDocumentBuilder(CadDocument document, NotificationEventHandler notification = null)
		{
			this.DocumentToBuild = document;
			this.NotificationHandler = notification;
		}

		public virtual void BuildDocument()
		{
			foreach (CadTemplate template in this.Templates.Values)
			{
				template?.Build(this);
			}
		}

		public CadObject GetCadObject(ulong handle)
		{
			if (this.Templates.TryGetValue(handle, out CadTemplate template))
				return template?.CadObject;
			else
				return null;
		}

		public T GetCadObject<T>(ulong handle) where T : CadObject
		{
			if (this.Templates.TryGetValue(handle, out CadTemplate template))
			{
				if (template?.CadObject is T)
					return (T)template?.CadObject;
			}

			return null;
		}

		public T GetCadObject<T>(ulong? handle) where T : CadObject
		{
			if (!handle.HasValue)
			{
				return null;
			}

			return GetCadObject<T>(handle.Value);
		}

		public bool TryGetCadObject<T>(ulong? handle, out T value) where T : CadObject
		{
			if (!handle.HasValue)
			{
				value = null;
				return false;
			}

			return this.TryGetCadObject(handle.Value, out value);
		}

		public bool TryGetCadObject<T>(ulong handle, out T value) where T : CadObject
		{
			if (this.Templates.TryGetValue(handle, out CadTemplate template))
			{
				if (template?.CadObject is T)
				{
					value = (T)template.CadObject;
					return true;
				}
			}

			value = null;
			return false;
		}

		public T GetObjectTemplate<T>(ulong handle) where T : CadTemplate
		{
			if (this.Templates.TryGetValue(handle, out CadTemplate builder))
			{
				return (T)builder;
			}

			return null;
		}

		public bool TryGetObjectTemplate<T>(ulong handle, out T value) where T : CadTemplate
		{
			if (this.Templates.TryGetValue(handle, out CadTemplate template))
			{
				if (template is T)
				{
					value = (T)template;
					return true;
				}
			}

			value = null;
			return false;
		}
	}
}
