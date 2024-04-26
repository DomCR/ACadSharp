using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadTextEntityTemplate : CadEntityTemplate
	{
		public ulong? StyleHandle { get; set; }

		public string StyleName { get; set; }

		public CadTextEntityTemplate(Entity entity) : base(entity) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			IText text = (IText)this.CadObject;

			if (this.getTableReference(builder, this.StyleHandle, this.StyleName, out TextStyle style))
			{
				text.Style = style;
			}
		}
	}
}
