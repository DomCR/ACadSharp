using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class FileSegmentIndex : FileSegment
{
	public List<Entry> Entries { get; } = new();

	public void AddEntry(ulong offset, uint size)
	{
		this.Entries.Add(new Entry { Offset = offset, Size = size });
	}

	public class Entry
	{
		public ulong Offset { get; set; }

		public uint Size { get; set; }

		public override string ToString()
		{
			return $"Offset: {this.Offset} | Size: {this.Size}";
		}
	}
}
