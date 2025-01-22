namespace ACadSharp.XData
{
	public class ExtendedDataControlString : ExtendedDataRecord
	{
		public static ExtendedDataControlString Open { get { return new ExtendedDataControlString(false); } }

		public static ExtendedDataControlString Close { get { return new ExtendedDataControlString(true); } }

		public bool IsClosing { get; set; }

		public char Value { get { return IsClosing ? '}' : '{'; } }

		public ExtendedDataControlString(bool isClosing) : base(DxfCode.ExtendedDataControlString, isClosing ? '}' : '{')
		{
			this.IsClosing = isClosing;
		}
	}
}
