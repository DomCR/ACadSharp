using System;

namespace ACadSharp.XData
{
	public abstract class ExtendedDataReference<T> : ExtendedDataRecord<ulong>
		where T : CadObject
	{
		protected ExtendedDataReference(DxfCode code, ulong handle) : base(code, handle) { }

		public T ResolveReference(CadDocument document)
		{
			throw new NotImplementedException();
		}
	}
}
