using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO
{
	internal abstract class CadDocumentBuilder
	{
		public event NotificationEventHandler OnNotification;

		public AppIdsTable AppIds { get; set; } = new AppIdsTable();

		public BlockRecordsTable BlockRecords { get; set; } = new BlockRecordsTable();

		public DimensionStylesTable DimensionStyles { get; set; } = new DimensionStylesTable();

		public CadDocument DocumentToBuild { get; }

		public ulong InitialHandSeed { get; set; } = 0;

		public abstract bool KeepUnknownEntities { get; }

		public abstract bool KeepUnknownNonGraphicalObjects { get; }

		public LayersTable Layers { get; set; } = new LayersTable();

		public LineTypesTable LineTypesTable { get; set; } = new LineTypesTable();

		public TextStylesTable TextStyles { get; set; } = new TextStylesTable();

		public UCSTable UCSs { get; set; } = new UCSTable();

		public ACadVersion Version { get; }

		public ViewsTable Views { get; set; } = new ViewsTable();

		public VPortsTable VPorts { get; set; } = new VPortsTable();

		protected Dictionary<ulong, CadObject> cadObjects = new();

		protected Dictionary<ulong, ICadObjectTemplate> cadObjectsTemplates = new();

		protected Dictionary<ulong, ICadDictionaryTemplate> dictionaryTemplates = new();

		protected Dictionary<ulong, ICadTableEntryTemplate> tableEntryTemplates = new();

		protected Dictionary<ulong, ICadTableTemplate> tableTemplates = new();

		protected Dictionary<ulong, ICadObjectTemplate> templatesMap = new();

		protected List<ICadObjectTemplate> unassignedObjects = new();

		public CadDocumentBuilder(ACadVersion version, CadDocument document)
		{
			this.Version = version;
			this.DocumentToBuild = document;
		}

		public void AddTemplate(ICadObjectTemplate template)
		{
			if (!this.addToMap(template))
			{
				return;
			}

			switch (template)
			{
				case ICadDictionaryTemplate dictionaryTemplate:
					this.dictionaryTemplates.Add(dictionaryTemplate.CadObject.Handle, dictionaryTemplate);
					break;
				case ICadTableTemplate tableTemplate:
					this.tableTemplates.Add(tableTemplate.CadObject.Handle, tableTemplate);
					break;
				case ICadTableEntryTemplate tableEntryTemplate:
					this.tableEntryTemplates.Add(tableEntryTemplate.CadObject.Handle, tableEntryTemplate);
					break;
				default:
					this.cadObjectsTemplates.Add(template.CadObject.Handle, template);
					break;
			}
		}

		public virtual void BuildDocument()
		{
			foreach (ICadTableEntryTemplate template in this.tableEntryTemplates.Values)
			{
				template.Build(this);
			}

			foreach (CadTemplate template in this.cadObjectsTemplates.Values)
			{
				template.Build(this);
			}
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

		public void BuildTables()
		{
			this.BuildTable(this.AppIds);
			this.BuildTable(this.TextStyles);
			this.BuildTable(this.LineTypesTable);
			this.BuildTable(this.Layers);
			this.BuildTable(this.UCSs);
			this.BuildTable(this.Views);
			this.BuildTable(this.BlockRecords);
			this.BuildTable(this.DimensionStyles);
			this.BuildTable(this.VPorts);
		}

		public T GetObjectTemplate<T>(ulong handle) where T : CadTemplate
		{
			if (this.templatesMap.TryGetValue(handle, out ICadObjectTemplate template))
			{
				return (T)template;
			}

			return null;
		}

		public void Notify(string message, NotificationType notificationType = NotificationType.None, Exception exception = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, notificationType, exception));
		}

		public void RegisterTables()
		{
			this.DocumentToBuild.RegisterCollection(this.AppIds);
			this.DocumentToBuild.RegisterCollection(this.TextStyles);
			this.DocumentToBuild.RegisterCollection(this.LineTypesTable);
			this.DocumentToBuild.RegisterCollection(this.Layers);
			this.DocumentToBuild.RegisterCollection(this.UCSs);
			this.DocumentToBuild.RegisterCollection(this.Views);
			this.DocumentToBuild.RegisterCollection(this.BlockRecords);
			this.DocumentToBuild.RegisterCollection(this.DimensionStyles);
			this.DocumentToBuild.RegisterCollection(this.VPorts);
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

				if (obj is UnknownNonGraphicalObject && !this.KeepUnknownNonGraphicalObjects)
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

		public bool TryGetTableEntry<T>(string name, out T entry)
			where T : TableEntry
		{
			//Only to be used when the tables are build
			if (string.IsNullOrEmpty(name))
			{
				entry = null;
				return false;
			}

			Table<T> table = null;
			if (typeof(T) == typeof(AppId))
			{
				table = this.AppIds as Table<T>;
			}
			else if (typeof(T) == typeof(Layer))
			{
				table = this.Layers as Table<T>;
			}
			else if (typeof(T) == typeof(LineType))
			{
				table = this.LineTypesTable as Table<T>;
			}
			else if (typeof(T) == typeof(UCS))
			{
				table = this.UCSs as Table<T>;
			}
			else if (typeof(T) == typeof(View))
			{
				table = this.Views as Table<T>;
			}
			else if (typeof(T) == typeof(DimensionStyle))
			{
				table = this.DimensionStyles as Table<T>;
			}
			else if (typeof(T) == typeof(TextStyle))
			{
				table = this.TextStyles as Table<T>;
			}
			else if (typeof(T) == typeof(VPortsTable))
			{
				table = this.VPorts as Table<T>;
			}
			else if (typeof(T) == typeof(BlockRecord))
			{
				table = this.BlockRecords as Table<T>;
			}

			if (table == null)
			{
				entry = null;
				return false;
			}

			return table.TryGetValue(name, out entry);
		}

		protected void buildDictionaries()
		{
			foreach (ICadDictionaryTemplate dictionaryTemplate in dictionaryTemplates.Values)
			{
				dictionaryTemplate.Build(this);
			}

			this.DocumentToBuild.UpdateCollections(true);
		}

		protected void createMissingHandles()
		{
			foreach (var template in this.unassignedObjects)
			{
				template.CadObject.Handle = this.InitialHandSeed + 1;
				this.AddTemplate(template);
			}

			this.unassignedObjects.Clear();
		}

		protected void registerTable<T, R>(T table)
			where T : Table<R>
			where R : TableEntry
		{
			if (table == null)
			{
				this.DocumentToBuild.RegisterCollection((T)Activator.CreateInstance(typeof(T)));
			}
			else
			{
				this.DocumentToBuild.RegisterCollection(table);
			}
		}

		private bool addToMap(ICadObjectTemplate template)
		{
			if (template.CadObject.Handle == 0)
			{
				this.unassignedObjects.Add(template);
				return false;
			}

			if (this.templatesMap.ContainsKey(template.CadObject.Handle))
			{
				this.Notify($"Repeated handle found {template.CadObject.Handle}.", NotificationType.Warning);
				template.CadObject.Handle = 0;
				this.unassignedObjects.Add(template);
				return false;
			}

			if (template.CadObject.Handle > this.InitialHandSeed)
			{
				this.InitialHandSeed = template.CadObject.Handle;
			}

			this.templatesMap.Add(template.CadObject.Handle, template);
			this.cadObjects.Add(template.CadObject.Handle, template.CadObject);
			return true;
		}
	}
}