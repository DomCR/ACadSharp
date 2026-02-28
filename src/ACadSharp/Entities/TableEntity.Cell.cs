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

			[Flags]
			public enum VirtualEdgeFlags
			{
				None = 0,
				Top = 1,
				Right = 2,
				Bottom = 4,
				Left = 8
			}

			public enum CellAlignmentType
			{
				None = 0,
				TopLeft = 1,
				TopCenter = 2,
				TopRight = 3,
				MiddleLeft = 4,
				MiddleCenter = 5,
				MiddleRight = 6,
				BottomLeft = 7,
				BottomCenter = 8,
				BottomRight = 9,
			}

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