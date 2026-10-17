using System.Collections.Generic;

namespace ACadSharp.Entities
{
public partial class TableEntity
{
	/// <summary>
	/// R2007 (AC1021) 及更早版本表格实体的表格级覆盖数据。
	/// <para>
	/// DWG 规范 20.4.101：pre-R2010 表格在所有单元格数据之后依次跟随 4 个可选覆盖段
	/// （表格样式覆盖 / 边框颜色覆盖 / 边框线宽覆盖 / 边框可见性覆盖），
	/// 每段以一个 B "Has xxx overrides" 标志位开头。单元格的边框覆盖属性会索引到
	/// 表格级覆盖数组，若写出时丢弃这些段，AutoCAD/ODA 将报 "needs recovery"。
	/// R2010+ 起表格内容为内联 TABLECONTENT 格式，不再使用这些段。
	/// </para>
	/// </summary>
	public sealed class TableLegacyOverrides
	{
		/// <summary>表格样式覆盖段（flags 位定义同 <see cref="TableOverrideFlags"/>）。</summary>
		public sealed class StyleOverrideData
		{
			/// <summary>BL 90 覆盖标志位，指明后续哪些字段存在。</summary>
			public int Flags { get; set; }

			/// <summary>B 280 标题行抑制（0x0001 置位时存在）。</summary>
			public bool TitleSuppressed { get; set; }

			/// <summary>BS 70 流向（0x0004 置位时存在）。</summary>
			public short FlowDirection { get; set; }

			/// <summary>BD 40 水平单元格边距（0x0008 置位时存在）。</summary>
			public double HorizontalCellMargin { get; set; }

			/// <summary>BD 41 垂直单元格边距（0x0010 置位时存在）。</summary>
			public double VerticalCellMargin { get; set; }

			/// <summary>CMC 64/65/66 标题/表头/数据行颜色（按 0x0020/0x0040/0x0080 顺序）。</summary>
			public List<Color> RowColors { get; } = new List<Color>();

			/// <summary>B 283 标题/表头/数据行填充无效标志（按 0x0100/0x0200/0x0400 顺序）。</summary>
			public List<bool> RowFillNone { get; } = new List<bool>();

			/// <summary>CMC 63 标题/表头/数据行填充颜色（按 0x0800/0x1000/0x2000 顺序）。</summary>
			public List<Color> RowFillColors { get; } = new List<Color>();

			/// <summary>BS 170 标题/表头/数据行对齐（按 0x4000/0x8000/0x10000 顺序）。</summary>
			public List<short> RowAlignments { get; } = new List<short>();

			/// <summary>H 7 标题/表头/数据行文字样式句柄（按 0x20000/0x40000/0x80000 顺序）。</summary>
			public List<ulong> TextStyleHandles { get; } = new List<ulong>();

			/// <summary>BD 140 标题/表头/数据行高（按 0x100000/0x200000/0x400000 顺序）。</summary>
			public List<double> RowHeights { get; } = new List<double>();
		}

		/// <summary>边框覆盖段通用结构（0-17 位，每个置位对应一个值，按位序排列）。</summary>
		public sealed class BorderOverrideData
		{
			/// <summary>BL 94/95/96 覆盖标志位。</summary>
			public int Flags { get; set; }

			/// <summary>CMC 边框颜色值（仅颜色覆盖段使用）。</summary>
			public List<Color> Colors { get; } = new List<Color>();

			/// <summary>BS 边框线宽值（仅线宽覆盖段使用）。</summary>
			public List<short> LineWeights { get; } = new List<short>();

			/// <summary>BS 边框可见性值（仅可见性覆盖段使用）。</summary>
			public List<bool> Visibility { get; } = new List<bool>();
		}

		/// <summary>表格样式覆盖段，null 表示 "Has table overrides" 位为 0。</summary>
		public StyleOverrideData StyleOverride { get; set; }

		/// <summary>边框颜色覆盖段，null 表示该段标志位为 0。</summary>
		public BorderOverrideData BorderColors { get; set; }

		/// <summary>边框线宽覆盖段，null 表示该段标志位为 0。</summary>
		public BorderOverrideData BorderLineWeights { get; set; }

		/// <summary>边框可见性覆盖段，null 表示该段标志位为 0。</summary>
		public BorderOverrideData BorderVisibility { get; set; }
	}

	/// <summary>
	/// 表格级 legacy 覆盖数据（仅 R2007 及更早 DWG 使用）；
	/// null 表示源文件中 4 个覆盖段标志位均为 0。
	/// </summary>
	public TableLegacyOverrides LegacyOverrides { get; set; }
}
}
