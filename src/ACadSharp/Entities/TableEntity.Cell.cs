using ACadSharp.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class Cell
		{
			/// <summary>
			/// Cell type.
			/// </summary>
			[DxfCodeValue(171)]
			public CellType Type { get; set; }

			/// <summary>
			/// Cell flag value.
			/// </summary>
			[DxfCodeValue(172)]
			public int FlagValue { get; set; }

			/// <summary>
			/// Cell merged value.
			/// </summary>
			[DxfCodeValue(173)]
			public int MergedValue { get; set; }

			/// <summary>
			/// Boolean flag indicating if the autofit option is set for the cell.
			/// </summary>
			[DxfCodeValue(174)]
			public bool Autofit { get; set; }

			/// <summary>
			/// Cell border width.
			/// </summary>
			/// <remarks>
			/// Applicable only for merged cells.
			/// </remarks>
			[DxfCodeValue(175)]
			public int BorderWidth { get; set; }

			/// <summary>
			/// Cell border height.
			/// </summary>
			/// <remarks>
			/// Applicable only for merged cells.
			/// </remarks>
			[DxfCodeValue(176)]
			public int BorderHeight { get; set; }

			/// <summary>
			/// Flag value for a virtual edge.
			/// </summary>
			[DxfCodeValue(178)]
			public short VirtualEdgeFlag { get; set; }

			/// <summary>
			/// Rotation value.
			/// </summary>
			/// <remarks>
			/// Applicable for a block-type cell and a text-type cell.
			/// </remarks>
			[DxfCodeValue(145)]
			public double Rotation { get; set; }

			[DxfCodeValue(300)]
			public string ToolTip { get; set; }

			[DxfCodeValue(90)]
			public TableCellStateFlags StateFlags { get; set; }

			[DxfCodeValue(92)]
			public bool HasLinkedData { get; set; }

			public CellStyle StyleOverride { get; set; } = new();

			public int CustomData { get; set; }

			public List<CustomDataEntry> CustomDataCollection { get; set; } = new();

			public bool HasMultipleContent
			{
				get
				{
					if (this.Contents == null)
					{
						return false;
					}

					return this.Contents.Count > 1;
				}
			}

			public CellContent Content
			{
				get
				{
					if (this.Contents == null || this.HasMultipleContent)
					{
						return null;
					}
					else
					{
						return this.Contents.FirstOrDefault();
					}
				}
			}

			public List<CellContent> Contents { get; } = new();

			public CellContentGeometry Geometry { get; set; }
		}
	}
}
