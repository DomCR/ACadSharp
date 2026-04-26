using ACadSharp.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public partial class Cell
		{
			/// <summary>
			/// Boolean flag indicating if the auto fit option is set for the cell.
			/// </summary>
			[DxfCodeValue(174)]
			public bool AutoFit { get; set; }

			[DxfCodeValue(144)]
			public double BlockScale { get; set; }

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

			/// <summary>
			/// Gets the single cell content if available; otherwise, returns null.
			/// </summary>
			/// <remarks>If the cell contains multiple content items or no content, this property returns null. Use this
			/// property when you expect the cell to have at most one content item.</remarks>
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

			/// <summary>
			/// Gets the collection of cell contents contained in this instance.
			/// </summary>
			public List<CellContent> Contents { get; } = new();

			/// <summary>
			/// Gets or sets the custom data value associated with this cell.
			/// </summary>
			[DxfCodeValue(91)]
			public int CustomData { get; set; }

			/// <summary>
			/// Gets or sets the collection of custom data entries associated with this instance.
			/// </summary>
			public List<CustomDataEntry> CustomDataCollection { get; set; } = new();

			/// <summary>
			/// Cell flag value.
			/// </summary>
			[DxfCodeValue(172)]
			public short EdgeFlags { get; set; }

			/// <summary>
			/// Gets or sets the geometric layout information for the cell content.
			/// </summary>
			public CellContentGeometry Geometry { get; set; }

			/// <summary>
			/// Gets or sets a value indicating whether the entity has linked data associated with it.
			/// </summary>
			[DxfCodeValue(92)]
			public bool HasLinkedData { get; set; }

			/// <summary>
			/// Gets a value indicating whether the instance contains more than one content item.
			/// </summary>
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

			/// <summary>
			/// Gets or sets the state flags that describe the current state of the table cell.
			/// </summary>
			[DxfCodeValue(90)]
			public TableCellStateFlags StateFlags { get; set; }

			public CellStyle StyleOverride { get; set; } = new CellStyle();

			public CellStyle Style { get; set; }

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