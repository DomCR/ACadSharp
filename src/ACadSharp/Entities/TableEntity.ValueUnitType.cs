namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public enum ValueUnitType
		{
			/// <summary>
			/// No units.
			/// </summary>
			NoUnits = 0,
			/// <summary>
			/// Distance.
			/// </summary>
			Distance = 1,
			/// <summary>
			/// Angle.
			/// </summary>
			Angle = 2,
			/// <summary>
			/// Area.
			/// </summary>
			Area = 4,
			/// <summary>
			/// Volumne.
			/// </summary>
			Volume = 8,
			/// <summary>
			/// Currency.
			/// </summary>
			Currency = 0x10,
			/// <summary>
			/// Percentage.
			/// </summary>
			Percentage = 0x20
		}
	}
}
