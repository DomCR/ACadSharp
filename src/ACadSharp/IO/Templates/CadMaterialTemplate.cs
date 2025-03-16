using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadMaterialTemplate : CadTemplate<Material>
	{
		public CadMaterialTemplate() : base(new Material()) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}
}
