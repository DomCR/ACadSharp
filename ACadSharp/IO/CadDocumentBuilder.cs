using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO
{
	internal abstract class CadDocumentBuilder
	{
		public event NotificationEventHandler OnNotification;

		public CadDocument DocumentToBuild { get; }

		public Dictionary<string, LineType> LineTypes { get; } = new Dictionary<string, LineType>(StringComparer.OrdinalIgnoreCase);

		//public CadReaderConfiguration Configuration { get; }

		// Stores all the templates to build the document, some of the elements can be null due a missing implementation
		protected Dictionary<ulong, CadTemplate> templates = new Dictionary<ulong, CadTemplate>();

		protected Dictionary<ulong, CadObject> cadObjects = new Dictionary<ulong, CadObject>();

		protected Dictionary<ulong, ICadTableTemplate> tableTemplates = new Dictionary<ulong, ICadTableTemplate>();

		public CadDocumentBuilder(CadDocument document)
		{
			this.DocumentToBuild = document;
		}

		public virtual void BuildDocument()
		{
			foreach (CadTemplate template in this.templates.Values)
			{
				template.Build(this);
			}
		}

		public void AddTableTemplate(ICadTableTemplate tableTemplate)
		{
			this.tableTemplates[tableTemplate.CadObject.Handle] = tableTemplate;
			this.cadObjects[tableTemplate.CadObject.Handle] = tableTemplate.CadObject;
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

			if (this.cadObjects.TryGetValue(handle.Value, out CadObject obj))
			{
				if (obj is T)
				{
					value = (T)obj;
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

		[Obsolete]
		public void Notify(NotificationEventArgs e)
		{
			this.OnNotification?.Invoke(this, e);
		}

		public void Notify(string message, NotificationType notificationType = NotificationType.None, Exception exception = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, notificationType, exception));
		}
	}
}
