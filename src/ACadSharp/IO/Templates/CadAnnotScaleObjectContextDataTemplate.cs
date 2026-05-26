using ACadSharp.Objects;

namespace ACadSharp.IO.Templates;

internal class CadAnnotScaleObjectContextDataTemplate : CadNonGraphicalObjectTemplate
{
	public ulong? ScaleHandle { get; internal set; }

	public CadAnnotScaleObjectContextDataTemplate(AnnotScaleObjectContextData cadObject)
				: base(cadObject)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		AnnotScaleObjectContextData contextData = (AnnotScaleObjectContextData)this.CadObject;
		if (builder.TryGetCadObject(this.ScaleHandle, out Scale scale))
		{
			contextData.Scale = scale;
		}
	}
}