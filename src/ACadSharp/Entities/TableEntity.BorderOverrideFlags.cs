using System;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	[Flags]
	internal enum BorderOverrideFlags
	{
		None = 0,
		TitleHorizontalTop = 0x01,
		TitleHorizontalInsert = 0x02,
		TitleHorizontalBottom = 0x04,
		TitleVerticalLeft = 0x8,
		TitleVerticalInsert = 0x10,
		TitleVerticalRight = 0x20,
		HeaderHorizontalTop = 0x40,
		HeaderHorizontalInsert = 0x80,
		HeaderHorizontalBottom = 0x100,
		HeaderVerticalLeft = 0x200,
		HeaderVerticalInsert = 0x400,
		HeaderVerticalRight = 0x800,
		DataHorizontalTop = 0x1000,
		DataHorizontalInsert = 0x2000,
		DataHorizontalBottom = 0x4000,
		DataVerticalLeft = 0x8000,
		DataVerticalInsert = 0x10000,
		DataVerticalRight = 0x20000,
	}
}