namespace ACadSharp.DataStorage;

internal partial class DataField
{
	public class DataHeader
	{
		public ulong Handle { get; set; }

		public uint LocalOffset { get; set; }

		public uint Size { get; set; }

		public override string ToString()
		{
			return $"Handle {this.Handle} | LocalOffset {this.LocalOffset}";
		}
	}
}