namespace ACadSharp.XData
{
	public class ExtendedDataControlString : ExtendedDataRecord
	{
		public bool IsClosing { get; set; }

		public char Value { get { return IsClosing ? '}' : '{'; } }

		public ExtendedDataControlString(bool isClosing) : base(DxfCode.ExtendedDataControlString, isClosing ? '}' : '{')
		{
			this.IsClosing = isClosing;
		}
	}
}
