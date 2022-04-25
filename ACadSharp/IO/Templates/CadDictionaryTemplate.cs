using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadDictionaryTemplate : CadTemplate<CadDictionary>
	{
		public Dictionary<string, ulong?> Entries { get; set; } = new Dictionary<string, ulong?>();

		public CadDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.OwnerHandle.HasValue && this.OwnerHandle == 0)
			{
				builder.DocumentToBuild.RootDictionary = this.CadObject;
			}
			//else if (builder.TryGetCadObject(this.OwnerHandle, out CadObject co))
			//{
			//	co.XDictionary = this.CadObject;
			//}

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
