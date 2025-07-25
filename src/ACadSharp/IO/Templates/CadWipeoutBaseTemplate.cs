﻿using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadWipeoutBaseTemplate : CadEntityTemplate
	{
		public ulong? ImgDefHandle { get; set; }

		public ulong? ImgReactorHandle { get; set; }

		public CadWipeoutBaseTemplate(CadWipeoutBase image) : base(image) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			CadWipeoutBase image = this.CadObject as CadWipeoutBase;

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
