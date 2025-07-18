using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadPdfUnderlayTemplate : CadEntityTemplate<PdfUnderlay>
	{
		public ulong? DefinitionHandle { get; set; }

		public CadPdfUnderlayTemplate() : base(new PdfUnderlay()) { }

		public CadPdfUnderlayTemplate(PdfUnderlay entity) : base(entity)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.DefinitionHandle, out PdfUnderlayDefinition definition))
			{
				this.CadObject.Definition = definition;
			}
			else
			{
				builder.Notify($"UnderlayDefinition not found for {this.CadObject.Handle}", NotificationType.Warning);
			}
		}
	}
}
