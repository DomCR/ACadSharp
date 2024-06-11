using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO
{
	internal abstract class CadDocumentBuilder
	{
		public event NotificationEventHandler OnNotification;

		public CadDocument DocumentToBuild { get; }

		public AppIdsTable AppIds { get; set; }

		public BlockRecordsTable BlockRecords { get; set; }

		public DimensionStylesTable DimensionStyles { get; set; }

		public LayersTable Layers { get; set; }

		public LineTypesTable LineTypesTable { get; set; }

		public TextStylesTable TextStyles { get; set; }

		public UCSTable UCSs { get; set; }

		public ViewsTable Views { get; set; }

		public VPortsTable VPorts { get; set; }

		public abstract bool KeepUnknownEntities { get; }

		public Dictionary<string, LineType> LineTypes { get; } = new Dictionary<string, LineType>(StringComparer.OrdinalIgnoreCase);

		protected Dictionary<ulong, CadTemplate> cadObjectsTemplates = new Dictionary<ulong, CadTemplate>();

		protected Dictionary<ulong, ICadObjectTemplate> templatesMap = new Dictionary<ulong, ICadObjectTemplate>();

		protected Dictionary<ulong, CadObject> cadObjects = new Dictionary<ulong, CadObject>();

		protected Dictionary<ulong, ICadTableTemplate> tableTemplates = new Dictionary<ulong, ICadTableTemplate>();

		protected Dictionary<ulong, ICadDictionaryTemplate> dictionaryTemplates = new();

		public CadDocumentBuilder(CadDocument document)
		{
			this.DocumentToBuild = document;
		}

		public virtual void BuildDocument()
		{
			foreach (CadTemplate template in this.cadObjectsTemplates.Values)
			{
				template.Build(this);
			}
		}

		public void AddTemplate(CadTemplate template)
		{
			this.cadObjectsTemplates[template.CadObject.Handle] = template;
			this.addToMap(template);
		}

		public void AddTableTemplate(ICadTableTemplate tableTemplate)
		{
			this.tableTemplates[tableTemplate.CadObject.Handle] = tableTemplate;
			this.addToMap(tableTemplate);
		}

		public void AddDictionaryTemplate(ICadDictionaryTemplate dictionaryTemplate)
		{
			this.dictionaryTemplates[dictionaryTemplate.CadObject.Handle] = dictionaryTemplate;
			this.addToMap(dictionaryTemplate);
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
			if (!handle.HasValue || handle == 0)
			{
				value = null;
				return false;
			}

			if (this.cadObjects.TryGetValue(handle.Value, out CadObject obj))
			{
				if (obj is UnknownEntity && !this.KeepUnknownEntities)
				{
					value = null;
					return false;
				}

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
			if (this.templatesMap.TryGetValue(handle, out ICadObjectTemplate template))
			{
				return (T)template;
			}

			return null;
		}

		public bool TryGetTableEntry<T>(string name, out T entry)
			where T : TableEntry
		{
			if (string.IsNullOrEmpty(name))
			{
				entry = null;
				return false;
			}

			entry = this.cadObjects.Values.OfType<T>().FirstOrDefault(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
			return entry != null;
		}

		public bool TryGetObjectTemplate<T>(ulong? handle, out T value) where T : CadTemplate
		{
			if (!handle.HasValue || handle == 0)
			{
				value = null;
				return false;
			}

			if (this.templatesMap.TryGetValue(handle.Value, out ICadObjectTemplate template))
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

		public void RegisterTables()
		{
			this.DocumentToBuild.RegisterCollection(this.AppIds);
			this.DocumentToBuild.RegisterCollection(this.LineTypesTable);
			this.DocumentToBuild.RegisterCollection(this.Layers);
			this.DocumentToBuild.RegisterCollection(this.TextStyles);
			this.DocumentToBuild.RegisterCollection(this.UCSs);
			this.DocumentToBuild.RegisterCollection(this.Views);
			this.DocumentToBuild.RegisterCollection(this.DimensionStyles);
			this.DocumentToBuild.RegisterCollection(this.VPorts);
			this.DocumentToBuild.RegisterCollection(this.BlockRecords);
		}

		public void BuildTables()
		{
			this.BuildTable(this.AppIds);
			this.BuildTable(this.LineTypesTable);
			this.BuildTable(this.Layers);
			this.BuildTable(this.TextStyles);
			this.BuildTable(this.UCSs);
			this.BuildTable(this.Views);
			this.BuildTable(this.DimensionStyles);
			this.BuildTable(this.VPorts);
			this.BuildTable(this.BlockRecords);
		}

		public void Notify(string message, NotificationType notificationType = NotificationType.None, Exception exception = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, notificationType, exception));
		}

		public void BuildTable<T>(Table<T> table)
			where T : TableEntry
		{
			if (this.tableTemplates.TryGetValue(table.Handle, out ICadTableTemplate template))
			{
				template.Build(this);
			}
			else
			{
				this.Notify($"Table {table.ObjectName} not found in the document", NotificationType.Warning);
			}
		}

		protected void buildDictionaries()
		{
			foreach (ICadDictionaryTemplate dictionaryTemplate in dictionaryTemplates.Values)
			{
				dictionaryTemplate.Build(this);
			}

			this.DocumentToBuild.UpdateCollections(true);
		}

		private void addToMap(ICadObjectTemplate template)
		{
			this.templatesMap.Add(template.CadObject.Handle, template);
			this.cadObjects.Add(template.CadObject.Handle, template.CadObject);
		}
	}
}
