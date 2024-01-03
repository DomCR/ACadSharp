using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadToleranceTemplate : CadEntityTemplate<Tolerance>
	{
		public ulong? DimensionStyleHandle { get; set; }

		public CadToleranceTemplate() : base(new Tolerance()) { }

		public CadToleranceTemplate(Tolerance tolerance) : base(tolerance) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.getTableReference(builder, DimensionStyleHandle, null, out DimensionStyle style))
			{
				this.CadObject.Style = style;
			}
		}
	}
}
