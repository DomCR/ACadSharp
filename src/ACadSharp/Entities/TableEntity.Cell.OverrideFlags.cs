using System;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public partial class Cell
		{
			[Flags]
			internal enum OverrideFlags
			{
				None = 0,
				CellAlignment = 0x01,
				BackgroundFillNone = 0x02,
				BackgroundColor = 0x04,
				ContentColor = 0x08,
				TextStyle = 0x10,
				TextHeight = 0x20,

				TopGridColor = 0x00040,
				TopGridLineWeight = 0x00400,
				TopVisibility = 0x04000,

				RightGridColor = 0x00080,
				RightGridLineWeight = 0x00800,
				RightVisibility = 0x08000,

				BottomGridColor = 0x00100,
				BottomGridLineWeight = 0x01000,
				BottomVisibility = 0x10000,

				LeftGridColor = 0x00200,
				LeftGridLineWeight = 0x02000,
				LeftVisibility = 0x20000,
			}
		}
	}
}