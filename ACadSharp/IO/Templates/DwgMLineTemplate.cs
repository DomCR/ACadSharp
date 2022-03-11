using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class DwgMLineTemplate : CadEntityTemplate
	{
		public ulong MLineStyleHandle { get; set; }

		public DwgMLineTemplate(MLine mline) : base(mline) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			MLine mLine = this.CadObject as MLine;

			if (this.MLineStyleHandle > 0
				&& builder.TryGetCadObject<MLStyle>(this.MLineStyleHandle, out MLStyle style))
			{
				mLine.Style = style;
			}
		}
	}
}
