using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadAttributeTemplate : CadTextEntityTemplate
	{
		public CadTextEntityTemplate MTextTemplate { get; set; }

		public CadAttributeTemplate(AttributeBase entity) : base(entity) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.MTextTemplate != null)
			{
				this.MTextTemplate.Build(builder);
			}
		}
	}
}
