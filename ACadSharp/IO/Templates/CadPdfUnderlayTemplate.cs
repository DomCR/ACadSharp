using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadPdfUnderlayTemplate : CadEntityTemplate<PdfUnderlay>
	{
		public ulong? DefinitionHandle { get; set; }

		public CadPdfUnderlayTemplate(PdfUnderlay entity) : base(entity)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.DefinitionHandle, out CadObject definition))
			{
				//this.CadObject.Definition = definition;
			}
		}
	}
}
