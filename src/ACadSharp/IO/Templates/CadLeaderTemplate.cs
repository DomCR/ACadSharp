using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadLeaderTemplate : CadEntityTemplate<Leader>
	{
		public CadLeaderTemplate() : base(new Leader()) { }

		public CadLeaderTemplate(Leader entity) : base(entity) { }

		public double Dimasz { get; set; }

		public ulong DIMSTYLEHandle { get; set; }

		public string DIMSTYLEName { get; set; }

		public ulong AnnotationHandle { get; set; }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			Leader leader = (Leader)this.CadObject;

			if (this.getTableReference(builder, this.DIMSTYLEHandle, this.DIMSTYLEName, out DimensionStyle style))
			{
				leader.Style = style;
			}

			if (builder.TryGetCadObject(this.AnnotationHandle, out Entity annotation))
			{
				leader.AssociatedAnnotation = annotation;
			}
		}
	}
}
