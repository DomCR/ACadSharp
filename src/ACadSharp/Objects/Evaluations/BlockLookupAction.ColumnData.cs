using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

public partial class BlockLookupAction
{
	public class ColumnData
	{
		public string ConnectionName { get; set; }

		public bool IsLookupProperty { get; set; }

		public bool IsReadOnly { get; set; }

		public int NodeId { get; set; }

		public List<string> Rows { get; private set; } = new();

		public int Type { get; set; }

		public string UnmatchedName { get; set; }

		public int ValueType { get; set; }
	}
}