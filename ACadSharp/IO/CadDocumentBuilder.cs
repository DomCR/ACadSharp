using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO
{
	internal abstract class CadDocumentBuilder
	{
		// Stores all the templates to build the document, some of the elements can be null due a missing implementation
		public Dictionary<ulong, DwgTemplate> Templates { get; } = new Dictionary<ulong, DwgTemplate>();

		public CadDocument DocumentToBuild { get; }

		public NotificationEventHandler NotificationHandler { get; }

		public CadDocumentBuilder(CadDocument document, NotificationEventHandler notification = null)
		{
			this.DocumentToBuild = document;
			this.NotificationHandler = notification;
		}

		public virtual void BuildDocument()
		{
			foreach (DwgTemplate template in this.Templates.Values)
			{
				template?.Build(this);
			}
		}

		public CadObject GetCadObject(ulong handle)
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate template))
				return template?.CadObject;
			else
				return null;
		}

		public T GetCadObject<T>(ulong handle) where T : CadObject
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate template))
			{
				if (template?.CadObject is T)
					return (T)template?.CadObject;
			}

			return null;
		}

		public bool TryGetCadObject<T>(ulong? handle, out T value) where T : CadObject
		{
			if (!handle.HasValue)
			{
				value = null;
				return false;
			}

			if (this.Templates.TryGetValue(handle.Value, out DwgTemplate template))
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

		public bool TryGetCadObject<T>(ulong handle, out T value) where T : CadObject
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate template))
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

		public T GetObjectTemplate<T>(ulong handle) where T : DwgTemplate
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate builder))
			{
				return (T)builder;
			}

			return null;
		}

		public bool TryGetObjectTemplate<T>(ulong handle, out T value) where T : DwgTemplate
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate template))
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
