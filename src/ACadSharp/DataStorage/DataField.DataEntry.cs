using System.IO;

namespace ACadSharp.DataStorage;

internal partial class DataField
{
	public class DataEntry
	{
		public DataBlobReference BlobReference { get; set; }

		public byte[] Data { get; set; }

		public uint DataSize { get; set; }

		public DataHeader Header { get; set; } = new DataHeader();

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

		public override string ToString()
		{
			return $"{this.Header}";
		}
	}
}