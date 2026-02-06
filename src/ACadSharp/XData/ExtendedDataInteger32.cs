namespace ACadSharp.XData
{
	public class ExtendedDataInteger32 : ExtendedDataRecord<int>
	{
		public ExtendedDataInteger32(int value) : base(DxfCode.ExtendedDataInteger32, value) { }
	}
}
