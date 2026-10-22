using System.Collections.Generic;

namespace ACadSharp.Entities
{
public partial class TableEntity
{
	/// <summary>
	/// Table-level override data for table entities in R2007 (AC1021) and earlier versions.
	/// <para>
	/// DWG spec 20.4.101: for pre-R2010 tables, 4 optional override sections follow after all the
	/// cell data in sequence (table style overrides / border color overrides / border line weight overrides /
	/// border visibility overrides),
	/// each section starting with a B "Has xxx overrides" flag bit. A cell's border override properties index into
	/// the table-level override arrays; if these sections are dropped when writing, AutoCAD/ODA will report "needs recovery".
	/// From R2010+ the table content is in the inline TABLECONTENT format and these sections are no longer used.
	/// </para>
	/// </summary>
	public sealed class TableLegacyOverrides
	{
		/// <summary>Table style override section (flags bits defined as in <see cref="TableOverrideFlags"/>).</summary>
		public sealed class StyleOverrideData
		{
			/// <summary>BL 90 override flag bits, indicating which following fields are present.</summary>
			public int Flags { get; set; }

			/// <summary>B 280 title row suppression (present when 0x0001 is set).</summary>
			public bool TitleSuppressed { get; set; }

			/// <summary>BS 70 flow direction (present when 0x0004 is set).</summary>
			public short FlowDirection { get; set; }

			/// <summary>BD 40 horizontal cell margin (present when 0x0008 is set).</summary>
			public double HorizontalCellMargin { get; set; }

			/// <summary>BD 41 vertical cell margin (present when 0x0010 is set).</summary>
			public double VerticalCellMargin { get; set; }

			/// <summary>CMC 64/65/66 title/header/data row colors (in the order 0x0020/0x0040/0x0080).</summary>
			public List<Color> RowColors { get; } = new List<Color>();

			/// <summary>B 283 title/header/data row fill-none flags (in the order 0x0100/0x0200/0x0400).</summary>
			public List<bool> RowFillNone { get; } = new List<bool>();

			/// <summary>CMC 63 title/header/data row fill colors (in the order 0x0800/0x1000/0x2000).</summary>
			public List<Color> RowFillColors { get; } = new List<Color>();

			/// <summary>BS 170 title/header/data row alignments (in the order 0x4000/0x8000/0x10000).</summary>
			public List<short> RowAlignments { get; } = new List<short>();

			/// <summary>H 7 title/header/data row text style handles (in the order 0x20000/0x40000/0x80000).</summary>
			public List<ulong> TextStyleHandles { get; } = new List<ulong>();

			/// <summary>BD 140 title/header/data row heights (in the order 0x100000/0x200000/0x400000).</summary>
			public List<double> RowHeights { get; } = new List<double>();
		}

		/// <summary>Common structure for border override sections (bits 0-17, each set bit corresponds to one value, arranged in bit order).</summary>
		public sealed class BorderOverrideData
		{
			/// <summary>BL 94/95/96 override flag bits.</summary>
			public int Flags { get; set; }

			/// <summary>CMC border color values (used only by the color override section).</summary>
			public List<Color> Colors { get; } = new List<Color>();

			/// <summary>BS border line weight values (used only by the line weight override section).</summary>
			public List<short> LineWeights { get; } = new List<short>();

			/// <summary>BS border visibility values (used only by the visibility override section).</summary>
			public List<bool> Visibility { get; } = new List<bool>();
		}

		/// <summary>Table style override section; null means the "Has table overrides" bit is 0.</summary>
		public StyleOverrideData StyleOverride { get; set; }

		/// <summary>Border color override section; null means this section's flag bit is 0.</summary>
		public BorderOverrideData BorderColors { get; set; }

		/// <summary>Border line weight override section; null means this section's flag bit is 0.</summary>
		public BorderOverrideData BorderLineWeights { get; set; }

		/// <summary>Border visibility override section; null means this section's flag bit is 0.</summary>
		public BorderOverrideData BorderVisibility { get; set; }
	}

	/// <summary>
	/// Table-level legacy override data (used only by R2007 and earlier DWG);
	/// null means all 4 override section flag bits are 0 in the source file.
	/// </summary>
	public TableLegacyOverrides LegacyOverrides { get; set; }
}
}
