using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadDictionaryTemplate : CadTemplate<CadDictionary>
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
				if (builder is DwgDocumentBuilder dwgBuilder)
				{
					//There is no way to find the root dictionary, dwg does not provide an explicit handle for it
					//the only way to check for the root dictionary is to try to find the handles that belong to it
					List<ulong?> rootHandles = new() 
					{
						dwgBuilder.HeaderHandles.DICTIONARY_ACAD_GROUP,
						dwgBuilder.HeaderHandles.DICTIONARY_ACAD_MLINESTYLE,
						dwgBuilder.HeaderHandles.DICTIONARY_COLORS,
						dwgBuilder.HeaderHandles.DICTIONARY_LAYOUTS,
						dwgBuilder.HeaderHandles.DICTIONARY_MATERIALS,
						dwgBuilder.HeaderHandles.DICTIONARY_NAMED_OBJECTS,
						dwgBuilder.HeaderHandles.DICTIONARY_PLOTSETTINGS,
						dwgBuilder.HeaderHandles.DICTIONARY_PLOTSTYLES,
						dwgBuilder.HeaderHandles.DICTIONARY_VISUALSTYLE,
					};

					foreach (ulong handle in rootHandles.Where(h => h.HasValue).Select(v => (ulong)v))
					{
						if (this.Entries.ContainsValue(handle))
						{
							builder.DocumentToBuild.RootDictionary = this.CadObject;
							break;
						}
					}
				}
				else
				{
					builder.DocumentToBuild.RootDictionary = this.CadObject;
				}
			}

			foreach (var item in this.Entries)
			{
				if (builder.TryGetCadObject(item.Value, out CadObject entry))
				{
					this.CadObject.Add(item.Key, entry);
				}
			}
		}
	}
}
