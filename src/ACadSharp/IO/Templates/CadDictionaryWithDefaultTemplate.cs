using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadDictionaryWithDefaultTemplate : CadDictionaryTemplate
	{
		public ulong? DefaultEntryHandle { get; set; }

		public CadDictionaryWithDefaultTemplate() : base(new CadDictionaryWithDefault()) { }

		public CadDictionaryWithDefaultTemplate(CadDictionaryWithDefault dictionary) : base(dictionary) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (builder.TryGetCadObject(DefaultEntryHandle, out CadObject entry))
			{
				((CadDictionaryWithDefault)this.CadObject).DefaultEntry = entry;
			}
		}
	}
}
