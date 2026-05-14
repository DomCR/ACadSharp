using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadMaterialTemplate : CadTemplate<Material>
	{
		public CadMaterialTemplate() : base(new Material()) { }

		public CadMaterialTemplate(Material material) : base(material) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);
		}
	}
}
