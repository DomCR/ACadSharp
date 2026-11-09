using System.Collections.Generic;
using System.IO;

namespace ACadSharp.DataStorage;

internal class DataField : FileSegment
{
	public List<DataEntry> DataEntries { get; set; } = new();

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

	public class DataEntry
	{
		public DataBlobReference BlobReference { get; set; }

		public byte[] Data { get; set; }

		public uint DataSize { get; set; }

		public DataHeader Header { get; set; }

		public byte[] GetData()
		{
			if (this.Data != null)
			{
				return this.Data;
			}
			else if (this.BlobReference != null)
			{
				MemoryStream stream = new MemoryStream();
				foreach (Blob01 blob in this.BlobReference.Blobs)
				{
					stream.Write(blob.Data, 0, blob.Data.Length);
				}

				return stream.ToArray();
			}
			else
			{
				return null;
			}
		}
	}

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