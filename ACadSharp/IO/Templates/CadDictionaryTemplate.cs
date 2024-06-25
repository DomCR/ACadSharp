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

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

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
					if (string.IsNullOrEmpty(entry.Name))
					{
						entry.Name = item.Key;
					}

					try
					{
						this.CadObject.Add(item.Key, entry);
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
