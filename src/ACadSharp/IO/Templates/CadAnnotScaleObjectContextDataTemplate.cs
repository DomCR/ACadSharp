using ACadSharp.Objects;

namespace ACadSharp.IO.Templates {
	internal class CadAnnotScaleObjectContextDataTemplate : CadNonGraphicalObjectTemplate
	{
		public CadAnnotScaleObjectContextDataTemplate(AnnotScaleObjectContextData cadObject)
			: base(cadObject)
		{
		}

		public ulong ScaleHandle { get; internal set; }

		protected override void build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			AnnotScaleObjectContextData contextData = (AnnotScaleObjectContextData)this.CadObject;
			if (builder.TryGetCadObject(this.ScaleHandle, out Scale scale))
			{
				contextData.Scale = scale;
			}
		}
	}
}
