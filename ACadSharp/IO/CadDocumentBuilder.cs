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
		public event NotificationEventHandler OnNotificationHandler;

		public CadDocument DocumentToBuild { get; }

		public Dictionary<string, LineType> LineTypes { get; } = new Dictionary<string, LineType>(StringComparer.OrdinalIgnoreCase);

		// Stores all the templates to build the document, some of the elements can be null due a missing implementation
		protected Dictionary<ulong, CadTemplate> templates = new Dictionary<ulong, CadTemplate>();

		protected Dictionary<ulong, CadObject> cadObjects = new Dictionary<ulong, CadObject>();

		public CadDocumentBuilder(CadDocument document, NotificationEventHandler notification = null)
		{
			this.DocumentToBuild = document;
			this.OnNotificationHandler = notification;
		}

		public virtual void BuildDocument()
		{
			foreach (CadTemplate template in this.templates.Values)
			{
				template.Build(this);
			}
		}

		public void AddTemplate(CadTemplate template)
		{
			this.templates[template.CadObject.Handle] = template;
			this.cadObjects[template.CadObject.Handle] = template.CadObject;
		}

		public CadObject GetCadObject(ulong handle)
		{
			if (this.templates.TryGetValue(handle, out CadTemplate template))
				return template?.CadObject;
			else
				return null;
		}

		public T GetCadObject<T>(ulong? handle) where T : CadObject
		{
			if (!handle.HasValue)
			{
				return null;
			}

			if (this.cadObjects.TryGetValue(handle.Value, out CadObject co))
			{
				if (co is T)
					return (T)co;
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

			if (this.templates.TryGetValue(handle.Value, out CadTemplate template))
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
			if (this.templates.TryGetValue(handle, out CadTemplate builder))
			{
				return (T)builder;
			}

			return null;
		}

		public bool TryGetObjectTemplate<T>(ulong handle, out T value) where T : CadTemplate
		{
			if (this.templates.TryGetValue(handle, out CadTemplate template))
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

		public void Notify(NotificationEventArgs e)
		{
			this.OnNotificationHandler?.Invoke(this, e);
		}
	}
}
