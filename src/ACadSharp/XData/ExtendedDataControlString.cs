namespace ACadSharp.XData
{
	public class ExtendedDataControlString : ExtendedDataRecord<char>
	{
		public ExtendedDataControlString(bool isClosing) : base(DxfCode.ExtendedDataControlString, isClosing ? '}' : '{')
		{
		}
	}
}
