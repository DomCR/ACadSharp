namespace ACadSharp.XData
{
	/// <summary>
	/// Interface to define <see cref="ExtendedDataRecord"/> that are referencing another <see cref="CadObject"/>.
	/// </summary>
	public interface IExtendedDataHandleReference
	{
		/// <summary>
		/// ulong value that points to a <see cref="IHandledCadObject.Handle"/>.
		/// </summary>
		ulong Value { get; }

		/// <summary>
		/// Resolve the reference of for an specific <see cref="CadDocument"/>.
		/// </summary>
		/// <param name="document"></param>
		/// <returns></returns>
		CadObject ResolveReference(CadDocument document);
	}
}
