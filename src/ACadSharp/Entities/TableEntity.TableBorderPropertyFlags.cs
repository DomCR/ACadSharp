namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		[System.Flags]
		public enum TableBorderPropertyFlags
		{
			/// <summary>
			/// None.
			/// </summary>
			None = 0x0,
			/// <summary>
			/// Border type.
			/// </summary>
			BorderType = 0x1,
			/// <summary>
			/// Line weight.
			/// </summary>
			LineWeight = 0x2,
			/// <summary>
			/// Line type.
			/// </summary>
			LineType = 0x4,
			/// <summary>
			/// Color.
			/// </summary>
			Color = 0x8,
			/// <summary>
			/// Invisibility.
			/// </summary>
			Invisibility = 0x10,
			/// <summary>
			/// Double line spacing.
			/// </summary>
			DoubleLineSpacing = 0x20,
			/// <summary>
			/// All.
			/// </summary>
			All = 0x3F
		}
	}
}
