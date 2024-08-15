using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadImageTemplate : CadEntityTemplate
	{
		public ulong? ImgDefHandle { get; set; }

		public ulong? ImgReactorHandle { get; set; }

		public CadImageTemplate(CadImageBase image) : base(image) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			CadImageBase image = this.CadObject as CadImageBase;

			if (builder.TryGetCadObject(this.ImgDefHandle, out ImageDefinition imgDef))
			{
				image.Definition = imgDef;
			}

			if (builder.TryGetCadObject(this.ImgReactorHandle, out ImageDefinitionReactor imgReactor))
			{
				image.DefinitionReactor = imgReactor;
			}
		}
	}
}
