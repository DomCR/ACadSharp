using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadTextEntityTemplate : CadEntityTemplate
	{
		public ulong StyleHandle { get; set; }

		public CadTextEntityTemplate(Entity entity) : base(entity) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			switch (dxfcode)
			{
				//Multiple options
				case 280:
					//return true;
				default:
					return false;
			}
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			switch (this.CadObject)
			{
				case TextEntity text:
					text.Style = builder.GetCadObject<TextStyle>(this.StyleHandle);
					break;
				case MText mtext:
					mtext.Style = builder.GetCadObject<TextStyle>(this.StyleHandle);
					break;
				default:
					throw new System.ArgumentException("Unknown type");
			}
		}
	}
}
