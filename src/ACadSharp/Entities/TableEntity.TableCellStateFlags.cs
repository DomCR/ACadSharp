namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		[System.Flags]
		public enum TableCellStateFlags
		{
			/// <summary>
			/// None
			/// </summary>
			None = 0x0,

			/// <summary>
			/// Content  locked
			/// </summary>
			ContentLocked = 0x1,

			/// <summary>
			/// Content read only
			/// </summary>
			ContentReadOnly = 0x2,

			/// <summary>
			/// Linked.
			/// </summary>
			Linked = 0x4,

			/// <summary>
			/// Content modifed after update
			/// </summary>
			ContentModifiedAfterUpdate = 0x8,

			/// <summary>
			/// Format locked
			/// </summary>
			FormatLocked = 0x10,

			/// <summary>
			/// Format readonly
			/// </summary>
			FormatReadOnly = 0x20,

			/// <summary>
			/// Format was modified after update
			/// </summary>
			FormatModifiedAfterUpdate = 0x40,
		}
	}
}
