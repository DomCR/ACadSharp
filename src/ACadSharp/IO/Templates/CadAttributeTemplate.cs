using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadAttributeTemplate : CadTextEntityTemplate
	{
		public CadTextEntityTemplate MTextTemplate { get; set; }

		public CadAttributeTemplate(AttributeBase entity) : base(entity) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.MTextTemplate != null)
			{
				this.MTextTemplate.Build(builder);
			}
		}
	}
}
