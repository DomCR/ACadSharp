using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionAssociationTemplate : CadTemplate<DimensionAssociation>
	{
		public ulong? DimensionHandle { get; set; }

		public ulong? GeometryHandle { get; set; }

		public CadDimensionAssociationTemplate() : base(new())
		{
		}

		public CadDimensionAssociationTemplate(DimensionAssociation obj) : base(obj)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (builder.TryGetCadObject<Dimension>(this.DimensionHandle, out var dimension))
			{
			}

			if (builder.TryGetCadObject<CadObject>(this.GeometryHandle, out var geom))
			{
			}
		}
	}
}