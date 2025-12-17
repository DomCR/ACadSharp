namespace ACadSharp.XData
{
	public class ExtendedDataString : ExtendedDataRecord<string>
	{
		public ExtendedDataString(string value) : base(DxfCode.ExtendedDataAsciiString, value)
		{
		}
	}
}
