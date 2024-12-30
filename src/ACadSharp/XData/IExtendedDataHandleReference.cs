namespace ACadSharp.XData
{
	public interface IExtendedDataHandleReference
	{
		ulong Value { get; }

		CadObject ResolveReference(CadDocument document);
	}
}
