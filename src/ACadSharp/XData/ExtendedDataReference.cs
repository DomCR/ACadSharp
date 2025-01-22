namespace ACadSharp.XData
{
	public abstract class ExtendedDataReference<T> : ExtendedDataRecord<ulong>, IExtendedDataHandleReference
		where T : CadObject
	{
		protected ExtendedDataReference(DxfCode code, ulong handle) : base(code, handle) { }

		/// <summary>
		/// Resolve the reference of for an specific <see cref="CadDocument"/>.
		/// </summary>
		/// <param name="document"></param>
		/// <returns></returns>
		public T ResolveReference(CadDocument document)
		{
			return document.GetCadObject<T>(this.Value);
		}

		/// <inheritdoc/>
		CadObject IExtendedDataHandleReference.ResolveReference(CadDocument document)
		{
			return this.ResolveReference(document);
		}
	}
}
