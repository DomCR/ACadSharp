namespace ACadSharp.XData
{
	public class ExtendedDataInteger16 : ExtendedDataRecord<short>
	{
		public ExtendedDataInteger16(short value) : base(DxfCode.ExtendedDataInteger16, value) { }
	}
}
