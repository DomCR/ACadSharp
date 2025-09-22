using System;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		[Flags]
		public enum MarginFlags
		{
			None = 0,
			Override = 1,
		}
	}
}
