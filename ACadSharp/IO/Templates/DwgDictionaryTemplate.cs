using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgDictionaryTemplate : DwgTemplate
	{
		public Dictionary<ulong, string> HandleEntries { get; set; } = new Dictionary<ulong, string>();
		public DwgDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }

		public override void Build(CadDocumentBuilder builder)
		{
			//TODO: DwgDictionaryTemplate.Build

			base.Build(builder);

			if (this.OwnerHandle.HasValue && this.OwnerHandle == 0)
			{

			}

			builder.NotificationHandler?.Invoke(this.CadObject, new NotificationEventArgs($"Dictionary setup not implemented"));
		}
	}
}
