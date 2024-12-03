namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public enum CellValueType
		{
			/// <summary>
			/// Unknown
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// 32 bit Long value
			/// </summary>
			Long = 1,
			/// <summary>
			/// Double value
			/// </summary>
			Double = 2,
			/// <summary>
			/// String value
			/// </summary>
			String = 4,
			/// <summary>
			/// Date value
			/// </summary>
			Date = 8,
			/// <summary>
			/// 2D point value
			/// </summary>
			Point2D = 0x10,
			/// <summary>
			/// 3D point value
			/// </summary>
			Point3D = 0x20,
			/// <summary>
			/// Object handle value
			/// </summary>
			Handle = 0x40,
			/// <summary>
			/// Buffer value
			/// </summary>
			Buffer = 0x80,
			/// <summary>
			/// Result buffer value
			/// </summary>
			ResultBuffer = 0x100,
			/// <summary>
			/// General
			/// </summary>
			General = 0x200
		}
	}
}
