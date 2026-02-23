namespace ACadSharp.IO.Templates;

internal class CadValueTemplate : ICadTemplate
{
	public CadValue CadValue { get; }

	public ulong? ValueHandle { get; set; }

	public CadValueTemplate(CadValue value)
	{
		CadValue = new CadValue();
	}

	public void Build(CadDocumentBuilder builder)
	{
		if (builder.TryGetCadObject<CadObject>(ValueHandle, out var cadObject))
		{
			this.CadValue.CadObject = cadObject;
		}
	}
}