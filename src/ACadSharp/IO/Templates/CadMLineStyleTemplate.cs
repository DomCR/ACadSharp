using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadMLineStyleTemplate : CadTemplate<MLineStyle>
	{
		public List<ElementTemplate> ElementTemplates { get; set; } = new List<ElementTemplate>();

		public CadMLineStyleTemplate() : base(new()) { }

		public CadMLineStyleTemplate(MLineStyle mlStyle) : base(mlStyle)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (var item in this.ElementTemplates)
			{
				item.Build(builder);
			}
		}
	}
}