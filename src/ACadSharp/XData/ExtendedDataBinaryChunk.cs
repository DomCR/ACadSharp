namespace ACadSharp.XData
{
	public class ExtendedDataBinaryChunk : ExtendedDataRecord<byte[]>
	{
		public ExtendedDataBinaryChunk(byte[] chunk) : base(DxfCode.ExtendedDataBinaryChunk, chunk) { }
	}
}
