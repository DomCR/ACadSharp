using System;

namespace ACadSharp.XData
{
	public abstract class ExtendedDataReference<T> : ExtendedDataRecord<ulong>, IExtendedDataHandleReference
		where T : CadObject
	{
		protected ExtendedDataReference(DxfCode code, ulong handle) : base(code, handle) { }

		public T ResolveReference(CadDocument document)
		{
			return document.GetCadObject<T>(this.Value);
		}

		CadObject IExtendedDataHandleReference.ResolveReference(CadDocument document)
		{
			return this.ResolveReference(document);
		}
	}
}
