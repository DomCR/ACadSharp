using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal partial class DataField
{
	public class DataBlobReference
	{
		public uint LastPageSize { get; set; }

		public uint PageCount { get; set; }

		public uint PageSize { get; set; }

		public uint RecordSize { get; set; }

		public ulong TotalDataSize { get; set; }

		public uint Unknown1 { get; set; }

		public uint Unknown2 { get; set; }

		public List<Blob01> Blobs { get; set; } = new();
	}
}