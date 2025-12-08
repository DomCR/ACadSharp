using ACadSharp.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class Cell
		{
			/// <summary>
			/// Boolean flag indicating if the auto fit option is set for the cell.
			/// </summary>
			[DxfCodeValue(174)]
			public bool AutoFit { get; set; }

			/// <summary>
			/// Cell border height.
			/// </summary>
			/// <remarks>
			/// Applicable only for merged cells.
			/// </remarks>
			[DxfCodeValue(176)]
			public int BorderHeight { get; set; }

			/// <summary>
			/// Cell border width.
			/// </summary>
			/// <remarks>
			/// Applicable only for merged cells.
			/// </remarks>
			[DxfCodeValue(175)]
			public int BorderWidth { get; set; }

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

			[DxfCodeValue(91)]
			public int CustomData { get; set; }

			public List<CustomDataEntry> CustomDataCollection { get; set; } = new();

			/// <summary>
			/// Cell flag value.
			/// </summary>
			[DxfCodeValue(172)]
			public short EdgeFlags { get; set; }

			public CellContentGeometry Geometry { get; set; }

			[DxfCodeValue(92)]
			public bool HasLinkedData { get; set; }

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

			/// <summary>
			/// Cell merged value.
			/// </summary>
			[DxfCodeValue(173)]
			public short MergedValue { get; set; }

			/// <summary>
			/// Rotation value.
			/// </summary>
			/// <remarks>
			/// Applicable for a block-type cell and a text-type cell.
			/// </remarks>
			[DxfCodeValue(145)]
			public double Rotation { get; set; }

			[DxfCodeValue(144)]
			public double BlockScale { get; set; }

			[DxfCodeValue(90)]
			public TableCellStateFlags StateFlags { get; set; }

			public CellStyle StyleOverride { get; set; } = new();

			[DxfCodeValue(300)]
			public string ToolTip { get; set; }

			/// <summary>
			/// Cell type.
			/// </summary>
			[DxfCodeValue(171)]
			public CellType Type { get; set; }

			/// <summary>
			/// Flag value for a virtual edge.
			/// </summary>
			[DxfCodeValue(178)]
			public short VirtualEdgeFlag { get; set; }
		}
	}
}