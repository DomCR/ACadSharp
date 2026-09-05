using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadDictionaryTemplate : CadTemplate<CadDictionary>, ICadDictionaryTemplate
	{
		public Dictionary<string, ulong?> Entries { get; set; } = new Dictionary<string, ulong?>();

		public CadDictionaryTemplate() : base(new CadDictionary()) { }

		public CadDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.OwnerHandle.HasValue
				&& this.OwnerHandle == 0
				&& builder.DocumentToBuild.RootDictionary == null)
			{
				if (builder is DwgDocumentBuilder dwgBuilder
					&& this.CadObject.Handle == dwgBuilder.HeaderHandles.DICTIONARY_NAMED_OBJECTS)
				{
					builder.DocumentToBuild.RootDictionary = this.CadObject;
				}
				else
				{
					builder.DocumentToBuild.RootDictionary = this.CadObject;
				}
			}

			foreach (var item in this.Entries)
			{
				if (builder.TryGetCadObject(item.Value, out NonGraphicalObject entry))
				{
					//This section basically sets the key and name to the same value to make sure that the 
					//different collections and dictionaries work properly.
					//For some collections like Scales there are some cases where the key doesn't match the name
					//but regarding changing the key doesn't seem to take an effect on it.
					if (string.IsNullOrEmpty(entry.Name))
					{
						entry.Name = item.Key;
					}

					//The entry goes in under its own name, which is what the collections built on
					//top of a dictionary look it up by. Some names are constants, though - every
					//SortEntitiesTable is called ACAD_SORTENTS - so when that name is already
					//taken the file's own key is used instead. Without this, a drawing that keeps
					//several such objects in one dictionary loses all but the first: one customer
					//drawing lost 447 draw order tables that way.
					string key = string.IsNullOrEmpty(entry.Name) ? item.Key : entry.Name;
					if (this.CadObject.ContainsKey(key))
					{
						key = item.Key;
					}

					try
					{
						this.CadObject.Add(key, entry);
					}
					catch (System.Exception ex)
					{
						builder.Notify($"Error when trying to add the entry {entry.Name} to {this.CadObject.Name}|{this.CadObject.Handle}", NotificationType.Error, ex);
					}
				}
				else
				{
					builder.Notify($"Entry not found {item.Key}|{item.Value} for dictionary {this.CadObject.Name}|{this.CadObject.Handle}", NotificationType.Warning);
				}
			}
		}
	}
}
