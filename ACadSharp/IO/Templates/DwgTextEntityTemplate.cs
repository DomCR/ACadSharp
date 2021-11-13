using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class DwgTextEntityTemplate : DwgEntityTemplate
	{
		public ulong StyleHandle { get; set; }

		public DwgTextEntityTemplate(Entity entity) : base(entity) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			switch (this.CadObject)
			{
				case TextEntity text:
					text.TextStyle = builder.GetCadObject<Style>(this.StyleHandle);
					break;
				case MText mtext:
					mtext.TextStyle = builder.GetCadObject<Style>(this.StyleHandle);
					break;
				default:
					throw new System.ArgumentException("Unknown type");
			}
		}
	}
}
