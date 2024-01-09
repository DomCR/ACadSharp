using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadLeaderTemplate : CadEntityTemplate
	{
		public CadLeaderTemplate(Leader entity) : base(entity) { }

		public double Dimasz { get; internal set; }

		public ulong DIMSTYLEHandle { get; internal set; }

		public ulong AnnotationHandle { get; internal set; }


		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Leader leader = (Leader)this.CadObject;

			if (this.getTableReference(builder, this.DIMSTYLEHandle, string.Empty, out DimensionStyle style)) {
				leader.Style = style;
			}
			if (builder.TryGetCadObject(this.AnnotationHandle, out Entity annotation)) {
				leader.AssociatedAnnotation = annotation;
			}
		}
	}
}
