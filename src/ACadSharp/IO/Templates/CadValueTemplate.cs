namespace ACadSharp.IO.Templates;

internal class CadValueTemplate : ICadTemplate
{
	public CadValue CadValue { get; }

	public ulong? ValueHandle { get; set; }

	public CadValueTemplate(CadValue value)
	{
		this.CadValue = value;
	}

	public void Build(CadDocumentBuilder builder)
	{
		if (builder.TryGetCadObject<CadObject>(this.ValueHandle, out var cadObject))
		{
			this.CadValue.SetValue(cadObject, CadValueType.Handle);
		}
	}
}