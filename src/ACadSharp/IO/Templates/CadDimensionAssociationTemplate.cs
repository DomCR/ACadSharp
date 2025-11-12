using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionAssociationTemplate : CadTemplate<DimensionAssociation>
	{
		public CadDimensionAssociationTemplate() : base(new()) { }

		public CadDimensionAssociationTemplate(DimensionAssociation obj) : base(obj) { }
	}
}
