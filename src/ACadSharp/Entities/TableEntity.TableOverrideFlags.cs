using System;

namespace ACadSharp.Entities
{
public partial class TableEntity
	{
		[Flags]
		internal enum TableOverrideFlags
		{
			None = 0,
			TitleSuppressed = 0x0001,
			HeaderSuppressed = 0x02,
			FlowDirection = 0x0004,
			HorizontalCellMargin = 0x0008,
			VerticalCellMargin = 0x0010,

			TitleRowColor = 0x0020,
			HeaderRowColor = 0x00040,
			DataRowColor = 0x0080,

			TitleRowFillNone = 0x0100,
			HeaderRowFillNone = 0x0200,
			DataRowFillNone = 0x0400,

			TitleRowFillColor = 0x0800,
			HeaderRowFillColor = 0x1000,
			DataRowFillColor = 0x2000,

			TitleRowAlign = 0x4000,
			HeaderRowAlign = 0x8000,
			DataRowAlign = 0x10000,

			TitleTextStyle = 0x20000,
			HeaderTextStyle = 0x40000,
			DataTextStyle = 0x80000,

			TitleRowHeight = 0x100000,
			HeaderRowHeight = 0x200000,
			DataRowHeight = 0x400000,
		}
	}
}