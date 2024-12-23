namespace ACadSharp.XData
{
	public class ExtendedDataControlString : ExtendedDataRecord
	{
		public string Value
		{
			get { return this._value as string; }
		}

		public ExtendedDataControlString(bool isClosing) : base(DxfCode.ExtendedDataControlString, isClosing ? '}' : '{')
		{
		}
	}
}
