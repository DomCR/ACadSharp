using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.DataStorage;

internal class SchemaIndex : FileSegment
{
	public List<Pointer> PropertyPointers { get; set; } = new();

	public List<string> SchemaNames { get; set; } = new();

	public List<Pointer> SchemaPointers { get; set; } = new();

	public IEnumerable<Pointer> GetAllPointers()
	{
		return this.PropertyPointers.Concat(this.SchemaPointers);
	}

	public IEnumerable<uint> GetSchemaIndexes()
	{
		return this.SchemaPointers.Select(p => p.SegmentIndex).Distinct();
	}	

	public class Pointer
	{
		public uint Index { get; set; }

		public uint LocalOffset { get; set; }

		public uint SegmentIndex { get; set; }

		public override string ToString()
		{
			return $"Index: {this.Index} | SegmentIndex: {this.SegmentIndex} | LocalOffset: {this.LocalOffset}";
		}
	}
}
