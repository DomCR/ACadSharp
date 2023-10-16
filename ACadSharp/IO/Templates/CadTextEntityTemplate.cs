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

			TextStyle style = null;

			switch (this.CadObject)
			{
				case TextEntity text:
					if (builder.TryGetCadObject<TextStyle>(StyleHandle, out style))
					{
						text.Style = style;
					}
					break;
				case MText mtext:
					if (builder.TryGetCadObject<TextStyle>(StyleHandle, out style))
					{
						mtext.Style = style;
					}
					break;
				default:
					throw new System.ArgumentException("Unknown type");
			}
		}
	}
}
