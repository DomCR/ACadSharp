namespace ACadSharp.XData
{
	public class ExtendedDataHandle : ExtendedDataReference<CadObject>
	{
		public ExtendedDataHandle(ulong handle) : base(DxfCode.ExtendedDataHandle, handle) { }
	}
}
