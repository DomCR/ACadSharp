using System.Collections.Generic;

namespace ACadSharp.DataStorage;

internal class CadFileDataStorage
{
	public List<AcdsRecord> Records { get; } = new();

	public List<AcdsSchema> Schemes { get; } = new();

	public const string AsmData = "ASM_Data";

	public const string Id = "AcDbDs::ID";
}

internal class FileSegment
{
	public FileSegmentHeader Header { get; set; }

	public override string ToString()
	{
		return $"{this.Header.Name}";
	}
}

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

internal class SchemaIndex : FileSegment
{
	public List<Pointer> PropertyPointers { get; set; } = new();

	public List<string> SchemaNames { get; set; } = new();

	public List<Pointer> SchemaUnknownPropertyPointer { get; set; } = new();

	public uint UnknownIndex1 { get; set; }

	public long UnknownMagic { get; set; }

	public List<Pointer> UnknownPropertyPointers { get; set; } = new();

	public class Pointer
	{
		public uint Index { get; set; }

		public uint Offset { get; set; }

		public uint SchemaIndex { get; set; }

		public override string ToString()
		{
			return $"Index: {this.Index} | SchemaIndex: {this.SchemaIndex} | Offset: {this.Offset}";
		}
	}
}

internal class FileHeader
{
	public int DataIndexSegmentIndex { get; set; }

	public int DataStorageRevision { get; set; }

	public int FileHeaderSize { get; set; }

	public uint FileSignature { get; set; }

	public int FileSize { get; set; }

	public int PreviousSaveIndex { get; set; }

	public int SchemaIndexSegmentIndex { get; set; }

	public int SearchSegmentIndex { get; set; }

	public int SegmentIndexEntryCount { get; set; }

	public int SegmentIndexOffset { get; set; }

	public int SegmentIndexUnknown { get; set; }

	public int Unknown1 { get; set; } = 2;

	public int Unknown2 { get; set; } = 0;

	public int Version { get; set; } = 2;
}